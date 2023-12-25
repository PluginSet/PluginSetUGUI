
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PluginSet.UGUI
{
    public class PackageItem
    {
        public string packageName;
        public string assetName;
        public string branch;
        public bool hasBranchAsset;
        public Object asset;
    }
    
    public class UIPackage
    {
        public const string URL_PREFIX = "ui://";
            
        private static readonly Dictionary<string, UIPackage> Packages = new Dictionary<string, UIPackage>();
        
        private static string _branch;
        public static string branch
        {
            get => _branch;

            set
            {
                var fromBranch = _branch;
                _branch = value;
                UnityGUIUtils.OnPackageBranchChanged(fromBranch, _branch);
                UIRoot.Instance.BroadcastMessage("OnBranchChanged", value, SendMessageOptions.DontRequireReceiver);
            }
        }

        public static UIPackage GetByName(string packageName)
        {
            if (Packages.TryGetValue(packageName, out var package))
                return package;

            return null;
        }

        public static UIPackage AddPackage(string packageName, PackageAssets assets)
        {
            var package = GetByName(packageName);
            if (package != null)
                return package;
            
            package = new UIPackage(packageName, assets);
            Packages.Add(packageName, package);
            return package;
        }

        public static void RemovePackage(string packageName)
        {
            var package = GetByName(packageName);
            if (package == null)
                return;

            Packages.Remove(packageName);
        }

        public static UIPackage[] GetPackages()
        {
            return Packages.Values.ToArray();
        }

        public static void RemoveAllPackages()
        {
            Packages.Clear();
        }

        public static bool SplitName(string url, out string packageName, out string assetName)
        {
            packageName = null;
            assetName = null;
            
            if (!url.StartsWith(URL_PREFIX))
                return false;

            int pos1 = url.IndexOf("//", StringComparison.Ordinal);
            if (pos1 == -1)
                return false;

            int pos2 = url.IndexOf('/', pos1 + 2);
            if (pos2 == -1)
                return false;

            packageName = url.Substring(pos1 + 2, pos2 - pos1 - 2);
            assetName = url.Substring(pos2 + 1);
            return true;
        }
        
        public static T GetAsset<T>(string url) where T : Object
        {
            var item = GetPackageItem(url);
            return item.asset as T;
        }
        
        public static T GetAsset<T>(string url, string branchName) where T : Object
        {
            var item = GetPackageItem(url, branchName);
            return item.asset as T;
        }

        public static PackageItem GetPackageItem(string url)
        {
            return GetPackageItem(url, _branch);
        }

        public static PackageItem GetPackageItem(string url, string branchName)
        {
            if (!SplitName(url, out var packageName, out var assetName))
            {
                Debug.LogError($"Invalid asset url: {url}");
                return null;
            }

            var package = GetByName(packageName);
            return package.GetItem(assetName, branchName);
        }

        public static bool HasBranchAsset(string url)
        {
            if (!SplitName(url, out var packageName, out var assetName))
            {
                return false;
            }
            
            var package = GetByName(packageName);
            return package.HasBranchPackageAsset(assetName);
        }

        internal PackageAssets Assets;
        
        private readonly Dictionary<string, PackageItem> _packageItems = new Dictionary<string, PackageItem>();
        
        public string name;
        private int _reference;

        private UIPackage(string name, PackageAssets assets)
        {
            this.name = name;
            Assets = assets;
            _reference = 1;
        }
        
        public bool IsNewReference()
        {
            return _reference == 1;
        }

        public void Retain()
        {
            _reference++;
        }

        public bool Release()
        {
            _reference--;
            return _reference <= 0;
        }
        
        public bool HasBranchPackageAsset(string assetName)
        {
            if (Assets is null)
                return false;

            return Assets.HasBranchAssets(assetName);
        }

        public PackageItem GetItem(string assetName, string branchName)
        {
            if (Assets == null)
                return null;

            var key = $"{assetName}_{branchName}";
            if (_packageItems.TryGetValue(key, out var result))
                return result;

            var asset = Assets.GetAsset(assetName, branchName, out bool hasBranchAssets, out string outBranch);
            if (asset == null)
                return null;

            var item = new PackageItem
            {
                asset = asset,
                assetName = assetName,
                packageName = name,
                branch = outBranch,
                hasBranchAsset = hasBranchAssets,
            };
            _packageItems.Add(key, item);
            return item;
        }
        
        public T[] GetAllAssets<T>(string branchName = null) where T: Object
        {
            if (Assets == null)
                return null;

            if (string.IsNullOrEmpty(branchName))
                branchName = branch;
            return Assets.GetAllAssets<T>(branchName).ToArray();
        }
        
        public T[] GetAllComponents<T>(string branchName = null) where T: Component
        {
            if (Assets == null)
                return null;

            if (string.IsNullOrEmpty(branchName))
                branchName = branch;
            
            var prefabs = Assets.GetAllAssets<GameObject>(branchName);
            var result = new List<T>();
            foreach (var prefab in prefabs)
            {
                var component = prefab.GetComponent<T>();
                if (component != null)
                    result.Add(component);
            }

            return result.ToArray();
        }
        
        public void Reload(PackageAssets assets)
        {
            Assets = assets;
            _packageItems.Clear();
        }
    }
}