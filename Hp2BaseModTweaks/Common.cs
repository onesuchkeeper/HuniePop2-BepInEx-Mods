﻿// Hp2Sample 2022, By OneSuchKeeper

using System.Collections.Generic;

namespace Hp2BaseModTweaks
{
    internal static class Common
    {
        public static readonly string Ui_PhotoAlbumSlot = "ui_photo_album_slot";
        public static readonly string Ui_PhotoButtonLeft = "ui_photo_button_left";
        public static readonly string Ui_PhotoButtonRight = "ui_photo_button_right";
        public static readonly string Ui_AppSettingArrowLeft = "ui_app_setting_arrow_left";
        public static readonly string Ui_AppSettingArrowLeftOver = "ui_app_setting_arrow_left_over";
        public static readonly string Ui_AppSettingArrowRight = "ui_app_setting_arrow_right";
        public static readonly string Ui_AppSettingArrowRightOver = "ui_app_setting_arrow_right_over";

        public static IEnumerable<string> AllSprites()
        {
            yield return Ui_PhotoAlbumSlot;
            yield return Ui_PhotoButtonLeft;
            yield return Ui_PhotoButtonRight;
            yield return Ui_AppSettingArrowLeft;
            yield return Ui_AppSettingArrowLeftOver;
            yield return Ui_AppSettingArrowRight;
            yield return Ui_AppSettingArrowRightOver;
        }

        public static readonly string Sfx_PhoneAppButtonPressed = "sfx_phone_app_button_pressed";
    }
}
