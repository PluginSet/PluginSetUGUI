using System;
using System.Collections;
using System.Collections.Generic;
using PluginSet.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PluginSet.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIWindowBase: BranchAssetAdaptor, IUIEntity
    {
        public Action<Action> EnterAction { get; set; }
        public Action<Action> ExitAction { get; set; }

        public int Tag { get; set; }

        public virtual bool IsModal { get; }

        public bool IsShown { get; protected set; }
        
        private string _panelUrl { get; set; }
        private bool _asyncOpen { get; set; }
        private string[] _dependentPackages { get; set; }

        private List<AsyncOperationHandle> _asyncHandles { get; set; }
        
        private List<UIPackage> _dependencies { get; set; }

        [SerializeField]
        protected RectTransform Panel { get; set; }
        
        private RectTransform _transform { get; set; }

        private bool _inited;

        public virtual RectTransform GetContent()
        {
            return Panel;
        }

        public void Constructor(string url, string[] dependentPackages, bool asyncOpen = true)
        {
            _panelUrl = url;
            _dependentPackages = dependentPackages;
            _asyncOpen = asyncOpen;

            Init();
        }

        private void Init()
        {
            _transform = gameObject.GetComponent<RectTransform>();
            if (_transform == null)
                _transform = gameObject.AddComponent<RectTransform>();
            
            _transform.anchorMin = Vector2.zero;
            _transform.anchorMax = Vector2.one;
            
            if (_dependentPackages != null && _dependentPackages.Length > 0)
            {
                if (_asyncOpen)
                {
                    foreach (var package in _dependentPackages)
                    {
                        AddInitAsyncHandle(UnityGUIUtils.PreparePackages(package));
                    }
                }
            }
        }

        protected override void OnAssetChanged(Object asset)
        {
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            if (InitAsyncAllCompleted())
            {
                InitPanel();
                return;
            }

            StartCoroutine(InitPanelAfterAsyncCompleted());
        }

        protected override void OnEnable()
        {
            if (Panel == null)
                return;
            
            if (!_inited)
            {
                OnInit();
                _inited = true;
            }
            
            if (Panel != null && !IsShown)
                DoShowAnimation();
        }

        protected override void OnDisable()
        {
            if (!_inited)
                return;
            
            if (IsShown)
            {
                IsShown = false;
                BeforeHide();
            }
            if (IsModal)
                UIManager.AdjustModalLayer();
            OnHide();
        }

        protected virtual void OnDestroy()
        {
            if (_dependencies != null)
            {
                foreach (var package in _dependencies)
                {
                    UnityGUIUtils.ReleasePackage(package);
                }
                _dependencies.Clear();
                _dependencies = null;
            }

            if (_asyncHandles != null && _dependentPackages != null && _dependentPackages.Length > 0)
            {
                foreach (var packageName in _dependentPackages)
                {
                    UnityGUIUtils.UnPreparePackage(packageName);
                }
            }
        }

        private void InitPanel()
        {
            if (_dependentPackages != null && _dependentPackages.Length > 0)
            {
                _dependencies = new List<UIPackage>();
                foreach (var package in _dependentPackages)
                {
                    _dependencies.Add(UnityGUIUtils.LoadPackage(package));
                }
            }

            _asyncHandles = null;

            if (!string.IsNullOrEmpty(_panelUrl))
                Url = _panelUrl;

            if (enabled && !IsShown)
                DoShowAnimation();
        }

        private void AddPanel(PackageItem item)
        {
            var obj = UIObjectPool.Instance.Get(item);
            if (obj is null)
                throw new Exception($"Cannot create gameObject with url `{_panelUrl}`");

            var panel = obj.GetComponent<RectTransform>();
            panel.SetParent(_transform, false);
            obj.SetActive(true);
            Panel = panel;

            OnInit();
            _inited = true;
        }

        private void RemovePanel()
        {
            if (Panel != null)
            {
                UIObjectPool.Instance.Put(Panel.gameObject);
                Panel = null;
            }
        }
        
        private IEnumerator InitPanelAfterAsyncCompleted()
        {
            yield return new WaitUntil(InitAsyncAllCompleted);
            
            InitPanel();
        }

        private bool InitAsyncAllCompleted()
        {
            if (_asyncHandles == null)
                return true;

            foreach (var handle in _asyncHandles)
            {
                if (!handle.isDone)
                    return false;
            }
            
            _asyncHandles.Clear();

            return true;
        }
        
        public void AddInitAsyncHandle(AsyncOperationHandle handle)
        {
            if (handle == null)
                return;
            
            if (_asyncHandles == null)
                _asyncHandles = new List<AsyncOperationHandle>();
            
            _asyncHandles.Add(handle);
        }

        protected override void OnPackageItemChanged(PackageItem item)
        {
            RemovePanel();
            AddPanel(item);
        }

        protected virtual void OnInit()
        {
            
        }

        protected virtual void DoShowAnimation()
        {
            BeforeShow();

            if (EnterAction != null)
            {
                EnterAction.Invoke(Show);
            }
            else
            {
                Show();
            }
        }

        private void Show()
        {
            IsShown = true;
            OnShown();
            
            if (IsModal)
                UIManager.AdjustModalLayer();
        }

        protected virtual void DoHideAnimation()
        {
            if (IsShown)
            {
                IsShown = false;
                BeforeHide();
            }

            if (ExitAction != null)
            {
                ExitAction.Invoke(HideImmediately);
            }
            else
            {
                HideImmediately();
            }
        }

        protected virtual void BeforeShow()
        {
            
        }

        protected virtual void OnShown()
        {
            
        }

        protected virtual void BeforeHide()
        {
            
        }

        protected virtual void OnHide()
        {
        }

        public void ShowOn(UILayer layer)
        {
            _transform.SetParent(layer.RectTransform, false);
            _transform.offsetMin = Vector2.zero;
            _transform.offsetMax = Vector2.zero;
            _transform.gameObject.SetActive(true);
            BringToFront();
        }

        public void BringToFront()
        {
            if (IsModal)
            {
                _transform.SetAsLastSibling();
                return;
            }

            var maxWinSibling = -1;
            var minModalSibling = int.MaxValue;
            foreach (var win in _transform.parent.GetComponentsInChildren<UIWindowBase>())
            {
                if (win == this)
                    continue;

                if (win.IsModal)
                    minModalSibling = Math.Min(minModalSibling, win._transform.GetSiblingIndex());
                else
                    maxWinSibling = Math.Max(maxWinSibling, win._transform.GetSiblingIndex());
            }
            if (minModalSibling == int.MaxValue)
                _transform.SetAsLastSibling();
            else
                _transform.SetSiblingIndex(Math.Min(maxWinSibling + 1, minModalSibling - 1));
        }

        public void Hide()
        {
            if (enabled)
                DoHideAnimation();
        }

        public virtual void HideImmediately()
        {
            RemovePanel();
            Object.Destroy(gameObject);
        }
    }
}