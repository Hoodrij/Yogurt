# ð¥ Yogurt

**Async Managed Entity-Component Framework for Unity**

Someday there will be a fight between ECS and Yogurt approach ð

---

# Installation

1. Open **Unity Package Manager**
2. Click *â* button â â*Add package from git URLâ¦*â
3. Enter **https://github.com/Hoodrij/Yogurt.git**

---

# Overview

### ð·ï¸ Entity

An Entity is nothing more than storage for components. 

```csharp
Entity entity = Entity.Create();

Assert.IsTrue(entity != Entity.Null);
Assert.IsTrue(entity != default);
Assert.IsTrue(entity.Exist);

entity.Kill();

Assert.IsFalse(entity.Exist);
```

### ð·ï¸ Component

Components are classes that contain data.

```csharp
public class Health : IComponent
{
    public int Value;
}
```

Entity has a bunch of methods to operate with components.

```csharp
entity.Add<Health>();
entity.Add(new Health());

entity.Has<Health>();
entity.Set(new Health());
entity.Remove<Health>();

entity.Get<Health>();
entity.TryGet(out Health health);
```

### ð·ï¸ Aspect

Aspect is an Entity with a defined set of Components. Used to speed up the interaction with Entity.

```csharp
public struct PlayerAspect : Aspect<PlayerAspect>
{
    public Entity Entity { get; set; }
    
    public PlayerTag Tag => this.Get<PlayerTag>();
    public Health Health => this.Get<Health>();
    public Transform Transform => this.Get<Transform>();
        
    public NestedAspect NestedAspect => this.Get<NestedAspect>();
}

PlayerAspect playerAspect = entity.ToAspect<PlayerAspect>();
playerAspect.Health.Value -= 1;
playerAspect.Add<OtherComponent>();
playerAspect.Kill();
```

### ð·ï¸ Query

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

### ð·ï¸ Entity hierarchy

Entity provides few methods to combine them into a Parent-Child relationship. All Childs will be killed after a Parent death.

```csharp
entity.SetParent(parentEntity);
entity.UnParent();
```

### ð·ï¸ Debug

You can access all the Entities list with full meta like this

```csharp
new Yogurt.Debug().Entities;
```

You can Execute this at debug mode right inside of you IDE.

To enable debug logging add YOGURT_DEBUG to your [Scripting Define Symbols](https://docs.unity3d.com/Manual/CustomScriptingSymbols.html)

---

# Examples

- **Roguelike** sample project

[https://github.com/Hoodrij/Yogurt-Roguelike](https://github.com/Hoodrij/Yogurt-Roguelike)