using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PluginSet.UGUI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(SafeAreaCalculator))]
    public class UIRoot: MonoBehaviour
    {
        public static UIRoot Instance { get; private set; }
        
        public RectTransform RectTransform { get; private set; }

        internal List<UILayer> Layers;
        
        private float _areaWith;
        private float _areaHeight;
        private float _scale;
        private float _offsetX;
        private float _offsetY;
        
        public UILayer AddLayer(string objName, int sortingOrder = 0)
        {
            var layer = UILayer.CreateLayer(RectTransform, objName, sortingOrder);
            layer.gameObject.layer = UIManager.UILayerMask;
            UILayerApplySafeArea(layer);
            Layers.Add(layer);
            Layers.Sort();
            return layer;
        }
        
        public UILayer FindLayer(string objName)
        {
            return Layers.Find(layer => layer.name == objName);
        }
        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RectTransform = GetComponent<RectTransform>();
            
            Layers = new List<UILayer>();
            Layers.AddRange(GetComponentsInChildren<UILayer>(true));
            Layers.Sort();
            
            var calculator = GetComponent<SafeAreaCalculator>();
            if (calculator != null)
            {
                var rect = calculator.GetSafeArea();
                CalculateSafeArea(rect.x, rect.y, rect.width, rect.height);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        private void CalculateSafeArea(float x, float y, float w, float h)
        {
            var canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                Debug.LogError("UIRoot: CanvasScaler.uiScaleMode must be ScaleWithScreenSize");
                return;
            }

            var factor = canvasScaler.scaleFactor;
            var dWidth = canvasScaler.referenceResolution.x;
            var dHeight = canvasScaler.referenceResolution.y;
            var matchMode = canvasScaler.screenMatchMode;

            _offsetX = x / factor;
            _offsetY = y / factor;
            
            var scaleX = w / dWidth;
            var scaleY = h / dHeight;

            switch (matchMode)
            {
                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                    var match = canvasScaler.matchWidthOrHeight;
                    if (match <= 0f)
                        _scale = scaleX;
                    else if (match >= 1f)
                        _scale = scaleY;
                    else
                        _scale = scaleX * (1f - match) + scaleY * match;
                    break;
                
                case CanvasScaler.ScreenMatchMode.Expand:
                    _scale = Mathf.Min(scaleX, scaleY);
                    break;
                
                case CanvasScaler.ScreenMatchMode.Shrink:
                    _scale = Mathf.Max(scaleX, scaleY);
                    break;
            }

            _areaWith = Mathf.Ceil(w / _scale);
            _areaHeight = Mathf.Ceil(h / _scale);
            
            foreach (var layer in Layers)
            {
                UILayerApplySafeArea(layer);
            }
        }

        private void UILayerApplySafeArea(UILayer layer)
        {
            var rectTransform = layer.GetComponent<RectTransform>();
            if (rectTransform == null)
                return;
            
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(_areaWith, _areaHeight);
            rectTransform.anchoredPosition = new Vector2(_offsetX, _offsetY);
            rectTransform.localScale = new Vector3(_scale, _scale, _scale);
        }
    }
}