using UnityEngine;

namespace Yogurt.Unity
{
    internal static class App
    {
        public static Life Life { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Start()
        {
            Life = new Life();
            Application.quitting -= Kill;
            Application.quitting += Kill;
        }

        private static void Kill()
        {
            Life.Kill();
        }
    }
}
