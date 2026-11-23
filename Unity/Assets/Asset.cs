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
            // worldSpace = false: the instance keeps its local transform under the parent, as Instantiate(prefab, parent) does.
            T[] instances = await Object.InstantiateAsync(Prefab, new InstantiateParameters { parent = parent, worldSpace = false });
            return instances[0];
        }
    }
}
