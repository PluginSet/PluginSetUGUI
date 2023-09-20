using System.Collections.Generic;

namespace PluginSet.UGUI
{
    public class UIWindow: UIWindowBase
    {
        private static Dictionary<string, List<UIWindow>> s_instances = new Dictionary<string, List<UIWindow>>();

        protected virtual bool DestroyOnHide => true;

        public static UIWindow GetInstance(string name)
        {
            if (s_instances.TryGetValue(name, out var list))
            {
                return list.Count > 0 ? list[0] : null;
            }

            return null;
        }

        public static void AddInstance(string name, UIWindow window)
        {
            if (s_instances.TryGetValue(name, out var list))
            {
                list.Add(window);
            }
            else
            {
                s_instances.Add(name, new List<UIWindow>(){ window });
            }
        }

        public static void ClearInstances()
        {
            s_instances.Clear();
        }

        private static void RemoveInstance(string name, UIWindow window)
        {
            if (s_instances.TryGetValue(name, out var list))
            {
                list.Remove(window);
            }
        }

        private object[] _args;

        protected override void Awake()
        {
            AddInstance(name, this);
        }

        protected override void OnDestroy()
        {
            RemoveInstance(name, this);
            base.OnDestroy();
        }


        public virtual void Init()
        {
            // Constructor
        }

        public void PushArgs(params object[] args)
        {
            _args = args;
            if (Panel != null && enabled)
                SetData(_args);
        }
        
        protected virtual void SetData(params object[] args)
        {
            
        }

        protected override void BeforeShow()
        {
            SetData(_args);
        }

        protected override void BeforeHide()
        {
            if (DestroyOnHide)
                RemoveInstance(name, this);
            
            base.BeforeHide();
        }
    }

    public interface IProxy
    {
    }

    public class UIWindow<T> : UIWindow where T : IProxy
    {
        protected override void SetData(params object[] args)
        {
            if (args != null && args.Length == 1 && args[0] is T data)
                SetData(data);
        }

        protected virtual void SetData(T data)
        {
            
        }
    }
}