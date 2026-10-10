---
name: yogurt-setup
description: Sets up a Unity 6 project for Yogurt with C# 11 for Unity and the IDE. Use when a project starts to use Yogurt, when C# 10/11 syntax does not compile, or when the IDE shows C# 9 errors in Yogurt code.
---

# Set up a Unity project for Yogurt

Write all files yourself. Use the Unity CLI (`unity-cli` skill) for Editor work. Do not ask the user to click.

## Files

`Packages/manifest.json`, add to `dependencies`:

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
"yogurt": "https://github.com/Hoodrij/Yogurt.git",
"games.corundum.isexternalinit": "https://github.com/CorundumGames/IsExternalInit.git",
"com.cysharp.csprojmodifier": "https://github.com/Cysharp/CsprojModifier.git?path=src/CsprojModifier/Assets/CsprojModifier"
```

`Assets/csc.rsp`:

```text
-langVersion:11
```

`LangVersion.props` and `Directory.Build.props` in the project root, same content:

```xml
<Project>
    <PropertyGroup>
        <LangVersion>11</LangVersion>
        <Nullable>disable</Nullable>
    </PropertyGroup>
</Project>
```

`ProjectSettings/CsprojModifierSettings.json` (`Position` 0 is Append, `*` is every project):

```json
{"AdditionalImports":[{"Path":"LangVersion.props","Position":0}],"AdditionalImportsAdditionalProjects":["*"],"EnableAddAnalyzerReferences":false,"AddAnalyzerReferencesAdditionalProjects":[]}
```

`Assets/Scripts/GlobalUsings.cs` (the aliases resolve name conflicts with `Yogurt.Debug` and `System`):

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using Cysharp.Threading.Tasks;
global using UnityEngine;
global using Yogurt;
global using Yogurt.Unity;
global using Debug = UnityEngine.Debug;
global using Object = UnityEngine.Object;
global using Random = UnityEngine.Random;
```

`Assets/Scripts/Boot.cs` (no fields):

```csharp
public class Boot : MonoBehaviour
{
    private void Awake()
    {
        new RunGameJob().Run();
    }
}
```

`Assets/Scripts/Entities/Game/RunGameJob.cs`: a first job that creates the game entity. See `references/meta-flow.md` in `yogurt-jobs`.

Folders: `Assets/Resources/Configs`, `Assets/Resources/Prefabs`, `Assets/Scripts/Entities`.

## Editor

1. Resolve packages: `UnityEditor.PackageManager.Client.Resolve()`.
2. Create `Assets/Game.unity` with one GameObject `Boot` that has `Boot`. Add the scene to the build.
3. Regenerate the project files: `Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll()`.

## Check

1. Add `Assets/Scripts/SetupCheck.cs`:

   ```csharp
   namespace SetupCheck;

   public record struct SetupCheckAspect(Entity Entity) : IAspect;

   public readonly record struct SetupCheckValue(int Value);
   ```

2. Compile with no errors.
3. Each `*.csproj` ends with `<Import Project="LangVersion.props" />`.
4. Enter play mode. The console has no errors.
5. Delete `SetupCheck.cs` and its `.meta`.

## Notes

1. Unity cannot run these C# 11 features: `ref` fields, static abstract interface members, `required` members.
2. Development builds: add `YOGURT_DEBUG` to the scripting defines for error logs on wrong use.
3. A project that cannot use C# 11: define `CSHARP_9`. Then `Life` is a class, and aspects are plain structs with `public Entity Entity { get; set; }`.
