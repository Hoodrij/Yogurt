using System.Collections.Generic;
using UnityEngine;

namespace Yogurt.Unity
{
    internal class Pool
    {
        private readonly Stack<PoolLink> free = new();

        public void Add(GameObject gameObject)
        {
            gameObject.AddComponent<PoolLink>().Pool = this;
        }

        public bool TryPop(out PoolLink link)
        {
            while (free.Count > 0)
            {
                link = free.Pop();
                // Objects in the pool can be destroyed with their scene.
                if (link == null)
                {
                    continue;
                }

                link.IsFree = false;
                link.gameObject.SetActive(true);
                return true;
            }

            link = null;
            return false;
        }

        public void Push(PoolLink link)
        {
            if (link.IsFree)
            {
                return;
            }

            link.IsFree = true;
            link.gameObject.SetActive(false);
            free.Push(link);
        }
    }
}
