using UnityEngine;

namespace PluginSet.UGUI
{
    public abstract class SafeAreaCalculator: MonoBehaviour
    {
        public abstract Rect GetSafeArea();
    }
}