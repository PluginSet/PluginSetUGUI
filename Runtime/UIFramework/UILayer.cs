using System;
using PluginSet.Core;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PluginSet.UGUI
{
    public sealed class UILayer: MonoBehaviour, IComparable<UILayer>
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/PluginSet UGUI/Add Layer")]
        public static void AddLayerOnRoot(UnityEditor.MenuCommand menuCommand)
        {
            var gameObject = menuCommand.context as GameObject;
            if (gameObject == null)
                return;
            
            var rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
                return;
            
            var layer = CreateLayer(rectTransform, "Layer");
            var layerRect = layer.GetComponent<RectTransform>();
            layerRect.anchorMin = Vector2.zero;
            layerRect.anchorMax = Vector2.one;
            layerRect.offsetMin = Vector2.zero;
            layerRect.offsetMax = Vector2.zero;
            
            UnityEditor.Undo.RegisterCreatedObjectUndo(layer.gameObject, "Create Layer");
        }
#endif
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        internal static UILayer CreateLayer(RectTransform parent, string name, int sortingOrder = 0)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var layer = obj.AddComponent<UILayer>();
            layer.Canvas = obj.AddComponent<Canvas>();
            obj.AddComponent<GraphicRaycaster>();
            layer.InitCanvas(sortingOrder);
            return layer;
        }

        internal Canvas Canvas { get; private set; }
        public RectTransform RectTransform { get; private set; }

        private RectTransform _modalLayer;

        private void Awake()
        {
            if (Canvas == null)
                Canvas = GetComponent<Canvas>();
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
        }

        private void InitCanvas(int sortingOrder)
        {
            Canvas.overrideSorting = true;
            Canvas.sortingOrder = sortingOrder;
            RectTransform = GetComponent<RectTransform>();
        }
        
        public int CompareTo(UILayer other)
        {
            return other.Canvas.sortingOrder.CompareTo(Canvas.sortingOrder);
        }

        internal bool AdjustModalLayer()
        {
            var findModal = false;
            var maxSiblingIndex = int.MinValue;
            foreach (var win in RectTransform.GetComponentsInChildren<UIWindowBase>())
            {
                if (!win.IsShown)
                    continue;
                
                if (!win.IsModal)
                    continue;
                
                maxSiblingIndex = Mathf.Max(maxSiblingIndex, win.transform.GetSiblingIndex());
                findModal = true;
            }

            if (!findModal)
            {
                HideModalLayer();
                return false;
            }
            
            ShowModalLayer(maxSiblingIndex - 1);
            return true;
        }
        
        private void ShowModalLayer(int siblingIndex)
        {
            if (_modalLayer == null)
            {
                _modalLayer = CreateModalLayer();
                _modalLayer.SetParent(RectTransform);
                _modalLayer.gameObject.AddComponent<MakeFullScreen>();
                siblingIndex++;
            }
            
            _modalLayer.gameObject.SetActive(true);
            _modalLayer.SetSiblingIndex(siblingIndex);
        }

        internal void HideModalLayer()
        {
            if (_modalLayer == null)
                return;
            
            _modalLayer.gameObject.SetActive(false);
        }

        private RectTransform CreateModalLayer()
        {
            var asset = UIConfig.ModalLayerAsset;
            
            if (asset.StartsWith(UIPackage.URL_PREFIX))
            {
                var item = UIPackage.GetPackageItem(asset);
                if (item != null)
                {
                    var uiObj = UIObjectPool.Instance.Get(item);
                    return uiObj.GetComponent<RectTransform>();
                }
            }
            else if (!string.IsNullOrEmpty(asset))
            {
                var res = ResourcesManager.Instance.Load<GameObject>(asset);
                if (res != null)
                {
                    var resObj = Object.Instantiate(res);
                    return resObj.GetComponent<RectTransform>();
                }
            }
            
            var obj = new GameObject("ModalLayer");
            obj.AddComponent<CanvasRenderer>();
            var image = obj.AddComponent<Image>();
            image.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            image.color = UIConfig.ModalLayerColor;
            var rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = obj.AddComponent<RectTransform>();

            return rectTransform;
        }
    }
}