---
name: yogurt
description: Core API, pillars and pitfalls of Yogurt, an entity-component framework for Unity. Use for any code with `using Yogurt`, `IComponent`, `IAspect`, `Query.Of`, `Entity.Create`, `Life` or `*Job` structs, and with the other yogurt-* skills.
---

# Yogurt

Target: indie games. In game code, readable beats fast. Needs C# 11 (`yogurt-setup`).

## Pillars

1. **Process thinking.** Jobs (structs with one `Run`) do all work and own the objects and processes they start. Objects only hold data. See `yogurt-jobs`.
2. **Aspects over entities.** Each entity type has a named aspect. See `yogurt-entity`.
3. **Life is a process.** A dead Life stops everything bound to it. See `yogurt-life`.
4. **Owners everywhere.** Each entity has a parent entity. Each job is awaited by a job or bound to a Life.

Views, assets, boot, waits: `yogurt-unity`.

## Rules

1. Aspects over entities. Parameters, return values, locals and component fields that point to an object use its aspect. A plain `Entity` only where any object fits. Never a component as a parameter.
2. Queries run only in jobs, never in aspects, components or views.
3. Change struct components through a reference (`ref` aspect property or `ref entity.Get<T>()`), never through a copy.
4. `Query.Single` only for a component that exists on exactly one entity.
5. Remove an object by killing it or its owner. Never `Destroy` a linked GameObject.
6. No `async void`, no `Task`, no `while (true)`. `.Forget()` only where a Life stops the work.
7. Work is a job. No managers, services, controllers, handlers, resolvers, helpers, static utility classes, event buses, DI. Extensions are fine: they add behavior to an existing type (`yogurt-entity`).
8. Default flow is jobs awaiting jobs. A per-frame update job over a query is fine when many objects must update in order (real-time movement).

## API

```csharp
Entity e = Entity.Create();
e.Add(new Health { Value = 10 });       // new component; YOGURT_DEBUG logs if it exists
e.Set(new Health { Value = 10 });       // add or replace
ref Health h = ref e.Get<Health>();
bool has = e.Has<Health>();
e.Remove<Health>();                     // removing the last component kills the entity
e.Kill();                               // also kills children
bool alive = e.Exist;
e.TryGet(out AgentView view);           // class components only

ZombieAspect zombie = Entity.Create()   // Add, Set, SetParent return the entity
    .Add(new Zombie())
    .Add(new Position { Value = cell })
    .SetParent(level.Entity)
    .As<ZombieAspect>();

public record struct AgentAspect(Entity Entity) : IAspect
{
    public ref Health Health => ref this.Get<Health>();   // struct component: ref
    public AgentView View => this.Get<AgentView>();       // class component
}
// generated on aspects: Get, Has, Add, Set, Remove, As, TryGet; AspectEx: Exist(), Kill(), Life(), SetParent

foreach (ZombieAspect z in Query.Of<ZombieAspect>().Without<Stunned>()) { }
foreach (Entity x in Query.Of<Position>().With<Food>()) { }
int n = Query.Of<ZombieAspect>().Count();   // also Any, None, First, AsEnumerable
PlayerAspect player = Query.Single<PlayerAspect>();
ref Progress progress = ref Query.Single<Progress>();

Life life = zombie.Life();              // dies with the entity; see yogurt-life
```

`Yogurt.Debug.Entities` lists all entities. `YOGURT_DEBUG` define adds error logs for wrong use.

## Pitfalls

1. `ref` locals are illegal in async methods. Change struct data through aspect properties there; an aspect stays valid across `await`.
2. Query order is not stable. Store order in a component when it matters.
3. `Single` returns the first match, or an aspect of `Entity.Null` when none. Check `Exist()` if absent is possible.
4. Every component-typed property of an aspect joins the aspect query. No computed component properties.
5. A query applies pending changes first. If a loop body kills/changes entities of the same query and then runs another query (often inside a called job), the outer loop can skip an entity. Only then copy to a list first; otherwise iterate directly.
6. Add each config to one entity, or `Query.Single` of it finds several.
7. The source generator registers components, aspects and literal queries. Never register manually.
8. Domain reload may be off: statics survive play sessions. No game state in statics.
