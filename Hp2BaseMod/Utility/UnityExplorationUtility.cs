﻿// Hp2BaseMod 2022, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
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
                    ModInterface.Log.LogInfo(child.name);
                    ModInterface.Log.IncreaseIndent();
                    LogChildren(child);
                    ModInterface.Log.DecreaseIndent();
                }
            }
        }

        public static void LogComponents(GameObject target) => LogComponents(target, new List<Component>() { });

        private static void LogComponents(GameObject target, List<Component> excluded)
        {
            var components = target.GetComponents<Component>();

            if (components.Length == 0)
            {
                ModInterface.Log.LogInfo($"No Components");
            }
            else
            {
                foreach (var component in components)
                {
                    ModInterface.Log.LogInfo($"Type: {component.GetType().Name}, Name: {component.name}");

                    ModInterface.Log.IncreaseIndent();
                    if (!excluded.Contains(component))
                    {
                        excluded.Add(component);
                        LogComponents(component.gameObject, excluded.ToList());
                    }
                    else
                    {
                        ModInterface.Log.LogInfo("Has already been logged, avoiding recursion");
                    }
                    ModInterface.Log.DecreaseIndent();
                }
            }
        }
    }
}
