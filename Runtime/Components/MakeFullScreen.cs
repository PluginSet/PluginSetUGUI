using UnityEngine;

namespace PluginSet.UGUI
{
    [RequireComponent(typeof(RectTransform))]
    public class MakeFullScreen: MonoBehaviour
    {
        private void OnEnable()
        {
            var selfRectTransform = GetComponent<RectTransform>();
            selfRectTransform.anchorMin = Vector2.zero;
            selfRectTransform.anchorMax = Vector2.zero;

            var scale = selfRectTransform.parent.lossyScale;
            var rootScale = UIRoot.Instance.RectTransform.lossyScale;
            selfRectTransform.sizeDelta = new Vector2(Screen.width * rootScale.x / scale.x, Screen.height * rootScale.y / scale.y);
            selfRectTransform.position = Vector3.zero;
        }
    }
}