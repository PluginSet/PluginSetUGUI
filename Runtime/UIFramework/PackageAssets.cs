using System;
using System.Collections.Generic;
using System.Linq;
using PluginSet.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PluginSet.UGUI
{
    [Serializable]
    public struct BranchAsset
    {
        [SerializeField]
        public string Branch;

        [SerializeField]
        public Object Asset;
    }
    
    [Serializable]
    public struct BranchAssets
    {
        [SerializeField]
        public string AssetName;

        [SerializeField]
        public BranchAsset[] Assets;
    }
    
    public class PackageAssets : ScriptableObject
    {
        public const string MainBranchName = "@main";
        
        [Tooltip("运行时使用的所有分支，只有这些分支将会打入包中")]
        public string[] runtimeBranches;

        [Tooltip("运行时使用的资源，只有这些资源会在构建时打入AssetBundle")]
        [DisableEdit]
        public BranchAssets[] runtimeAssets;

        protected Dictionary<string, Dictionary<string, Object>> _assets;

        public Dictionary<string, Dictionary<string, Object>> Assets
        {
            get
            {
                if (_assets == null)
                {
                    _assets = new Dictionary<string, Dictionary<string, Object>>();
                    
                    foreach (var info in runtimeAssets)
                    {
                        var dict = new Dictionary<string, Object>();
                        foreach (var asset in info.Assets)
                        {
                            dict.Add(asset.Branch, asset.Asset);
                        }
                        _assets.Add(info.AssetName, dict);
                    }
                }

                return _assets;
            }
        }
        
        public Object GetAsset(string assetName, string branch, out bool hasBranchAssets, out string outBranch)
        {
            outBranch = null;
            
            if (string.IsNullOrEmpty(branch))
                branch = MainBranchName;
            
            if (Assets.TryGetValue(assetName, out var dict))
            {
                hasBranchAssets = dict.Count > 1;

                if (dict.TryGetValue(branch, out var result))
                {
                    outBranch = branch;
                    return result;
                }

                if (dict.TryGetValue(MainBranchName, out result))
                {
                    outBranch = MainBranchName;
                    return result;
                }
                
                Debug.LogError($"Cannot get asset with name {assetName} at branch {branch}");
                return null;
            }

            hasBranchAssets = false;
            Debug.LogError($"Cannot get asset with name {assetName}");
            return null;
        }
        
        public Object GetAsset(string assetName, string branch = MainBranchName)
        {
            if (string.IsNullOrEmpty(branch))
                branch = MainBranchName;
            
            if (Assets.TryGetValue(assetName, out var dict))
            {
                if (dict.TryGetValue(branch, out var result))
                    return result;

                if (dict.TryGetValue(MainBranchName, out result))
                    return result;
                
                Debug.LogError($"Cannot get asset with name {assetName} at branch {branch}");
                return null;
            }
            
            
            Debug.LogError($"Cannot get asset with name {assetName}");
            return null;
        }

        public bool HasBranchAssets(string assetName)
        {
            if (Assets.TryGetValue(assetName, out var dict))
            {
                return dict.Count > 1;
            }

            return false;
        }
    }
    
    public class PackageAssets<T>: PackageAssets where T : Object
    {
#if UNITY_EDITOR
        [Tooltip("包含的所有分支")]
        public string[] includeBranches;

        [Tooltip("包含的所有资源，包括分支资源，分支资源文件名称以_branchName结尾")]
        public AssetReference<T>[] includeAssets;
        
        [Tooltip("额外的依赖包，方便控制依赖资源打包分装")]
        public Object[] extensionDepends;
        
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;
            
            CheckBranches();
            UpdateRuntimeAssets();
        }

        public void Refresh()
        {
            OnValidate();
        }

        private void CheckBranches()
        {
            if (includeBranches == null)
            {
                includeBranches = new [] { MainBranchName };
            }
            else if (!includeBranches.Contains(MainBranchName))
            {
                var branches = new List<string>(includeBranches);
                branches.Insert(0, MainBranchName);
                includeBranches = branches.ToArray();
            }
            
            if (runtimeBranches == null)
            {
                runtimeBranches = new [] { MainBranchName };
            }
            else if (!runtimeBranches.Contains(MainBranchName))
            {
                var branches = new List<string>(runtimeBranches);
                branches.Insert(0, MainBranchName);
                runtimeBranches = branches.ToArray();
            }
        }

        private void UpdateRuntimeAssets()
        {
            if (_assets != null)
            {
                _assets.Clear();
                _assets = null;
            }
            var assetsMap = new Dictionary<string, Dictionary<string, T>>();
            var childBranches = new List<string>(includeBranches);
            childBranches.Remove(MainBranchName);
            foreach (var reference in includeAssets)
            {
                var path = reference.AssetPath;
                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(fileName))
                {
                    Debug.LogError("Cannot get file name at path: " + path);
                    continue;
                }

                var assetName = fileName;
                var branchName = MainBranchName;

                if (childBranches.Count > 0)
                {
                    foreach (var branch in childBranches)
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(fileName, $"(\\w*)_{branch}");
                        if (match.Success)
                        {
                            assetName = match.Groups[1].Value;
                            branchName = branch;
                            break;
                        }
                    }
                }

                if (assetsMap.TryGetValue(assetName, out var dict))
                {
                    if (dict.TryGetValue(branchName, out var _))
                    {
                        Debug.LogError("Has already same assetName and branch with path:: " + path);
                    }
                    else
                    {
                        dict.Add(branchName, reference.CachedAsset);
                    }
                }
                else
                {
                    assetsMap.Add(assetName, new Dictionary<string, T>
                    {
                        {branchName, reference.CachedAsset}
                    });
                }
            }

            var runtimeAssetsData = new List<BranchAssets>();
            foreach (var kv in assetsMap)
            {
                var assetName = kv.Key;
                var dict = kv.Value;
                var assets = new List<BranchAsset>();
                var containsMainBranch = false;
                foreach (var branchKv in dict)
                {
                    var branch = branchKv.Key;
                    var asset = branchKv.Value;
                    if (runtimeBranches.Contains(branch))
                    {
                        assets.Add(new BranchAsset
                        {
                            Branch = branch,
                            Asset = asset,
                        });

                        if (branch.Equals(MainBranchName))
                            containsMainBranch = true;
                    }
                }
                
                if (!containsMainBranch)
                    Debug.LogError($"{assetName} in {this} has no MainBranch Asset!");
                
                runtimeAssetsData.Add(new BranchAssets
                {
                    AssetName = assetName,
                    Assets = assets.ToArray(),
                });
            }

            runtimeAssets = runtimeAssetsData.ToArray();
            Debug.Log("UpdateRuntimeAssets: " + runtimeAssets.Length);
        }

        public void AddAssets(IEnumerable<T> assets)
        {
            List<AssetReference<T>> includedReferences = new List<AssetReference<T>>();
            List<Object> includedAssets = new List<Object>();
            if (includeAssets != null)
            {
                foreach (var reference in includeAssets)
                {
                    includedAssets.Add(reference.CachedAsset);
                    includedReferences.Add(reference);
                }
            }
            
            foreach (var asset in assets)
            {
                if (asset == null)
                    continue;
                
                if (includedAssets.Contains(asset))
                    continue;
                
                includedAssets.Add(asset);
                
                var newReference = new AssetReference<T>();
                newReference.SetEditorAsset(asset);
                includedReferences.Add(newReference);
            }

            includeAssets = includedReferences.ToArray();
            
            OnValidate();
        }
#endif
    }
}