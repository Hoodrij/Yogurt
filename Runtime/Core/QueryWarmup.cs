using System;
using System.Collections.Generic;

namespace Yogurt
{
    public static class QueryWarmup
    {
        private static readonly List<Action> registrations = new();

        public static void Register(Action warmup)
        {
            registrations.Add(warmup);

            if (WorldFacade.World != null)
            {
                warmup();
            }
        }

        internal static void Run()
        {
            foreach (Action queryWarmup in registrations)
            {
                queryWarmup();
            }
        }
    }
}
