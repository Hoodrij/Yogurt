using System.Collections.Generic;
using System.Linq;

namespace Yogurt
{
    public class Debug
    {
        public static List<Entity> Entities => WorldFacade.GetEntities().ToList();
    }
}