using System;
using System.Collections.Generic;
using System.Linq;
using PluginSet.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PluginSet.UGUI
{
    internal class ResObjectPool : ObjectPool<GameObject>
    {
        private readonly GameObject _prefab;
        private readonly Transform _transform;

        public string PackageName;
        public PackageItem PackageItem { get; private set; }
        
        public ResObjectPool(PackageItem item, Transform panel, int maxSize = 100) : base(maxSize)
        {
            PackageItem = item;
            _prefab = (GameObject)item.asset;
            _prefab.SetActive(false);
            _transform = panel;
        }

        protected override GameObject CreateOne()
        {
            return Object.Instantiate(_prefab);
        }

        protected override void OnCreate(GameObject obj)
        {
            var owner = obj.AddComponent<ObjectPoolOwner>();
            owner.PoolOwner = this;
        }

        protected override void OnPreGet(GameObject obj)
        {
            obj.transform.SetParent(null, false);
        }

        protected override void OnPrePut(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(_transform, false);
        }

        protected override void OnPreDrop(GameObject obj)
        {
            Object.Destroy(obj);
        }
    }
    
    public class UIObjectPool: IDisposable
    {
        private static readonly Dictionary<string, UIObjectPool> Instances = new Dictionary<string, UIObjectPool>();
        private static UIObjectPool _instance;

        public static UIObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance("UnityGUIPool");
                }

                return _instance;
            }
        }

        public static UIObjectPool CreateInstance(string rootName)
        {
            if (Instances.TryGetValue(rootName, out var inst))
                return inst;
            
            var gameObject = new GameObject(rootName);
            Object.DontDestroyOnLoad(gameObject);
            var newInst = new UIObjectPool(gameObject);
            Instances.Add(rootName, newInst);
            return newInst;
        }

        public static void ClearAll()
        {
            foreach (var inst in Instances.Values)
            {
                inst.Clear();
            }
        }

        private readonly GameObject _gameObject;
        private readonly Transform _transform;
        private readonly Dictionary<PackageItem, ResObjectPool> _assetPools = new Dictionary<PackageItem, ResObjectPool>();

        private UIObjectPool(GameObject gameObject)
        {
            _gameObject = gameObject;
            _transform = gameObject.transform;
            UnityGUIUtils.OnPackageUnloadHandle += OnPackageUnload;
        }

        private ResObjectPool GetObjectPool(PackageItem item)
        {
            if (!_assetPools.TryGetValue(item, out var result))
            {
                result = new ResObjectPool(item, _transform)
                {
                    PackageName = item.packageName,
                };
                _assetPools.Add(item, result);
            }

            return result;
        }

        public GameObject Get(PackageItem item)
        {
            var pool = GetObjectPool(item);
            return pool.Get();
        }

        public void Put(GameObject obj)
        {
            if (obj == null)
                return;

            var owner = obj.GetComponent<ObjectPoolOwner>();
            if (owner != null)
                owner.Put();
            else
                Object.Destroy(obj);
        }

        public void Clear()
        {
            foreach (var pool in _assetPools.Values)
            {
                pool.Clear();
            }
            _assetPools.Clear();
        }

        public void ClearByItem(PackageItem item)
        {
            if (_assetPools.TryGetValue(item, out var pool))
            {
                pool.Clear();
                _assetPools.Remove(item);
            }
        }

        private void OnPackageUnload(string packageName)
        {
            foreach (var pool in _assetPools.Values.ToArray())
            {
                if (pool.PackageName.Equals(packageName))
                {
                    pool.Clear();
                    _assetPools.Remove(pool.PackageItem);
                }
            }
        }

        public void Dispose()
        {
            Clear();
            UnityGUIUtils.OnPackageUnloadHandle -= OnPackageUnload;
            Object.Destroy(_gameObject);
        }
    }
}