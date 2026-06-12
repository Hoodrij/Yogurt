using System.Collections.Generic;

namespace Yogurt
{
    public class Debug
    {
        public static List<Entity> Entities => WorldFacade.CollectAliveEntities();
    }
}
