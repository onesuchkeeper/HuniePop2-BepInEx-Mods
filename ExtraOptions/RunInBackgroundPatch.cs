﻿using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.ModLoader;
using UnityEngine;

namespace Hp2ExtraOptions
{
    [HarmonyPatch(typeof(UiCellphoneAppCode), "OnSubmitButtonPressed")]
    public static class RunInBackgroundPatch
    {
        public static void Postfix()
        {
            Application.runInBackground = GameDefinitionProvider.IsCodeUnlocked(Constants.RunInBackgroundCodeId);
        }
    }

    [HarmonyPatch(typeof(UiTitleCanvas), "OnInitialAnimationComplete")]
    public static class RunInBackgroundLoadPatch
    {
        public static void Postfix()
        {
            Application.runInBackground = GameDefinitionProvider.IsCodeUnlocked(Constants.RunInBackgroundCodeId);
        }
    }
}
