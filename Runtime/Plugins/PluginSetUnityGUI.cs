using System.Collections;
using PluginSet.Core;

namespace PluginSet.UGUI
{
    [PluginRegister]
    public class PluginSetUnityGUI: PluginBase, IStartPlugin
    {
        public int StartOrder => PluginsStartOrder.AppStart;
        public bool IsRunning { get; private set; }
        public override string Name => "UnityGUI";

        private PluginSetUnityGUIConfig _config;

        protected override void Init(PluginSetConfig config)
        {
            _config = config.Get<PluginSetUnityGUIConfig>();
        }

        public IEnumerator StartPlugin()
        {
            if (IsRunning)
                yield break;
            
            IsRunning = true;

            UIConfig.ModalLayerAsset = _config.ModalLayerAsset;
            UIConfig.ModalLayerColor = _config.ModalLayerColor;
        }

        public void DisposePlugin(bool isAppQuit = false)
        {
            if (isAppQuit) return;
            
            UIManager.CloseAll();
            UIWindow.ClearInstances();
            UIObjectPool.ClearAll();
            UIPackage.RemoveAllPackages();
        }
    }
}