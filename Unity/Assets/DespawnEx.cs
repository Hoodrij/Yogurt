using UnityEngine;

namespace Yogurt.Unity
{
    public static class DespawnEx
    {
        public static void Despawn(this GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out PoolLink link))
            {
                link.Pool.Push(link);
            }
            else
            {
                Object.Destroy(gameObject);
            }
        }

        public static void Despawn(this Component component)
        {
            component.gameObject.Despawn();
        }
    }
}
