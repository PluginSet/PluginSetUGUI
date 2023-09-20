using PluginSet.Core;

namespace PluginSet.UGUI
{
    public interface IPackageLoader
    {
        AsyncOperationHandle PreparePackage(string packageName);

        UIPackage LoadPackage(string packageName);

        void OnBranchChanged(UIPackage package, string fromBranch, string toBranch);

        void ReleasePackage(UIPackage package);
        
        void UnPreparePackage(string packageName);
    }
}