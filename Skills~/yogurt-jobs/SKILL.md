---
name: yogurt-jobs
description: Writes Yogurt jobs with process thinking: one struct per complete process that owns what it starts. Use when writing or changing any `*Job`, adding a game rule, splitting logic, or building the game flow.
---

# Yogurt jobs

## Pillar: process thinking

OOP puts objects at the center, and they grow into `PlayerController`, `GameManager`, `DamageHandler`, `GridUtils`. Yogurt puts the process at the center. A job is a process with a name, a start, an end and an owner:

- it owns the objects it creates (sets their parent, kills them or lets the parent do it),
- it owns the processes it starts (awaits them, or binds them to a Life),
- objects own nothing and do nothing.

Design a feature by asking "what happens, in which order, until when?", name each process with a verb, write it as a job, then pass it the main object and let it find the rest.

| OOP habit | Yogurt |
|---|---|
| `PlayerController.Update()` | `WaitForPlayerStepJob` → `MoveAgentJob` |
| `HealthManager.Damage()` | `DamageAgentJob` |
| static `GridUtils.IsFree()` | `IsCellFreeJob` |
| `OnDeath` event + handlers | the killing job calls the next job, or someone awaits `agent.Life()` |
| state machine | `RunGameJob` awaiting `RunMenuJob`, then `RunLevelJob` |

A process is made of smaller processes: cooking soup is boiling water, chopping carrots, adding them. Jobs form their own tree of processes. It does not mirror the entity tree: a process can own no entity, and an entity can have no process. A process stops when its owner job ends or its owner Life dies.

## Shape

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

`public struct`, no fields, one public `Run`, one job per file, called as `new EatFoodJob().Run(player, food)`.

## A finished job

1. Does all its name promises and nothing more.
2. Takes the objects it works on as aspects, plus values the caller chose. Never values pulled out of an object it receives, never a component, never argument structs or tuples.
3. Finds the rest itself: aspect properties, `Query.Single` for singletons and configs, a tag query for "the current" object.
4. Leaves data, views and entities consistent: changing a visible value also updates the view; a damage job also handles death.
5. Owns what it starts: every created entity has a parent, every started job is awaited or bound to a Life.
6. `Run` reads like the game rules. Named game steps become jobs, small unnamed steps become local functions.

## Return types

`void` (instant change), a value (`Get…Job`, `Is…Job`, no side effects), `UniTask` (takes frames), `UniTask<T>` (takes frames, returns a result, e.g. `Create…Job` returns its aspect). Never `async void`, `Task` or `UniTaskVoid`.

## Loops

No `while (true)`: a loop always has an owner. `owner.Run(step)` calls the step every frame while the owner exists; an async step runs to its end before the next one starts.

```csharp
public struct RunTurnsJob
{
    public void Run(LevelAspect level)
    {
        level.Run(MakeNextTurn);
        return;

        async UniTask MakeNextTurn()
        {
            AgentAspect agent = new PassTurnJob().Run();
            await new MakeTurnJob().Run(agent);
        }
    }
}
```

For real-time logic over many objects in a fixed order, one update job with `level.Run(step)` that loops a query is the Yogurt form of a system. Start such jobs in a fixed order in one place.

## Waits

A wait on a game condition gets a name (job or local function), not a long inline lambda. Bind it to the Life of its owner (`yogurt-life`).

```csharp
Vector2Int direction = default;
await Wait.Until(HasDirection, player.Life());

bool HasDirection()
{
    direction = new GetInputDirectionJob().Run();
    return direction != Vector2Int.zero;
}
```

## Variants

Pick behavior from data and tags, not from jobs stored in components or interfaces per kind:

```csharp
Vector2Int cell = agent.Has<Player>()
    ? await new WaitForPlayerStepJob().Run(agent.As<PlayerAspect>())
    : new GetZombieStepJob().Run(agent.As<ZombieAspect>());
```

## Splitting

New job when a step has its own name in the game, is reused, or waits. Too big: name needs "And", `Run` exceeds a screen. Too small: only forwards to one job, or is named after the component it sets (`SetHealthJob`).

## References

- `references/examples.md`: roguelike jobs (create, pass turn, step, attack, damage, move, eat).
- `references/meta-flow.md`: menu → level → result → menu, phase entities and results.
