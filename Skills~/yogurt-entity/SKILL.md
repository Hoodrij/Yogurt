---
name: yogurt-entity
description: Designs Yogurt entities: aspects, components, tags, extensions, configs, owners and names. Use when adding a game object or writing or changing any IAspect or IComponent type.
---

# Yogurt entities

Entities store components with data. They do nothing themselves; jobs (`yogurt-jobs`) do the work.

## Aspects

An aspect is the type of an entity: a set of components that a query guarantees to be present. Jobs take and return aspects, because a named type explains the code and its `ref` properties edit struct components in place, even across `await`.

```csharp
public record struct AgentAspect(Entity Entity) : IAspect
{
    public ref Team Team => ref this.Get<Team>();
    public ref Health Health => ref this.Get<Health>();
    public ref Position Position => ref this.Get<Position>();
    public AgentView View => this.Get<AgentView>();
}

public record struct PlayerAspect(Entity Entity) : IAspect
{
    public AgentAspect Agent => this.As<AgentAspect>();
    public ref Player Player => ref this.Get<Player>();
}
```

- `record struct` with `Entity Entity` in the primary constructor. `this.Get<T>()`.
- `ref` property for struct components, plain property for class components.
- Only components the object always has, plus nested aspects. No methods, computed properties, optional components or queries: every component-typed property joins the query.
- Nesting means "is-a" (a player is an agent). For "has-a", make a separate entity. A chest that holds a sword is not a sword.
- Aspects over entities. A named aspect tells the reader what the object is; a plain `Entity` tells nothing. Parameters, return values, locals and component fields that point to an object use its aspect. Use a plain `Entity` only where any object fits, for example a parent.

```csharp
public struct Target : IComponent { public AgentAspect Value; }   // not Entity
```

## Components

```csharp
public struct Health : IComponent { public int Value; }
public struct Position : IComponent { public Vector2Int Value; }
public struct Player : IComponent { }                       // tag
```

- Struct by default. Class for views (MonoBehaviour), configs (ScriptableObject) and data shared by reference between entities.
- Fields only, no logic. One component per set of data that changes together. Single value: field `Value`.
- A tag (empty component) marks a role or state that a query must find. Add or remove it, instead of a `bool` field. "The current one" of many is a tag: `Query.Of<AgentAspect>().With<TurnOwner>().Single()`.

## Extensions

Components and aspects hold no logic. A small question about the data of one object, which many jobs ask, goes into an extension of its type. Do not copy the same local function into several jobs.

```csharp
public static class TeamEx
{
    public static bool IsEnemyOf(this in Team team, Team other) => team.Value != other.Value;
}

// or, with C# 14 extension blocks (they also allow properties and static members):
public static class TeamEx
{
    extension(in Team team)
    {
        public bool IsEnemyOf(Team other) => team.Value != other.Value;
    }
}
```

- One static class `<Type>Ex` for each extended type, as `AspectEx` and `QueryEx` in Yogurt.
- An extension reads the object it extends and returns a value. It does not run queries, change other objects or make a game step. These are jobs: `IsCellFreeJob` needs the other agents on the grid, so it is a job.

## Configs

ScriptableObject + `IComponent`, assets in `Assets/Resources/Configs`, loaded by a job onto the game entity (`yogurt-unity`). One entity per config. Never changed at run time: copy changing values into components.

## Owners

Each entity except the game entity has a parent (`SetParent`). Killing the parent kills its children and despawns their views. To end a level, kill the level entity.

```text
Game
├── Menu
└── Level
    └── Player, Zombie, Food, Exit
```

## Names

Game words only. Aspect `<Object>Aspect`, view `<Object>View`, config `<Object>Config`, components and tags are nouns or past participles (`Health`, `Stunned`), jobs are verb + object + `Job`. Extension classes are `<Type>Ex`. Never `Manager`, `Service`, `Controller`, `Handler`, `Resolver`, `Provider`, `System`, `Helper`, `Util`, `Data`, `Base`, `Impl`, `Component`. No single-implementation interfaces, no base classes for components or aspects.
