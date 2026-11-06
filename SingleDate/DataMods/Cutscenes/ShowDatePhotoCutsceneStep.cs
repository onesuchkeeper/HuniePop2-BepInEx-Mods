using System;
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
/// A cutscene step that opens the photo window with a date photo for the girl on a single date, and waits for the
/// window to close. The photo is chosen from her date photos (<see cref="Plugin.AddGirlDatePhotos"/>) that have
/// unlocked at her current level, preferring photos she hasn't seen. If there is nothing to show (not a single
/// date, no photos, or any error) the step completes immediately, and never blocks the cutscene.
/// </summary>
public class ShowDatePhotoCutsceneStep : CutsceneStepSubDefinition, IFunctionalCutsceneStep
{
    // Private members of the base game's windows: the window manager's current window, the photos window's photo, and its refresh method.
    private static readonly FieldInfo f_currentWindow = AccessTools.Field(typeof(WindowManager), "_currentWindow");
    private static readonly FieldInfo f_bigPhotoDefinition = AccessTools.Field(typeof(UiWindowPhotos), "_bigPhotoDefinition");
    private static readonly MethodInfo m_refreshBigPhoto = AccessTools.Method(typeof(UiWindowPhotos), "RefreshBigPhoto");

    // So Complete is only raised once, as the step can finish from a failure or from the window closing.
    private bool _completed;

    /// <summary>Creates the step for the base mod's cutscene data. The step has no data of its own.</summary>
    public class Info : IGameDefinitionInfo<CutsceneStepSubDefinition>
    {
        public void SetData(ref CutsceneStepSubDefinition def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            _ = gameData;
            _ = assetProvider;
            _ = insertStyle;
            def = new ShowDatePhotoCutsceneStep();
        }

        public void RequestInternals(AssetProvider assetProvider)
        {
            // no internals
        }
    }

    /// <summary>Raised when the step is done.</summary>
    public event CutsceneStepComplete Complete;

    /// <summary>Runs the step: shows the photo if there is one and waits for the window to close, otherwise finishes.</summary>
    public void Act()
    {
        _completed = false;

        var waitingOnWindow = false;
        try
        {
            waitingOnWindow = TryShowPhoto();
        }
        catch (Exception e)
        {
            ModInterface.Log.Error($"{nameof(ShowDatePhotoCutsceneStep)} failed, skipping step: {e}");
        }

        // Nothing to wait for (no photo, bad data, or an error): finish immediately.
        if (!waitingOnWindow)
        {
            Finish();
        }
    }

    /// <summary>
    /// Returns true only if the photo window is up and we're waiting for it to close.
    /// </summary>
    private bool TryShowPhoto()
    {
        if (f_currentWindow == null || f_bigPhotoDefinition == null || m_refreshBigPhoto == null)
        {
            ModInterface.Log.Error($"{nameof(ShowDatePhotoCutsceneStep)}: required UiWindowPhotos/WindowManager members not found.");
            return false;
        }

        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);
        var pairDef = playerFileGirlPair?.girlPairDefinition;

        if (pairDef?.girlDefinitionTwo == null || !State.IsSingle(pairDef))
        {
            return false;
        }

        var girlId = ModInterface.Data.GetDataId(pairDef.girlDefinitionTwo);
        var girlSave = State.SaveFile.GetGirl(girlId);
        var datePhotos = Plugin.GetSingleDateGirl(girlId)?.DatePhotos;

        if (girlSave == null || datePhotos == null)
        {
            return false;
        }

        var maxLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;
        if (maxLevel <= 0)
        {
            return false;
        }

        var datePercentage = girlSave.RelationshipLevel / (float)maxLevel;

        // Her level as a fraction of max, a photo is a candidate once she has reached its percentage.
        // Also drops duplicate ids and photo ids that don't resolve to a photo.
        var candidates = datePhotos
            .Where(x => x.RelationshipPercentage <= datePercentage)
            .Select(x => x.PhotoId)
            .Distinct()
            .Select(id => (Id: id, Photo: ModInterface.GameData.GetPhoto(id)))
            .Where(x => x.Photo is not null)
            .ToArray();

        if (candidates.Length == 0)
        {
            return false;
        }

        // Prefer photos she hasn't unlocked yet; otherwise repeat one.
        var unlocked = girlSave.UnlockedPhotos;
        var locked = unlocked == null
            ? candidates
            : candidates.Where(x => !unlocked.Contains(x.Id)).ToArray();

        var pick = (locked.Length > 0 ? locked : candidates).GetRandom();

        // Subscribe before showing, so a window that closes right away isn't missed. Finish unsubscribes, including on the early return below.
        Game.Manager.Windows.WindowHideEvent += OnWindowHideEvent;
        Game.Manager.Windows.ShowWindow(ModInterface.Assets.PhotosWindow, false);

        if (f_currentWindow.GetValue(Game.Manager.Windows) is not UiWindowPhotos window)
        {
            return false; // Finish() will unsubscribe
        }

        f_bigPhotoDefinition.SetValue(window, pick.Photo);
        m_refreshBigPhoto.Invoke(window, null);

        // Only record the unlock once the photo is actually on screen.
        girlSave.UnlockedPhotos ??= new();
        if (!girlSave.UnlockedPhotos.Contains(pick.Id))
        {
            girlSave.UnlockedPhotos.Add(pick.Id);
        }

        return true;
    }

    /// <summary>The photo window closed, the step is done.</summary>
    private void OnWindowHideEvent() => Finish();

    /// <summary>Stops listening for the window closing and raises Complete once.</summary>
    private void Finish()
    {
        Game.Manager.Windows.WindowHideEvent -= OnWindowHideEvent;

        if (_completed)
        {
            return;
        }

        _completed = true;
        Complete?.Invoke(this);
    }
}
