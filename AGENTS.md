# Yogurt: instructions for agents

Yogurt is an entity-component framework for Unity. Users install it as a UPM git package from https://github.com/Hoodrij/Yogurt.

Yogurt has its own git repository. The development copy is a submodule of the Yogurt-Polygon repository, at `Yogurt-Polygon-Unity/Assets/Yogurt`. Yogurt-Polygon is the test project. Its `AGENTS.md` tells you how to compile, test, measure and report. Read both files before you change Yogurt.

Do not commit or push. The user commits.

## 1. Goals

Use this order when two goals conflict. A goal with a lower number always wins.

1. **Simple API.** The public API stays as small and as simple as it is now. This is a hard constraint. Do not trade it for speed or memory.
2. **Correct behavior.** All unit tests pass. Never accept a speed gain that breaks a behavior.
3. **Fast hot paths.** Component access, queries and the flush must be fast. See section 6.
4. **Cheap structural changes.** Add, Remove and Kill of components must be cheap. The deferred flush pays the cost, not the call.
5. **Low memory.** Memory per entity and per group stays small. Steady-state code allocates nothing.
6. **Simple implementation.** Keep the code small. Remove code that has no caller.

Two more rules apply to all goals:

- Components can be classes or structs. Both must work and both must be fast. Do not add a feature that works for only one kind.
- Roslyn source generation is allowed. Use it when it moves work from run time to compile time, for example registrations, masks and warmups.

## 2. Public API

The public API is the contract with users. `README.md` shows it to users.

- Entity: `Entity.Create`, `Add`, `Set`, `Get` (returns `ref`), `TryGet`, `Has`, `Remove`, `Kill`, `Exist`, `SetParent`, `UnParent`, `Life`, `As<TAspect>`, `Entity.Null`.
- Component: any class or struct that implements `IComponent`.
- Aspect: a struct that implements `IAspect`. Generated extension methods give it `Get`, `TryGet`, `Add`, `Set`, `Has`, `Remove` and `As`.
- Query: `Query.Of<T>`, `With<T>`, `Without<T>`, `Single`, `Warmup`, `foreach`, `Query.Single<T>`. The `QueryEx` extensions give `Any`, `Count`, `First`, `None` and `AsEnumerable`.
- Life: `Life`, `Kill`, `IsAlive`, `AsToken`, `AsUniTask`, `Or`, `And`, `SetParent`.
- Debug: `Debug.Entities`. The `YOGURT_DEBUG` define enables error logs for invalid use.

Rules:

1. Do not add a step that users must do. Examples: a world object to pass, a system base class, a manual registration, a size limit to configure.
2. Do not remove, rename or change a public member without approval from the user.
3. Keep new types `internal`. Make a type `public` only when generated code or users must use it.
4. When a public API changes, update `README.md` in the same change.

## 3. Glossary

Each term has one meaning in this file and in the code.

| Term | Meaning |
|---|---|
| Component ID | A `ushort` index for one component type. `ComponentID.Of` gives it at first registration. IDs are domain-lifetime. |
| Mask | A bit set of component IDs in 64-bit words. Bit N is component ID N. |
| `Mask.Words` | The number of words in each entity mask. It grows when a new component type registers. |
| Slot | One entity record in `EntityMetas`: the `EntityMeta` struct, the component mask, then the pending mask. |
| Structural change | A change of the component set of an entity: Add of a new component, Remove, Kill. |
| Flush | `PostProcessor.Update`. It applies queued structural changes to groups. It runs before each query enumeration and each `Single`. |
| Composition | The included mask and the excluded mask of a query. It identifies one group. |
| Group | A sparse set of the entities that match one composition. |
| Query node | A cached `QueryNode` for one `With`/`Without` chain. It holds the composition and caches the group. |
| Storage | `Storage<T>`, the paged array of all values of one component type. |
| Warmup | The creation of groups for literal query chains when a world starts. |

## 4. Architecture

### 4.1 Files

| File | Responsibility |
|---|---|
| `Runtime/Core/Entity*.cs` | The entity API. |
| `Runtime/Core/World.cs`, `WorldFacade.cs` | The single static world: entity creation, recycling, dispose. |
| `Runtime/Core/EntityMeta.cs`, `EntityMetas.cs` | The per-entity slots in one unmanaged buffer. |
| `Runtime/Core/Mask.cs` | Mask operations on `ulong*` and the mask width. |
| `Runtime/Core/ComponentID.cs` | Component type registration. |
| `Runtime/Core/Storage.cs` | Paged component storage and the list of groups that depend on each component. |
| `Runtime/Core/Composition.cs`, `Group.cs`, `Groups.cs` | Group identity, membership and the group registry. |
| `Runtime/Core/QueryNode.cs`, `Query.cs`, `ComponentQuery.cs` | Query API and cached query state. |
| `Runtime/Core/PostProcessor.cs` | The deferred structural-change queue and the flush. |
| `Runtime/Generation/*.cs` | Runtime API that generated code calls. |
| `Runtime/Utils/Life/*.cs` | `Life`, a pooled lifetime token on top of UniTask. |
| `Runtime/Utils/Unsafe/UnsafeSpan.cs` | Unmanaged growable list for per-entity group and child lists. |
| `Generator~/` | Roslyn source generators. Unity ignores folders that end with `~`. |
| `Generator.dll` | The compiled generators. Unity runs this file. |

### 4.2 Data flow

1. `entity.Add(c)` writes `c` into `Storage<T>`. If the component is new on the entity, it sets the mask bit, increments `ComponentCount` and queues a change. One entity has at most one queued change operation. The pending mask collects the changed bits.
2. The flush reads each operation. For a change, it visits the groups of each changed component. Each group evaluates the entity once per flush. For a kill, it clears the storages and removes the entity from its groups.
3. `Query.Of<A>().With<B>()` walks cached query nodes. It does no hashing and no allocation after the first call. Enumeration resolves the group of the node, runs the flush, and iterates the dense entity array of the group.
4. A new component type calls `Storage.EnsureCapacity` and `Mask.EnsureCapacity`. When `Mask.Words` grows while a world exists, `EntityMetas.Relayout` moves every slot to a wider layout.
5. Generated code runs at `RuntimeInitializeLoadType.AfterAssembliesLoaded`. It creates storages, registers aspect masks and registers warmups. This happens before the first world exists.

## 5. Invariants

Code comments refer to these rules. Read them before you edit a file in `Runtime/Core`. A change that breaks one of these rules needs a new rule and new tests.

1. **Resolve IDs before you take a slot pointer.** A first-time `ComponentID<T>.Value` can grow `Mask.Words` and move all slots. Read the ID first, then take the `EntityMeta*`.
2. **Do not keep a slot pointer across calls that can move slots.** `Entity.Create` can grow the slot buffer. Life callbacks run user code. Take the `EntityMeta*` again after such calls.
3. **Every registered ID fits in the entity masks.** `Mask.EnsureCapacity` runs at each registration. Compositions contain registered IDs only. Because of this, `Group.Fits` has no bounds check.
4. **Released slots have empty masks.** The kill clears the component words. The pending words are already empty. Entity creation does not clear masks.
5. **Composition words have no trailing zero words.** Equality and hashing depend on this. Build compositions only with `Mask.With`.
6. **Query nodes do not change after creation, except their caches.** Query structs copy one reference. Copies must not affect each other.
7. **Group caches use `World.Version`.** A disposed group drops its arrays, because query nodes keep it until their next resolution.
8. **Each group evaluates an entity once per flush.** `Group.ProcessChange` uses the visit counter of the flush. `ProcessEntity` must stay idempotent.
9. **A group subscribes to storages before it allocates.** A component without storage then throws before memory can leak.
10. **`Groups.GetOrCreate` adds the group to both indexes before it fills the group.** The fill runs a flush, and the flush can reach this group.
11. **Slot 0 is `Entity.Null` and is never alive.** `EntityMetas.Peek` maps an invalid ID to slot 0.
12. **Liveness compares `Age` for equality only.** `Age` can wrap.
13. **The hot fields of `EntityMeta` stay last.** They share a cache line with the component mask after the struct.
14. **`Storage<T>.Instance` must exist before use.** The generated registry creates it for each closed component type. Tests that use generic component types call `StorageFactory.Create<T>` themselves.
15. **Statics can survive between play sessions.** Users can disable domain reload. Generated registrations run once. A new world must work with old statics.

## 6. Hot paths

These paths run per entity or per frame:

- `Get`, `Set`, `Has`, `TryGet`, `Exist`, `Add`, `Remove`.
- `Query.Of`, `With`, `Without`, `Single`, `Query.Single`, `foreach`, `Any`, `Count`.
- The flush: `PostProcessor.Update`, `Group.ProcessChange`, `Fits`, `TryAdd`, `TryRemove`.
- `Entity.Create` and `Kill`.

Rules for hot paths:

1. Allocate no managed memory in steady state. The unit tests `StorageTests` and `MemoryTests` check this.
2. Do not use LINQ, lambdas that capture, boxing, or reflection.
3. Do not add a dictionary lookup to a call that runs each frame. Use a generic static cache, for example `Storage<T>.Instance` or `ComponentID<T>.Value`.
4. Do not add a virtual or interface call inside a per-entity loop.
5. Unsafe code and pointers are allowed. Use `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on very small helpers.
6. Keep the cost of a check the same at every mask width. Iterate set bits or non-zero words, not all words.
7. Keep class components and struct components equally fast. `Storage<T>` wraps values in the struct `Slot` to skip the array covariance check for classes.

## 7. Source generators

| Generator | Output |
|---|---|
| `ComponentRegistryGenerator` | `StorageFactory.Create<T>()` for each closed, non-generic component type of the assembly. |
| `AspectGenerator` | The aspect extension methods. |
| `AspectCacheGenerator` | `AspectCache<TAspect>.Register(...)` with the component mask of each aspect, including nested aspects. |
| `QueryGenerator` | `QueryWarmup.Register(...)` for each literal `Query.Of` chain. |

Rules:

1. Generated code is compiled into user assemblies. It can call only `public` runtime API.
2. When you change runtime API that generated code calls, change the generator in the same change.
3. Keep `Microsoft.CodeAnalysis.CSharp` at version 4.3.0. Unity loads analyzers built against this version only.
4. Do not replace `Generator.dll.meta`. It holds the import settings that make Unity run the generators.

Rebuild procedure:

1. Run `dotnet build -c Release` in `Generator~`.
2. Copy `Generator~/bin/Release/netstandard2.0/Generator.dll` to `Generator.dll`. `Generator~/build.cmd` does steps 1 and 2. Run it from a `cmd` shell in `Generator~`.
3. Compile Yogurt-Polygon and run its unit tests.
4. Run `QueryWarmupTests` and the aspect tests in `QueryTests`. These tests use generated code.

## 8. Change procedure

Do these steps for each change to Yogurt:

1. Read sections 1, 5 and 6.
2. Make the change with the Rider MCP. The Yogurt-Polygon `AGENTS.md`, section 2.1, gives the rules. Create new files with `create_new_file`. Edit files with `apply_patch`. Use the Rider refactorings for renames and deletions.
3. Compile Yogurt-Polygon. Fix all compile errors.
4. Run Rider `lint_files` and ReSharper `get_diagnostics` on each changed file. Fix all warnings and errors.
5. Add unit tests for each new behavior and each fixed bug. Remove tests of removed behavior.
6. Run all unit tests. All tests must pass.
7. Measure before and after with the interleaved A/B procedure in the Yogurt-Polygon `AGENTS.md`. Use the `Performance` category. Use the `Memory` category when the change can affect memory.
8. Measure with wide masks when the change touches masks, slots, groups or the flush. Use 200 and 968 reserved component IDs.
9. Report the results to the user. Name each regression.

## 9. Code style

1. Match the code around your change: naming, braces, file layout.
2. Use the namespace `Yogurt`. Put one main type in each file.
3. Use braces for `for` and `foreach` bodies in `Runtime`. ReSharper reports a missing brace as an error.
4. Use `readonly struct` when a struct does not change after creation.

### 9.1 Comments

AI agents maintain this code. Most comments only repeat the code.

1. Do not write a comment that tells what the code does.
2. Write a comment only for a rule that the code cannot show. Examples: an invariant from section 5, an order that must not change, or a reason a simpler version is wrong.
3. Keep each comment to one line when possible.
4. Remove a comment when its rule stops being true.
5. Do not keep commented-out code.
