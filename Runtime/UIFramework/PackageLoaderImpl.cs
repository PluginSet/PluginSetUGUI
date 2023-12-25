using System;
using System.Collections.Generic;
using PluginSet.Core;

namespace PluginSet.UGUI
{
    public class PackageLoaderImpl: IPackageLoader
    {
        private readonly Dictionary<string, Dictionary<string, AsyncOperationHandle>> _loadedPackages = new Dictionary<string, Dictionary<string, AsyncOperationHandle>>();

        protected virtual string GetBundleName(string packageName)
        {
            return $"{packageName}".ToLower();
        }

        protected virtual string GetBundleName(string packageName, string branch)
        {
            return $"{GetBundleName(packageName)}_{branch}".ToLower();
        }

        protected virtual string GetBranchBundleName(string packageName, string branch)
        {
            var manager = ResourcesManager.Instance;
            var branchBundle = GetBundleName(packageName, branch);
            if (manager.ExistsBundle(branchBundle))
                return branchBundle;
            
            var bundleName = GetBundleName(packageName);
            if (!string.Equals(branchBundle, bundleName) && manager.ExistsBundle(bundleName))
                return bundleName;

            return null;
        }

        public string GetPackageBundleName(string packageName)
        {
            return GetBranchBundleName(packageName, UIPackage.branch);
        }

        public AsyncOperationHandle PreparePackage(string packageName)
        {
            if (UIPackage.GetByName(packageName) != null)
                return AsyncNonResultHandle<object>.Default;
            
            if (!_loadedPackages.TryGetValue(packageName, out var dict))
            {
                dict = new Dictionary<string, AsyncOperationHandle>();
                _loadedPackages.Add(packageName, dict);
            }
            
            var manager = ResourcesManager.Instance;
            var bundleName = GetBranchBundleName(packageName, UIPackage.branch);
            if (string.IsNullOrEmpty(bundleName))
                return AsyncNonResultHandle<object>.Default;
                
            if (dict.TryGetValue(bundleName, out var handle))
                return handle;
            
            handle = manager.LoadBundleAsync(bundleName);
            dict.Add(bundleName, handle);
            return handle;
        }

        public UIPackage LoadPackage(string packageName)
        {
            var bundleName = GetBranchBundleName(packageName, UIPackage.branch);
            if (string.IsNullOrEmpty(bundleName))
                throw new Exception($"Cannot find bundle for package {packageName}");
            
            var manager = ResourcesManager.Instance;
            var packageAsset = manager.LoadAsset<PackageAssets>(bundleName, packageName);
            if (packageAsset == null)
                throw new Exception($"Cannot load package asset {packageName} from bundle {bundleName}");
            
            manager.RetainAsset(packageAsset);
            var result = UIPackage.AddPackage(packageName, packageAsset);

            if (_loadedPackages.ContainsKey(bundleName))
            {
                manager.ReleaseBundle(bundleName);
                _loadedPackages.Remove(bundleName);
            }

            return result;
        }

        public void OnBranchChanged(UIPackage package, string fromBranch, string toBranch)
        {
            if (string.Equals(fromBranch, toBranch))
                return;
            
            // 分支改变时，重新加载所有资源
            var packageName = package.name;
            var fromBundle = GetBranchBundleName(packageName, fromBranch);
            var toBundle = GetBranchBundleName(packageName, toBranch);
            if (string.Equals(fromBundle, toBundle))
                return;
            
            var asset = package.Assets;
            var manager = ResourcesManager.Instance;
            var packageAsset = manager.LoadAsset<PackageAssets>(toBundle, packageName);
            if (packageAsset == null)
                throw new Exception($"Cannot load package asset {packageName} from bundle {toBundle}");
            
            manager.RetainAsset(packageAsset);
            package.Reload(packageAsset);
            manager.ReleaseAsset(asset);
        }

        public void ReleasePackage(UIPackage package)
        {
            var asset = package.Assets;
            package.Assets = null;
            ResourcesManager.Instance.ReleaseAsset(asset);
            UIPackage.RemovePackage(package.name);
        }

        public void UnPreparePackage(string packageName)
        {
            var bundleName = GetBranchBundleName(packageName, UIPackage.branch);
            if (string.IsNullOrEmpty(bundleName))
                return;
            
            if (_loadedPackages.ContainsKey(bundleName))
            {
                ResourcesManager.Instance.ReleaseBundle(bundleName);
                _loadedPackages.Remove(bundleName);
            }
        }
    }
}