using System.Text;

namespace Yogurt.Generator
{
    internal static class RuntimeInitialization
    {
        public static void AppendAttribute(StringBuilder source)
        {
            source.AppendLine("#if UNITY_2019_1_OR_NEWER");
            source.AppendLine("        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]");
            source.AppendLine("#endif");
        }
    }
}
