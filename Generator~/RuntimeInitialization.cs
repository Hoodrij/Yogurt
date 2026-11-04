using System.Text;

namespace Yogurt.Generator
{
    internal static class RuntimeInitialization
    {
        // Unity runs the registration after its assemblies load; plain .NET (Godot, console tools) runs it as a module initializer.
        public static void AppendAttribute(StringBuilder source)
        {
            source.AppendLine("#if UNITY_2019_1_OR_NEWER");
            source.AppendLine("        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]");
            source.AppendLine("#else");
            source.AppendLine("        [System.Runtime.CompilerServices.ModuleInitializer]");
            source.AppendLine("#endif");
        }
    }
}
