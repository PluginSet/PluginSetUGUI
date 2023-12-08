using System;
using System.Diagnostics;
using PluginSet.Core;

namespace PluginSet.UGUI
{
    public static class UnityGUIUtils
    {
        private static IPackageLoader _packageLoader;

        public delegate void OnPackageLoadedOrUnload(string packageName);

        public delegate void OnPackageBranchChange(string packageName, string fromBranch, string toBranch);
        
//        public static void AddOnBranchChange(this GObject gObject, EventCallback0 callback)
//        {
//            gObject.AddEventListener(UnionPackage.EVENT_ON_BRANCH_CHANGE, callback);
//        }
//        
//        public static void AddOnBranchChange(this GObject gObject, EventCallback1 callback)
//        {
//            gObject.AddEventListener(UnionPackage.EVENT_ON_BRANCH_CHANGE, callback);
//        }
//        
//        public static void RemoveOnBranchChange(this GObject gObject, EventCallback0 callback)
//        {
//            gObject.RemoveEventListener(UnionPackage.EVENT_ON_BRANCH_CHANGE, callback);
//        }
//        
//        public static void RemoveBranchChange(this GObject gObject, EventCallback1 callback)
//        {
//            gObject.RemoveEventListener(UnionPackage.EVENT_ON_BRANCH_CHANGE, callback);
//        }

        public static void SetPackageLoad(IPackageLoader loader)
        {
            _packageLoader = loader;
        }

        public static string GetBundleName(string package)
        {
            CheckPackageLoader();
            return _packageLoader.GetPackageBundleName(package);
        }

        public static AsyncOperationHandle PreparePackages(params string[] packageNames)
        {
            var count = packageNames.Length;
            if (count <= 0) 
                return AsyncNonResultHandle<object>.Default;
            
            CheckPackageLoader();
            var handles = new AsyncOperationHandle[count];
            for (int i = 0; i < count; i++)
            {
                handles[i] = _packageLoader.PreparePackage(packageNames[i]);
            }
            
            return new MultiAsyncOperations(handles);
        }

        public static event OnPackageLoadedOrUnload OnPackageLoadedHandle;
        public static event OnPackageLoadedOrUnload OnPackageUnloadHandle;
        public static event OnPackageBranchChange OnPackageBranchChangeHandle;

        public static UIPackage LoadPackage(string packageName)
        {
            CheckPackageLoader();
            var pkg = UIPackage.GetByName(packageName);
            if (pkg != null)
            {
                pkg.Retain();
                return pkg;
            }
            
            pkg = _packageLoader.LoadPackage(packageName);
            OnPackageLoadedHandle?.Invoke(packageName);
            return pkg;
        }

        public static void OnPackageBranchChanged(string fromBranch, string toBranch)
        {
            CheckPackageLoader();
            foreach (var package in UIPackage.GetPackages())
            {
                _packageLoader.OnBranchChanged(package, fromBranch, toBranch);
                OnPackageBranchChangeHandle?.Invoke(package.name, fromBranch, toBranch);
            }
        }

        public static void ReleasePackage(UIPackage package)
        {
            CheckPackageLoader();
            if (!package.Release())
                return;
            
            OnPackageUnloadHandle?.Invoke(package.name);
            _packageLoader.ReleasePackage(package);
        }

        public static void UnPreparePackage(string package)
        {
            CheckPackageLoader();
            _packageLoader.UnPreparePackage(package);
        }

        public static void SetDefaultFont(string fontName)
        {
//            if (fontName.Equals(UIConfig.defaultFont))
//                return;
//
//            UIConfig.defaultFont = fontName;
//            ApplyTextFieldFont(Stage.inst);
        }

//        private static void ApplyTextFieldFont(DisplayObject parent)
//        {
//            if (parent == null)
//                return;
//
//            if (parent is TextField)
//            {
//                ((TextField) parent).ApplyFormat();
//            }
//            else if (parent is Container)
//            {
//                var children = ((Container) parent).GetChildren();
//                foreach (var child in children)
//                {
//                    ApplyTextFieldFont(child);
//                }
//            }
//        }

        [Conditional("UNITY_EDITOR")]
        private static void CheckPackageLoader()
        {
            if (_packageLoader == null)
                throw new Exception("Please set PackageLoader first!");
        }
    }
}