using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Yogurt.Unity
{
    [Serializable]
    public class Asset<T> where T : Component
    {
        public T Prefab;

        public async UniTask<T> Spawn(Transform parent = null)
        {
            T[] instances = await Object.InstantiateAsync(Prefab, parent);
            return instances[0];
        }
    }
}
