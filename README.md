# 🥛 Yogurt

**Async Managed Entity-Component Framework for Unity**


# Installation
UPM
```
https://github.com/Hoodrij/Yogurt.git
```

Yogurt requires C# 11 for best performance.

If a project cannot use C# 11, add `CSHARP_9` to the Scripting Define Symbols.

# Examples

- ⚔️ **Roguelike** sample project                                                      
    [https://github.com/Hoodrij/Yogurt-Roguelike](https://github.com/Hoodrij/Yogurt-Roguelike)
    
- 💣 **Arena** more complex sample project                                           
    [https://github.com/Hoodrij/Yogurt-Arena](https://github.com/Hoodrij/Yogurt-Arena)

# Overview

### 🏷️ Entity

An Entity is nothing more than storage for components. 

```csharp
Entity entity = Entity.Create();

Assert.IsTrue(entity != Entity.Null);
Assert.IsTrue(entity != default);
Assert.IsTrue(entity.Exist);

entity.Kill();

Assert.IsFalse(entity.Exist);
```

### 🏷️ Component

Components are types containing data.

```csharp
public class Health : IComponent
{
    public int Value;
}

public record struct HealthStruct(int Value) : IComponent;
```

Entity has a bunch of methods to operate with components.

```csharp
entity.Add(new Health());
entity.Set(new Health());
entity.Has<Health>();
entity.Remove<Health>();

ref entity.Get<Health>();
entity.TryGet(out Health health); // Class components only.
```

`TryGet` works only for class components. For a struct component, the out value would be a copy, and a change to it would be lost. For a struct, use `Has` and `ref Get`.

### 🏷️ Aspect

Aspect is an Entity with a defined set of Components. Used to speed up the interaction with Entity.

```csharp
public struct PlayerAspect : IAspect
{
    public Entity Entity { get; set; }
    
    public PlayerTag Tag => Entity.Get<PlayerTag>();
    public Health Health => Entity.Get<Health>();
    public Transform Transform => Entity.Get<Transform>();
        
    public NestedAspect NestedAspect => Entity.Get<NestedAspect>();
}

PlayerAspect player = anyEntity.As<PlayerAspect>();
player.Health.Value -= 1;
player.Add(new OtherComponent());
player.Exist();
player.Kill();
```

### 🏷️ Query

Query is used to get required Entities.

- Getting a Query
    
    ```csharp
    // Query of an Entity
    var query = Query.Of<Health>()
                     .With<PlayerTag>()
                     .Without<DeadTag>();
    
    // Or Query of an Aspect
    var query = Query.Of<PlayerAspect>();
    ```
    
- Operating with Query
    
    ```csharp
    // Iterate over
    foreach (Entity entity in query)
    {
    
    }
    
    // Or get Single
    Entity entity = query.Single();
    
    // Common IEnumerable methods
    query.Where(entity => entity.Get<Health>().Value > 50)
         .Any();
    ```
    
- Fast Single Query

```csharp
// Of Component
GameData data = Query.Single<GameData>();

// Or of an Aspect
PlayerAspect playerAspect = Query.Single<PlayerAspect>();
```

### 🏷️ Entity hierarchy

Entity provides few methods to combine them into a Parent-Child relationship. All Childs will be killed after a Parent death.

```csharp
entity.SetParent(parentEntity);
entity.UnParent();
```

# Unity integration

The `Yogurt.Unity` assembly connects entities to Unity objects. It is in the `Unity` folder and in the `Yogurt.Unity` namespace. The core `Yogurt` assembly does not use it.

### 🏷️ Wait

`Wait` gives conditions that you can await. A wait stops when its Life dies. A stopped wait throws `OperationCanceledException`.

```csharp
await Wait.Until(IsPlayerOnExit, level.Life());
await Wait.While(IsStunned, zombie.Life());
await Wait.Seconds(0.5f, zombie.Life());
await Wait.Update();
```

A wait without a Life stops when the application quits.

### 🏷️ Run

`Run` calls an action one time in each frame. It stops when the entity dies.

```csharp
projectile.Run(() => projectile.View.transform.position = projectile.Motion.Position);
```

### 🏷️ Link

`Link` connects a GameObject to an entity. When the entity dies, Yogurt despawns the GameObject.

```csharp
Entity zombie = Entity.Create().Link(view.gameObject).Add(view);
Entity clicked = hit.collider.GetEntity();
```

To remove a linked GameObject, kill its entity. Do not destroy a linked GameObject.

### 🏷️ Asset

`Asset<T>` is a prefab reference. `PooledAsset<T>` also keeps despawned instances and spawns them again.

```csharp
public class LevelConfig : ScriptableObject, IComponent
{
    public PooledAsset<AgentView> Zombie;
}

AgentView view = await config.Zombie.Spawn(level.View.Board);
view.Despawn();
```

`Despawn` puts a pooled instance back into its pool. It destroys an instance that has no pool.

### 🏷️ Blueprint

An `IBlueprint` adds components to an entity. `EntityBlueprint` is a component that keeps a blueprint on an entity.

```csharp
public class EnemyConfig : ScriptableObject, IBlueprint
{
    public int Health = 2;

    public void Populate(Entity entity)
    {
        entity.Add(new Zombie()).Add(new Health { Value = Health });
    }
}

Entity zombie = Entity.Create().PopulateFrom(enemyConfig);
```

### 🏷️ Debug

You can access all the Entities list with full meta like this

```csharp
new Yogurt.Debug().Entities;
```

You can Execute this at debug mode right inside of you IDE.

To enable debug logging add YOGURT_DEBUG to your [Scripting Define Symbols](https://docs.unity3d.com/Manual/CustomScriptingSymbols.html)
