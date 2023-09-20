#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;


namespace PluginSet.UGUI
{
    [Serializable]
    public sealed class AssetReference<T> where T: Object
    {
        [FormerlySerializedAs("m_assetGUID")]
        [SerializeField]
        string m_AssetGUID = "";
        
        T m_CachedAsset;
        string m_CachedGUID = "";
        bool m_Loaded = false;

        private void AutoLoad()
        {
            if (!m_Loaded)
            {
                if (!string.IsNullOrEmpty(m_AssetGUID))
                {
                    var path = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
                    SetEditorAsset(AssetDatabase.LoadAssetAtPath<T>(path));
                }
                else
                {
                    SetEditorAsset(null);
                }
            }
        }
        
        public string AssetPath
        {
            get
            {
                var asset = CachedAsset;
                if (asset == null)
                    return null;
                
                return AssetDatabase.GetAssetOrScenePath(asset);
            }
        }

        /// <summary>
        /// Cached Editor Asset.
        /// </summary>
        public T CachedAsset
        {
            get
            {
                if (m_CachedGUID != m_AssetGUID)
                {
                    m_Loaded = false;
                }

                AutoLoad();
                return m_CachedAsset;
            }
            set
            {
                m_CachedAsset = value;
                m_CachedGUID = m_AssetGUID;
            }
        }

        public bool IsMissing
        {
            get
            {
                if (string.IsNullOrEmpty(m_AssetGUID))
                    return false;

                return CachedAsset == null;
            }
        }
        
        
        public bool ValidateAsset(string path)
        {
            return true;
        }
        
        /// <summary>
        /// Sets the main asset on the AssetReference.  Only valid in the editor, this sets both the editorAsset attribute,
        ///   and the internal asset GUID, which drives the RuntimeKey attribute. If the reference uses a sub object,
        ///   then it will load the editor asset during edit mode and load the sub object during runtime. For example, if
        ///   the AssetReference is set to a sprite within a sprite atlas, the editorAsset is the atlas (loaded during edit mode)
        ///   and the sub object is the sprite (loaded during runtime). If called by AssetReferenceT, will set the editorAsset
        ///   to the requested object if the object is of type T, and null otherwise.
        /// <param name="value">Object to reference</param>
        /// </summary>
        public bool SetEditorAsset(T value)
        {
            if (value == null)
            {
                m_AssetGUID = string.Empty;
                CachedAsset = null;
                m_Loaded = true;
                return true;
            }

            if (m_CachedAsset != value)
            {
                var path = AssetDatabase.GetAssetOrScenePath(value);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarningFormat("Invalid object for AssetReference {0}.", value);
                    return false;
                }
                if (!ValidateAsset(path))
                {
                    Debug.LogWarningFormat("Invalid asset for AssetReference path = '{0}'.", path);
                    return false;
                }
                else
                {
                    m_AssetGUID = AssetDatabase.AssetPathToGUID(path);
                    T mainAsset = AssetDatabase.LoadAssetAtPath<T>(path);
                    CachedAsset = mainAsset;
                }
            }

            m_Loaded = true;
            return true;
        }
        
    }
}

#endif