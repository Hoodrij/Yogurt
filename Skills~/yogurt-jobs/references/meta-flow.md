# Meta → gameplay → meta

- One job (`RunGameJob`) owns the whole flow. A phase never starts the next phase itself.
- Each phase is an entity that owns its views and objects. Its Life is the phase.
- The flow awaits the phase entity's Life (awaiting a Life never throws).
- A phase writes its result into an owner that outlives it (the game entity). A dead entity's data is gone.
- Meta and gameplay share only the game entity.

```csharp
public enum LevelResult { None, Won, Lost, Quit }

public struct Game : IComponent { }
public struct Progress : IComponent { public int Level; public LevelResult Result; }
public struct Menu : IComponent { }

public record struct GameAspect(Entity Entity) : IAspect
{
    public ref Game Game => ref this.Get<Game>();
    public ref Progress Progress => ref this.Get<Progress>();
    public GameConfig Config => this.Get<GameConfig>();
}

public record struct MenuAspect(Entity Entity) : IAspect
{
    public ref Menu Menu => ref this.Get<Menu>();
    public MenuView View => this.Get<MenuView>();
}

public class Boot : MonoBehaviour
{
    private void Awake()
    {
        new RunGameJob().Run();
    }
}

public struct RunGameJob
{
    public void Run()
    {
        GameAspect game = new CreateGameJob().Run();
        game.Run(PlaySession);   // repeats while the game entity exists
        return;

        async UniTask PlaySession()
        {
            await new RunMenuJob().Run(game);
            await new RunLevelJob().Run(game);
            await new ShowResultJob().Run(game);
        }
    }
}

public struct CreateGameJob
{
    public GameAspect Run()
    {
        return Entity.Create()
            .Add(new Game())
            .Add(new Progress { Level = 1 })
            .Add(Resources.Load<GameConfig>("Configs/GameConfig"))
            .Add(Resources.Load<LevelConfig>("Configs/LevelConfig"))
            .As<GameAspect>();
    }
}

public struct RunMenuJob
{
    public async UniTask Run(GameAspect game)
    {
        MenuAspect menu = await new CreateMenuJob().Run(game);
        await menu.View.Play.OnClickAsync(menu.Life());
        menu.Kill();
    }
}

public struct CreateMenuJob
{
    public async UniTask<MenuAspect> Run(GameAspect game)
    {
        MenuView view = await game.Config.Menu.Spawn();
        MenuAspect menu = Entity.Create()
            .Link(view.gameObject)
            .Add(view)
            .Add(new Menu())
            .SetParent(game.Entity)
            .As<MenuAspect>();

        view.Show(game);
        return menu;
    }
}

public struct RunLevelJob
{
    public async UniTask Run(GameAspect game)
    {
        game.Progress.Result = LevelResult.None;
        LevelAspect level = await new CreateLevelJob().Run(game);
        PlayerAspect player = Query.Single<PlayerAspect>();

        new RunTurnsJob().Run(level);
        new QuitLevelOnClickJob().Run(level).Forget();

        await (level.Life() | player.Life());

        if (!player.Exist())
        {
            new LoseLevelJob().Run(game);
        }

        level.Kill();   // stops turns and the quit wait, despawns all level views
    }
}

public struct ReachExitJob          // called when the player enters the exit cell
{
    public void Run()
    {
        GameAspect game = Query.Single<GameAspect>();
        game.Progress.Result = LevelResult.Won;
        game.Progress.Level++;
        Query.Single<LevelAspect>().Kill();
    }
}

public struct LoseLevelJob
{
    public void Run(GameAspect game)
    {
        game.Progress.Result = LevelResult.Lost;
        game.Progress.Level = 1;
    }
}

public struct QuitLevelOnClickJob
{
    public async UniTask Run(LevelAspect level)
    {
        await level.View.Quit.OnClickAsync(level.Life());
        Query.Single<GameAspect>().Progress.Result = LevelResult.Quit;
        level.Kill();
    }
}

public struct ShowResultJob
{
    public async UniTask Run(GameAspect game)
    {
        if (game.Progress.Result == LevelResult.Quit)
        {
            return;
        }

        ResultScreenAspect screen = await new CreateResultScreenJob().Run(game);   // like CreateMenuJob
        await screen.View.Continue.OnClickAsync(screen.Life());
        screen.Kill();
    }
}
```
