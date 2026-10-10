# Roguelike jobs

Turn-based roguelike on a grid. The level entity owns every object in it.

## The model

```csharp
public enum TeamKind
{
    Survivors,
    Zombies,
}

public struct Team : IComponent { public TeamKind Value; }
public struct Health : IComponent { public int Value; }
public struct Position : IComponent { public Vector2Int Value; }
public struct Turn : IComponent { public int Order; }
public struct TurnOwner : IComponent { }
public struct Player : IComponent { }
public struct Zombie : IComponent { }
public struct Food : IComponent { public int Value; }
public struct Wall : IComponent { }
public struct Exit : IComponent { }
public struct Level : IComponent { public int Number; }

public record struct AgentAspect(Entity Entity) : IAspect
{
    public ref Team Team => ref this.Get<Team>();
    public ref Health Health => ref this.Get<Health>();
    public ref Position Position => ref this.Get<Position>();
    public ref Turn Turn => ref this.Get<Turn>();
    public AgentView View => this.Get<AgentView>();
}

public record struct PlayerAspect(Entity Entity) : IAspect
{
    public AgentAspect Agent => this.As<AgentAspect>();
    public ref Player Player => ref this.Get<Player>();
}

public record struct ZombieAspect(Entity Entity) : IAspect
{
    public AgentAspect Agent => this.As<AgentAspect>();
    public ref Zombie Zombie => ref this.Get<Zombie>();
}

public record struct FoodAspect(Entity Entity) : IAspect
{
    public ref Food Food => ref this.Get<Food>();
    public ref Position Position => ref this.Get<Position>();
}

public record struct LevelAspect(Entity Entity) : IAspect
{
    public ref Level Level => ref this.Get<Level>();
    public LevelView View => this.Get<LevelView>();
}
```

## Create an object

```csharp
public struct CreateZombieJob
{
    public async UniTask<ZombieAspect> Run(LevelAspect level, Vector2Int cell)
    {
        LevelConfig config = Query.Single<LevelConfig>();
        AgentView view = await config.Zombie.Spawn(level.View.Board);

        ZombieAspect zombie = Entity.Create()
            .Link(view.gameObject)
            .Add(view)
            .Add(new Zombie())
            .Add(new Team { Value = TeamKind.Zombies })
            .Add(new Health { Value = config.ZombieHealth })
            .Add(new Position { Value = cell })
            .Add(new Turn { Order = Query.Of<AgentAspect>().Count() })
            .SetParent(level.Entity)
            .As<ZombieAspect>();

        view.Show(zombie.Agent);
        return zombie;
    }
}
```

## Read the world

```csharp
public struct GetEnemyAtJob
{
    public AgentAspect Run(AgentAspect agent, Vector2Int cell)
    {
        foreach (AgentAspect other in Query.Of<AgentAspect>())
        {
            if (other.Position.Value == cell && other.Team.Value != agent.Team.Value)
            {
                return other;
            }
        }

        return default;
    }
}

public struct IsCellFreeJob
{
    public bool Run(Vector2Int cell)
    {
        foreach (Entity entity in Query.Of<Position>().Without<Food>().Without<Exit>())
        {
            if (entity.Get<Position>().Value == cell)
            {
                return false;
            }
        }

        return true;
    }
}
```

A `default` aspect wraps `Entity.Null`; callers check `Exist()`.

## Pass the turn

```csharp
public struct PassTurnJob
{
    public AgentAspect Run()
    {
        AgentAspect current = Query.Of<AgentAspect>().With<TurnOwner>().Single();
        int currentOrder = current.Exist() ? current.Turn.Order : -1;

        AgentAspect first = default;
        AgentAspect next = default;
        foreach (AgentAspect agent in Query.Of<AgentAspect>())
        {
            if (!first.Exist() || agent.Turn.Order < first.Turn.Order)
            {
                first = agent;
            }

            if (agent.Turn.Order > currentOrder && (!next.Exist() || agent.Turn.Order < next.Turn.Order))
            {
                next = agent;
            }
        }

        if (!next.Exist())
        {
            next = first;
        }

        if (current.Exist())
        {
            current.Remove<TurnOwner>();
        }

        next.Add(new TurnOwner());
        return next;
    }
}
```

## Do a step

```csharp
public struct StepJob
{
    public async UniTask Run(AgentAspect agent, Vector2Int cell)
    {
        AgentAspect enemy = new GetEnemyAtJob().Run(agent, cell);
        if (enemy.Exist())
        {
            await new AttackJob().Run(agent, enemy);
            return;
        }

        if (!new IsCellFreeJob().Run(cell))
        {
            return;
        }

        await new MoveAgentJob().Run(agent, cell);
        if (agent.Has<Player>())
        {
            new EnterCellJob().Run(agent.As<PlayerAspect>());
        }
    }
}
```

```csharp
public struct AttackJob
{
    public async UniTask Run(AgentAspect attacker, AgentAspect target)
    {
        await attacker.View.PlayAttack(target.Position.Value);
        await new DamageAgentJob().Run(target, 1);
    }
}

public struct DamageAgentJob
{
    public async UniTask Run(AgentAspect target, int damage)
    {
        target.Health.Value -= damage;
        target.View.Show(target);
        if (target.Health.Value > 0)
        {
            return;
        }

        await target.View.PlayDeath();
        target.Kill();
    }
}
```

```csharp
public struct MoveAgentJob
{
    public async UniTask Run(AgentAspect agent, Vector2Int cell)
    {
        agent.Position.Value = cell;
        await agent.View.PlayMove(cell);
    }
}

public struct EnterCellJob
{
    public void Run(PlayerAspect player)
    {
        foreach (FoodAspect food in Query.Of<FoodAspect>())
        {
            if (food.Position.Value == player.Agent.Position.Value)
            {
                new EatFoodJob().Run(player, food);
            }
        }

        if (new IsExitAtJob().Run(player.Agent.Position.Value))
        {
            new ReachExitJob().Run();
        }
    }
}
```

```csharp
public struct EatFoodJob
{
    public void Run(PlayerAspect player, FoodAspect food)
    {
        player.Agent.Health.Value += food.Food.Value;
        player.Agent.View.Show(player.Agent);
        food.Kill();
    }
}
```

## Zombie step

```csharp
public struct GetZombieStepJob
{
    public Vector2Int Run(ZombieAspect zombie)
    {
        PlayerAspect player = Query.Single<PlayerAspect>();
        Vector2Int from = zombie.Agent.Position.Value;
        Vector2Int to = player.Agent.Position.Value;
        Vector2Int delta = to - from;

        return Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? from + new Vector2Int(Math.Sign(delta.x), 0)
            : from + new Vector2Int(0, Math.Sign(delta.y));
    }
}
```

