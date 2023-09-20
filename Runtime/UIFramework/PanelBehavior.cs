using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PluginSet.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    public class PanelBehavior: MonoBehaviour
    {
        public virtual Action<Action> EnterAction { get; }
        public virtual Action<Action> ExitAction { get; }

        protected UIWindowBase Owner { get; private set; }
        public RectTransform Panel { get; private set; }

        private Button _closeButton;
        
        public void OnInit(UIWindowBase owner)
        {
            Panel = GetComponent<RectTransform>();
            Owner = owner;
            
            InitCommonFrame();
            InitMembers();
            OnInitUI();
        }

        private void InitCommonFrame()
        {
            var frame = Panel.Find("frame");
            if (frame == null)
                return;
            
            var closeBtn = frame.Find("closeButton");
            if (closeBtn == null)
                return;

            var btn = closeBtn.GetComponent<Button>();
            if (btn == null)
                return;
            
            btn.onClick.AddListener(HideWindow);
            _closeButton = btn;
        }

        protected virtual void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(HideWindow);
            Owner = null;
        }

        protected void HideWindow()
        {
            Owner.Hide();
        }

        protected virtual void InitMembers()
        {
            
        }

        protected virtual void OnInitUI()
        {
            
        }

        public virtual void SetData(IProxy data)
        {
        }
        
        public virtual void BeforeShow()
        {
            
        }

        public virtual void OnShown()
        {
            
        }
        
        public virtual void BeforeHide()
        {
            
        }
        
        public virtual void OnHide()
        {
            
        }
    }
    
    public abstract class PanelBehavior<T>: PanelBehavior
    {
        public override void SetData(IProxy data)
        {
            SetData((T)data);
        }

        protected abstract void SetData(T data);
    }
}