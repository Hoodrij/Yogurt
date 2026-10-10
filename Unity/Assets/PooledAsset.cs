using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Yogurt.Unity
{
    [Serializable]
    public class PooledAsset<T> where T : Component
    {
        public Asset<T> Asset;

        private Pool pool;

        public async UniTask<T> Spawn(Transform parent = null)
        {
            pool ??= new Pool();
            if (pool.TryPop(out PoolLink link))
            {
                link.transform.SetParent(parent, false);
                return link.GetComponent<T>();
            }

            T instance = await Asset.Spawn(parent);
            pool.Add(instance.gameObject);
            return instance;
        }
    }
}
