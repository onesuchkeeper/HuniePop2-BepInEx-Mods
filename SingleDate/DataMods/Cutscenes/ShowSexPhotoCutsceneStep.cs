using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData.Interface;
using Hp2BaseMod.Utility;

namespace SingleDate;

/// <summary>
/// A cutscene step that opens the photo window with one of the girl's sex photos on a single date, and waits for the
/// window to close. Photos come from <see cref="Plugin.AddGirlSexPhotos"/>: those for the current location, or all of
/// them if none match, preferring ones she hasn't unlocked. The step completes immediately if there is nothing to show.
/// </summary>
public class ShowSexPhotoCutsceneStep : CutsceneStepSubDefinition, IFunctionalCutsceneStep
{
    // Private members of the base game's windows: the window manager's current window, the photos window's photo, and its refresh method.
    private static readonly FieldInfo f_currentWindow = AccessTools.Field(typeof(WindowManager), "_currentWindow");
    private static readonly FieldInfo f_bigPhotoDefinition = AccessTools.Field(typeof(UiWindowPhotos), "_bigPhotoDefinition");
    private static readonly MethodInfo m_refreshBigPhoto = AccessTools.Method(typeof(UiWindowPhotos), "RefreshBigPhoto");

    /// <summary>Creates the step for the base mod's cutscene data. The step has no data of its own.</summary>
    public class Info : IGameDefinitionInfo<CutsceneStepSubDefinition>
    {
        public void SetData(ref CutsceneStepSubDefinition def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            _ = gameData;
            _ = assetProvider;
            _ = insertStyle;
            def = new ShowSexPhotoCutsceneStep();
        }

        public void RequestInternals(AssetProvider assetProvider)
        {
            // no internals
        }
    }

    /// <summary>Raised when the step is done.</summary>
    public event CutsceneStepComplete Complete;

    /// <summary>Runs the step: picks and shows a photo and waits for the window to close, otherwise completes immediately.</summary>
    public void Act()
    {
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);

        if (playerFileGirlPair == null
            || !State.IsSingle(playerFileGirlPair.girlPairDefinition))
        {
            Complete?.Invoke(this);
            return;
        }

        var girlId = ModInterface.Data.GetDataId(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo);
        var girlSave = State.SaveFile.GetGirl(girlId);
        var singleDateGirl = Plugin.GetSingleDateGirl(girlId);

        if (!singleDateGirl.SexPhotos.Any())
        {
            Complete?.Invoke(this);
            return;
        }

        var locId = ModInterface.Data.GetDataId(GameDataType.Location, Game.Session.Location.currentLocation.id);

        // Photos that belong to this location.
        var sexPhotos = singleDateGirl.SexPhotos.Where(x => x.LocationId == locId).Select(x => x.PhotoId).ToArray();

        // None for this location, use any of her photos.
        if (sexPhotos.Length == 0)
        {
            sexPhotos = singleDateGirl.SexPhotos.Select(x => x.PhotoId).ToArray();
        }

        if (sexPhotos.Length == 0)
        {
            Complete?.Invoke(this);
            return;
        }

        // Prefer photos she hasn't unlocked yet, otherwise repeat one.
        var photoPool = sexPhotos.Except(girlSave.UnlockedPhotos).ToArray();
        var bigPhotoId = photoPool.Length == 0
            ? sexPhotos.GetRandom()
            : photoPool.GetRandom();

        // NOTE: the unlock is saved before the photo is confirmed to exist or show, unlike ShowDatePhotoCutsceneStep.
        girlSave.UnlockedPhotos ??= new();
        girlSave.UnlockedPhotos.Add(bigPhotoId);

        var photo = ModInterface.GameData.GetPhoto(bigPhotoId);
        if (photo == null)
        {
            Complete?.Invoke(this);
            return;
        }

        Game.Manager.Windows.WindowHideEvent += OnWindowHideEvent;

        Game.Manager.Windows.ShowWindow(ModInterface.Assets.PhotosWindow, false);

        var window = f_currentWindow.GetValue(Game.Manager.Windows);

        f_bigPhotoDefinition.SetValue(window, photo);
        m_refreshBigPhoto.Invoke(window, null);
    }

    /// <summary>The photo window closed, the step is done.</summary>
    private void OnWindowHideEvent()
    {
        Game.Manager.Windows.WindowHideEvent -= OnWindowHideEvent;
        Complete?.Invoke(this);
    }
}
