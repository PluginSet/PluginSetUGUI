using UnityEngine;

namespace PluginSet.UGUI
{
    public class PluginSetUnityGUIConfig: ScriptableObject
    {
        public string ModalLayerAsset;
        public Color ModalLayerColor = new Color(0, 0, 0, 0.5f);

        public void CloneTo(PluginSetUnityGUIConfig config)
        {
            config.ModalLayerAsset = ModalLayerAsset;
            config.ModalLayerColor = ModalLayerColor;
        }
    }
}