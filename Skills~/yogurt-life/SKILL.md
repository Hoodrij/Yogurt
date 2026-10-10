---
name: yogurt-life
description: Yogurt Life as the lifetime of a process: create, end, await, combine, parent, bind waits, scope, race, store process state. Use whenever code starts, stops, races or waits for a process, or touches Life, CancellationToken, `.Forget()` or `using` in Yogurt code.
---

# Yogurt Life

A Life answers "is this process happening now?". Alive or dead; dead stays dead, so a restart needs a new Life. Treat it as the time span of a process (entity, level, turn, animation, open screen), not as a cancel signal.

## Sources

```csharp
Life turn = new Life();                         // until Kill or end of using
Life zombieLife = zombie.Life();                // until the entity dies
async Life DetectHit() { ... }                  // until the method returns
Life move = view.PlayMove(cell);                // UniTask -> Life, until the task ends
Life app = token;                               // CancellationToken -> Life
Life any = a | b;  Life all = a & b;            // first dies / both die
Life child = new Life().SetParent(level.Life()); // dies with parent or on Kill
Life none = default;                            // dead
```

## Use

```csharp
if (life) { }                    // IsAlive(), IsDead()
life.Kill();                     // no-op if dead
await life;                      // continues when it dies, never throws
CancellationToken t = life;      // for UniTask/Unity APIs
using Life scope = new Life();   // killed at scope end
```

## When a Life dies

- `await life` continues normally.
- `Wait.*(…, life)`, `button.OnClickAsync(life)` and any UniTask call given it as a token throw `OperationCanceledException`. It unwinds every job awaiting that chain; `.Forget()` and `async Life` methods swallow it. Do not catch it.
- `owner.Run(step)` stops.

So bind a wait to the Life whose death should stop this job and its callers. When a child can end alone without stopping the parent flow, await the child's Life instead.

## Ownership

Every `new Life()` gets an end (`Kill`, `using` or a parent). Every `.Forget()` starts a job that some Life stops. Lives in static fields are wrong; keep them in locals or components.

## Patterns

Scope: things that belong to a turn live exactly as long as it. A Life is a valid parameter.

```csharp
public struct PlayerTurnJob
{
    public async UniTask Run(PlayerAspect player)
    {
        using Life turn = new Life();
        new ShowPossibleStepsJob().Run(player, turn).Forget();   // shows, awaits turn, hides
        Vector2Int cell = await new WaitForPlayerStepJob().Run(player);
        await new StepJob().Run(player.Agent, cell);
    }
}
```

Race: `async Life` local functions, joined with `|`. When the first ends, `using` kills `turn`, which stops the other.

```csharp
using Life turn = new Life().SetParent(player.Life());
bool skipped = false;
await (WaitForStep() | WaitForSkip());

async Life WaitForStep() => await new WaitForPlayerStepJob().Run(player);
async Life WaitForSkip()
{
    await Query.Single<LevelAspect>().View.Skip.OnClickAsync(turn);
    skipped = true;
}
```

Process state other jobs can ask about: keep the Life in a component.

```csharp
public struct Stun : IComponent { public Life Life; }

using Life stun = new Life().SetParent(agent.Life());
agent.Set(new Stun { Life = stun });
await Wait.Seconds(seconds, stun);

bool stunned = agent.Has<Stun>() && agent.Get<Stun>().Life;
```

Phase: a phase is an entity; the flow awaits its Life (`yogurt-jobs` `references/meta-flow.md`).

## Type

`readonly record struct` (default is dead). With `CSHARP_9` defined it is a class (null is dead). The API is the same.
