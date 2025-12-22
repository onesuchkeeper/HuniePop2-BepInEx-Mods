using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData.Interface;
using Hp2BaseMod.Utility;

namespace SingleDate;

public class ShowDatePhotoCutsceneStep : CutsceneStepSubDefinition, IFunctionalCutsceneStep
{
    private static readonly FieldInfo f_currentWindow = AccessTools.Field(typeof(WindowManager), "_currentWindow");
    private static readonly FieldInfo f_bigPhotoDefinition = AccessTools.Field(typeof(UiWindowPhotos), "_bigPhotoDefinition");
    private static readonly MethodInfo m_refreshBigPhoto = AccessTools.Method(typeof(UiWindowPhotos), "RefreshBigPhoto");

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

    public event CutsceneStepComplete Complete;

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

        var datePercentage = girlSave.RelationshipLevel / (float)Plugin.MaxSingleGirlRelationshipLevel.Value;

        if (!singleDateGirl.DatePhotos.Any())
        {
            Complete?.Invoke(this);
            return;
        }

        var validDatePhotos = singleDateGirl.DatePhotos.Where(x => x.RelationshipPercentage <= datePercentage).Select(x => x.PhotoId).ToArray();

        if (validDatePhotos.Length == 0)
        {
            Complete?.Invoke(this);
            return;
        }

        var lockedDatePhotos = girlSave.UnlockedPhotos == null
            ? validDatePhotos
            : validDatePhotos.Except(girlSave.UnlockedPhotos).ToArray();

        var bigPhotoId = lockedDatePhotos.Length == 0
            ? validDatePhotos.GetRandom()
            : lockedDatePhotos.GetRandom();

        girlSave.UnlockedPhotos ??= new();
        girlSave.UnlockedPhotos.Add(bigPhotoId);

        var photo = ModInterface.GameData.GetPhoto(bigPhotoId);
        if (photo == null)
        {
            Complete?.Invoke(this);
            return;
        }

        Game.Manager.Windows.WindowHideEvent += OnWindowHideEvent;

        Game.Manager.Windows.ShowWindow(ModInterface.State.UiWindowPhotos, false);

        var window = f_currentWindow.GetValue(Game.Manager.Windows);

        f_bigPhotoDefinition.SetValue(window, photo);
        m_refreshBigPhoto.Invoke(window, null);
    }

    private void OnWindowHideEvent()
    {
        Game.Manager.Windows.WindowHideEvent -= OnWindowHideEvent;
        Complete?.Invoke(this);
    }
}
