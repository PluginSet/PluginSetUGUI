using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PluginSet.UGUI
{
    public abstract class BranchAssetAdaptor: MonoBehaviour
    {
        [SerializeField]
        private string url;
        
        public string Url
        {
            get => url;

            set
            {
                url = value;
                OnSetPackageItemUrl(value);
            }
        }
        
        protected abstract void OnAssetChanged(Object asset);

        protected PackageItem CurrentAsset;


        protected virtual void OnEnable()
        {
            if (!string.IsNullOrEmpty(url) && CurrentAsset == null)
                OnSetPackageItemUrl(url);
        }
        
        protected virtual void OnDisable()
        {
        }

        public void OnBranchChanged(string branch)
        {
            if (string.IsNullOrEmpty(url))
                return;

            var branchAsset = UIPackage.GetPackageItem(url);
            SetPackageItem(branchAsset);
        }

        public void SetPackageItem(string url, PackageItem item)
        {
            this.url = url;
            SetPackageItem(item);
        }
        
        private void SetPackageItem(PackageItem item)
        {
            if (CurrentAsset == item)
                return;

            CurrentAsset = item;
            OnPackageItemChanged(item);
        }

        protected virtual void OnPackageItemChanged(PackageItem item)
        {
            OnAssetChanged(item?.asset);
        }
        
        private void OnSetPackageItemUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                SetPackageItem(null);
            }
            else
            {
                var branchAsset = UIPackage.GetPackageItem(url);
                SetPackageItem(branchAsset);
            }
        }
    }

    public abstract class BranchAssetAdaptor<T> : BranchAssetAdaptor where T : Object
    {
        protected override void OnAssetChanged(Object asset)
        {
            OnAssetChanged(asset as T);
        }

        protected abstract void OnAssetChanged(T asset);
    }
}