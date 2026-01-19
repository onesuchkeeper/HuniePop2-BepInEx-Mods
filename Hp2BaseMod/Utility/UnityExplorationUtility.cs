// Hp2BaseMod 2022, By OneSuchKeeper

using UnityEngine;

namespace Hp2BaseMod.Utility
{
    // For dev work, easy log calls to print info about unity structures to the log
    public static class UnityExplorationUtility
    {
        public static void LogChildren(Transform target)
        {
            for (int i = 0; i < target.childCount; i++)
            {
                var child = target.GetChild(i);
                if (child != null)
                {
                    ModInterface.Log.Message(child.name);
                    ModInterface.Log.IncreaseIndent();
                    LogChildren(child);
                    ModInterface.Log.DecreaseIndent();
                }
            }
        }

        public static void LogComponents(GameObject target)
        {
            var components = target.GetComponents<Component>();

            if (components.Length == 0)
            {
                ModInterface.Log.Message($"No Components");
            }
            else
            {
                foreach (var component in components)
                {
                    ModInterface.Log.Message($"Type: {component.GetType().Name}, Name: {component.name}");
                }
            }
        }

        public static void LogCanvas(GameObject gameObject)
        {
            var canvas = gameObject.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    ModInterface.Log.Message($"CanvasScaler- scale mode: {scaler.uiScaleMode}, refResolution:{scaler.referenceResolution}, screenMatchMode:{scaler.screenMatchMode},matchWidthOrHeight:{scaler.matchWidthOrHeight}");
                }
                else
                {
                    ModInterface.Log.Message($"CanvasScaler- NULL");
                }
            }
            else
            {
                ModInterface.Log.Message($"No Canvas Component");
            }
        }
    }
}
