// using System.Collections.Generic;
// using System.Reflection;
// using HarmonyLib;
// using UnityEngine;

// namespace SingleDate;

// [HarmonyPatch(typeof(WindowManager))]
// public static class WindowManagerPatch
// {
//     [HarmonyPatch("windowContainerLow", MethodType.Setter)]
//     [HarmonyPostfix]
//     public static void SetWindowContainerLow(WindowManager __instance)
//     {
//         ExtendedWindowManager.GetExtension(__instance).SetWindowContainerLow();
//     }

//     [HarmonyPatch("windowContainerHigh", MethodType.Setter)]
//     [HarmonyPostfix]
//     public static void SetWindowContainerHigh(WindowManager __instance)
//     {
//         ExtendedWindowManager.GetExtension(__instance).SetWindowContainerHigh();
//     }
// }

// public class ExtendedWindowManager
// {
//     private static Dictionary<WindowManager, ExtendedWindowManager> _extendedTransitions
//         = new Dictionary<WindowManager, ExtendedWindowManager>();

//     public static ExtendedWindowManager GetExtension(WindowManager __instance)
//     {
//         if (!_extendedTransitions.TryGetValue(__instance, out var extended))
//         {
//             extended = new ExtendedWindowManager(__instance);
//             _extendedTransitions[__instance] = extended;
//         }

//         return extended;
//     }

//     private static FieldInfo _windowContainerLow = AccessTools.Field(typeof(WindowManager), "_windowContainerLow");
//     private static FieldInfo _windowContainerHigh = AccessTools.Field(typeof(WindowManager), "_windowContainerHigh");

//     public Vector2 DefaultWindowContainerLowAnchor => _defaultWindowContainerLowAnchor;
//     private Vector2 _defaultWindowContainerLowAnchor;

//     public Vector2 DefaultWindowContainerHighAnchor => _defaultWindowContainerHighAnchor;
//     private Vector2 _defaultWindowContainerHighAnchor;

//     private WindowManager _core;

//     public ExtendedWindowManager(WindowManager core)
//     {
//         _core = core;
//     }

//     public void SetWindowContainerHigh()
//     {
//         if (_windowContainerHigh.GetValue(_core) is RectTransform highWindow)
//         {
//             _defaultWindowContainerHighAnchor = highWindow.anchoredPosition;
//         }
//     }

//     public void SetWindowContainerLow()
//     {
//         if (_windowContainerLow.GetValue(_core) is RectTransform lowWindow)
//         {
//             _defaultWindowContainerLowAnchor = lowWindow.anchoredPosition;
//         }
//     }

//     public void UseDefaultPosition()
//     {
//         var lowWindow = (RectTransform)_windowContainerLow.GetValue(_core);
//         lowWindow.anchoredPosition = _defaultWindowContainerLowAnchor;

//         var highWindow = (RectTransform)_windowContainerHigh.GetValue(_core);
//         highWindow.anchoredPosition = _defaultWindowContainerHighAnchor;
//     }

//     public void UseSinglePosition()
//     {
//         var delta = Game.Session.gameCanvas.header.xValues.y - Game.Session.gameCanvas.header.xValues.x;

//         var lowWindow = (RectTransform)_windowContainerLow.GetValue(_core);
//         lowWindow.anchoredPosition = new Vector2(_defaultWindowContainerLowAnchor.x + delta, _defaultWindowContainerLowAnchor.y);

//         var highWindow = (RectTransform)_windowContainerHigh.GetValue(_core);
//         highWindow.anchoredPosition = new Vector2(_defaultWindowContainerHighAnchor.x + delta, _defaultWindowContainerHighAnchor.y);
//     }
// }