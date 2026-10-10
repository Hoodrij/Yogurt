---
name: yogurt-unity
description: Yogurt.Unity glue: passive views, buttons awaited by jobs, Link/Despawn, Asset and PooledAsset, configs in Resources, boot, Wait, Run, folder layout. Use when writing any MonoBehaviour view, UI, input, prefab reference, ScriptableObject config or boot code in a Yogurt game.
---

# Yogurt in Unity

`Yogurt.Unity` (namespace `Yogurt.Unity`, add `using Yogurt.Unity;`): `Wait`, `Run`, `Link`, `EntityLink`, `GetEntity`, `Asset<T>`, `PooledAsset<T>`, `Despawn`, `IBlueprint`, `EntityBlueprint`, `PopulateFrom`.

## Views

A view is a passive MonoBehaviour component of its entity. A job passes it the object's aspect; the view reads what it shows. One `Show(aspect)` beats several setters that callers must call in the right order.

```csharp
public class AgentView : MonoBehaviour, IComponent
{
    [SerializeField] private TMP_Text health;
    public Button Select;                  // controls are public for jobs to await

    public void Show(AgentAspect agent)
    {
        transform.localPosition = (Vector2)agent.Position.Value;
        health.text = agent.Health.Value.ToString();
    }

    public async UniTask PlayMove(Vector2Int cell) { /* tween to cell */ }
}
```

A view never runs queries or jobs, never changes components, never stores an aspect or entity, and has no game logic in `Update`/`Start`/`Awake`. The job that changes data calls `View.Show(...)` afterwards.

## Input

Jobs await controls, bound to a Life. No `onClick.AddListener`, no C# events, no view calling a job.

```csharp
await level.View.Quit.OnClickAsync(level.Life());
int i = await UniTask.WhenAny(menu.View.Play.OnClickAsync(menu.Life()), menu.View.Exit.OnClickAsync(menu.Life()));
await zombie.Agent.View.GetAsyncPointerClickTrigger().OnPointerClickAsync(zombie.Life());   // Cysharp.Threading.Tasks.Triggers; needs EventSystem + Physics(2D)Raycaster
```

## Link and despawn

```csharp
AgentView view = await config.Zombie.Spawn(level.View.Board);
ZombieAspect zombie = Entity.Create().Link(view.gameObject).Add(view).Add(new Zombie()).SetParent(level.Entity).As<ZombieAspect>();
Entity hit = collider.GetEntity();   // Entity.Null if not linked
```

A linked GameObject despawns when its entity dies: kill the entity, never `Destroy` it. `view.Despawn()` only for unlinked objects such as effects (returns pooled objects to the pool, destroys the rest).

## Assets

`Asset<T>` is a prefab reference (`T` on the prefab root). `PooledAsset<T>` reuses despawned instances: use it for frequent objects, `Asset<T>` for rare ones. `Spawn(parent)` takes at least a frame. A pooled view keeps old state, so `Show` it after spawning.

## Configs

```csharp
[CreateAssetMenu]
public class LevelConfig : ScriptableObject, IComponent
{
    public int Zombies = 3;
    public Asset<LevelView> Level;
    public PooledAsset<AgentView> Zombie;
}

Entity.Create().Add(new Game()).Add(Resources.Load<LevelConfig>("Configs/LevelConfig"));   // in CreateGameJob
LevelConfig config = Query.Single<LevelConfig>();                                          // in any job
```

Many configs of one kind implement `IBlueprint.Populate(Entity)`; `Entity.Create().PopulateFrom(config)`. `EntityBlueprint` keeps the blueprint on the entity.

## Boot

```csharp
public class Boot : MonoBehaviour
{
    private void Awake() { new RunGameJob().Run(); }
}
```

No fields, no parameters: the game must start from an empty boot.

## Wait and Run

`Wait.Until/While(condition, life)`, `Wait.Seconds(s, life)` (scaled time), `Wait.Update()`. Without a Life a wait ends on application quit. `entity.Run(step)` calls the step every frame while the entity lives; see `yogurt-jobs` and `yogurt-life`.

## Layout

```text
Assets/
├── Game.unity                  one GameObject with Boot
├── Resources/Configs, Resources/Prefabs
└── Scripts/
    ├── Boot.cs, GlobalUsings.cs
    └── Entities/<Object>/      jobs of the object
        └── Components/         its aspect, components, view, config
```

One type per file, no assembly definitions. One namespace for the game, not under `Yogurt`: inside `Yogurt.X`, a `Unity.*` name such as `Unity.Mathematics` resolves to `Yogurt.Unity`.
