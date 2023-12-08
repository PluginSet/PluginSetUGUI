using PluginSet.Core;

namespace PluginSet.UGUI
{
    public interface IPackageLoader
    {
        string GetPackageBundleName(string packageName);
        
        AsyncOperationHandle PreparePackage(string packageName);

        UIPackage LoadPackage(string packageName);

        void OnBranchChanged(UIPackage package, string fromBranch, string toBranch);

        void ReleasePackage(UIPackage package);
        
        void UnPreparePackage(string packageName);
    }
}