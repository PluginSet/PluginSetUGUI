using PluginSet.Core;
using UnityEngine;

namespace PluginSet.UGUI
{
    public class ObjectPoolOwner: MonoBehaviour
    {
        public ObjectPool<GameObject> PoolOwner { private get; set; }

        public void Put()
        {
            if (PoolOwner == null)
                Destroy(this);
            else
                PoolOwner.Put(gameObject);
        }
    }
}