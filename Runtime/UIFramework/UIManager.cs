
using System;
using System.Collections.Generic;
using PluginSet.Core;
using UnityEngine;
using UnityEngine.Rendering;
using Logger = PluginSet.Core.Logger;

namespace PluginSet.UGUI
{
    public static class UIManager
    {
        private static readonly Logger Logger = LoggerManager.GetLogger("UIManager");
        
        public static readonly LayerMask UILayerMask = LayerMask.NameToLayer("UI");
        
        private struct WindowInfo
        {
            public Type WindowType;
            public bool IsSingleton;
            public int Tag;
        }

        private static readonly Dictionary<string, WindowInfo> WindowInfos = new Dictionary<string, WindowInfo>();
        
        private static string _defaultToastUrl;

        public static void SetDefaultToastUrl(string url)
        {
            _defaultToastUrl = url;
        }
        
//        public static void SetDefaultToastUrl<T>(string url) where T: UIToast
//        {
////            UIObjectFactory.SetPackageItemExtension(url, typeof(T));
//            _defaultToastUrl = url;
//        }

        public static void RegisterWindow<T>(string name, bool isSingleton = true, int tag = 0) where T: UIWindow
        {
            RegisterWindow(name, typeof(T), isSingleton, tag);
        }
        
        public static void RegisterWindow(string name, Type type, bool isSingleton = true, int tag = 0)
        {
            if (WindowInfos.ContainsKey(name))
            {
                Logger.Warn("Repeated window name {0} with type {1}", name, type.FullName);
                return;
            }
            
            WindowInfos.Add(name, new WindowInfo()
            {
                WindowType = type,
                IsSingleton = isSingleton,
                Tag = tag,
            });
        }
        
        public static void UnRegisterWindow(string name)
        {
            if (WindowInfos.ContainsKey(name))
                WindowInfos.Remove(name);
        }

        public static UIWindow FindWindow(string name)
        {
            return UIWindow.GetInstance(name);
        }

        public static bool IsWindowShown(string name, bool isStrict = true)
        {
            var window = FindWindow(name);
            if (window == null)
                return false;

            return !isStrict || window.IsShown;
        }

        public static T ShowOn<T>(UILayer layer, string name, params object[] args) where T: UIWindow
        {
            if (!WindowInfos.TryGetValue(name, out var info))
            {
                Logger.Error("Cannot find window info with name {0}", name);
                return null;
            }

            T window = null;
            if (info.IsSingleton)
            {
                window = UIWindow.GetInstance(name) as T;
            }

            if (window == null)
            {
                var gameObject = new GameObject(name)
                {
                    layer = UILayerMask
                };

                gameObject.SetActive(false);
                window = gameObject.AddComponent(info.WindowType) as T;
                if (window == null)
                    throw new Exception($"Window component type {info.WindowType} is not base on UIWindow");
                
                window.Init();
                window.name = name;
                window.Tag = info.Tag;
            }
            
            window.PushArgs(args);
            window.ShowOn(layer);

            return window;
        }

        public static void CreateToastRule(string name, float defaultStaySeconds, bool outImmediatelyWhenNew, bool enableShowWhenLastOut)
        {
            UIToastShowRule.AddRule(name, new UIToastShowRuleDefault(defaultStaySeconds, outImmediatelyWhenNew, enableShowWhenLastOut));
        }

        public static void ShowToastOn(UILayer layer, string text, string icon = null, string url = null, int tag = 0)
        {
            if (string.IsNullOrEmpty(url))
                url = _defaultToastUrl;
            
            ShowToastOn(layer, text, icon, url, tag, url);
        }
        
        public static void ShowToastOn(UILayer layer, string text, string icon, string url, int tag, string rule)
        {
            if (string.IsNullOrEmpty(url))
                url = _defaultToastUrl;

            var item = UIPackage.GetPackageItem(url);
            var toast = UIObjectPool.Instance.Get(item);
            if (toast == null)
                return;
            
            var toastComponent = toast.GetComponent<UIToast>();
            if (toastComponent == null)
            {
                Logger.Error("Toast component is null");
                return;
            }

            toastComponent.Tag = tag;
            toastComponent.text = text; 
            toastComponent.icon = icon;
            
            toast.GetComponent<RectTransform>().SetParent(layer.RectTransform, false);
            toastComponent.SetRule(UIToastShowRule.GetRule(rule));
        }

        public static void HideAll(UILayer layer)
        {
            HideAllWinOn(layer, window => true);
        }
        
        public static void CloseAll()
        {
            CloseAllWin(val => true);
        }

        public static void CloseAll(UILayer layer)
        {
            CloseAllWinOn(layer, window => true);
        }

        public static void HideAllOn(UILayer layer, int tag)
        {
            HideAllWinOn(layer, win => tag.Equals(win.Tag));
        }
        
        public static void CloseAllOn(UILayer layer, int tag)
        {
            CloseAllWinOn(layer, win => tag.Equals(win.Tag));
        }

        public static void HideAllWindow(string name)
        {
            HideAllWin(win => win.name.Equals(name));
        }

        public static void HideAllWindow(int tag)
        {
            HideAllWin(win => tag.Equals(win.Tag));
        }

        public static void CloseAllWindow(string name)
        {
            CloseAllWin(win => win.name.Equals(name));
        }

        public static void CloseAllWindow(int tag)
        {
            CloseAllWin(win => tag.Equals(win.Tag));
        }

        private static void HideAllWinOn(UILayer layer, Func<IUIEntity, bool> match)
        {
            foreach (var win in layer.GetComponentsInChildren<IUIEntity>(true))
            {
                if (match(win))
                    win.Hide();
            }
        }

        private static void CloseAllWinOn(UILayer layer, Func<IUIEntity, bool> match)
        {
            foreach (var win in layer.GetComponentsInChildren<IUIEntity>(true))
            {
                if (match(win))
                    win.HideImmediately();
            }
        }

        private static void HideAllWin(Func<IUIEntity, bool> match)
        {
            foreach (var layer in UIRoot.Instance.Layers)
            {
                HideAllWinOn(layer, match);
            }
        }
        
        private static void CloseAllWin(Func<IUIEntity, bool> match)
        {
            foreach (var layer in UIRoot.Instance.Layers)
            {
                CloseAllWinOn(layer, match);
            }
        }
        

        internal static void AdjustModalLayer()
        {
            var isTop = true;
            foreach (var layer in UIRoot.Instance.Layers)
            {
                if (isTop && layer.AdjustModalLayer())
                {
                    isTop = false;
                    continue;
                }
                
                layer.HideModalLayer();
            }
        }
    }
}
