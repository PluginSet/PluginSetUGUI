using UnityEngine;

namespace PluginSet.UGUI
{
    public class PanelWindowBase: UIWindow<IProxy>
    {
        protected virtual PanelBehavior PanelBehavior { get; set; }
        protected override bool DestroyOnHide => true;
        
        protected override void OnInit()
        {
            base.OnInit();
            if (PanelBehavior == null)
                PanelBehavior = Panel.GetComponent<PanelBehavior>();
            
            if (PanelBehavior != null)
            {
                PanelBehavior.OnInit(this);
                EnterAction = PanelBehavior.EnterAction;
                ExitAction = PanelBehavior.ExitAction;
            }
        }

        protected override void SetData(IProxy data)
        {
            if (PanelBehavior != null)
                PanelBehavior.SetData(data);
        }

        protected override void BeforeShow()
        {
            base.BeforeShow();
            if (PanelBehavior != null)
                PanelBehavior.BeforeHide();
        }

        protected override void OnShown()
        {
            base.OnShown();
            if (PanelBehavior != null)
                PanelBehavior.OnShown();
        }

        protected override void BeforeHide()
        {
            if (PanelBehavior != null)
                PanelBehavior.BeforeHide();
            base.BeforeHide();
        }

        protected override void OnHide()
        {
            if (PanelBehavior != null)
                PanelBehavior.OnHide();
            base.OnHide();
        }

        public override void HideImmediately()
        {
            if (DestroyOnHide)
                base.HideImmediately();
            else
                gameObject.SetActive(false);
        }
    }
    
    public abstract class PanelWindow<T>: PanelWindowBase where T : IProxy
    {
        protected override void SetData(IProxy data)
        {
            if (data is T t)
                SetData(t);
        }

        protected abstract void SetData(T data);
    }

    public sealed class PanelWindow : PanelWindowBase
    {
        [SerializeField]
        private PanelBehavior panelBehavior;
        
        public bool destroyOnHide;
        
        public bool directShow;

        [SerializeField]
        private bool isModal;

        protected override bool DestroyOnHide => destroyOnHide;
        public override bool IsModal => isModal;

        protected override PanelBehavior PanelBehavior { get => panelBehavior; set => panelBehavior = value; }

        private bool _isFirstAwake = true;

        protected override void Awake()
        {
            if (_isFirstAwake)
            {
                UIManager.RegisterWindow(this.name, this.GetType());
                Constructor(null, null, false);
                
                if (!directShow)
                    this.gameObject.SetActive(false);
                
                _isFirstAwake = false;
            }
            
            Panel = panelBehavior.GetComponent<RectTransform>();
            base.Awake();
        }

    }
}