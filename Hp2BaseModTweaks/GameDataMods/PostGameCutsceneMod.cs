using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseModTweaks;

internal class PostGameCutsceneMod : IGameDataMod<CutsceneDefinition>
{
    private static readonly string _itemNotifierWindow = "ItemNotifierWindow";
    private static readonly RelativeId _id = new RelativeId(-1, 171);
    public RelativeId Id => _id;

    public int LoadPriority => 0;

    public IEnumerable<string> GetInternalAudioRequests() => null;

    public IEnumerable<string> GetInternalSpriteRequests() => null;

    /// <inheritdoc/>
    public void SetData(CutsceneDefinition def, GameDefinitionProvider gameData, AssetProvider assetProvider)
    {
        if (!def.steps.TryGetFirst(x => x.stepType == CutsceneStepType.SHOW_WINDOW && x.windowPrefab != null, out var template))
        {
            ModInterface.Log.LogWarning("Failed to find window to use as template");
            return;
        }

        def.steps.Add(CutsceneStepUtility.MakeGameAction(new LogicAction()
        {
            type = LogicActionType.SET_FLAG,
            stringValue = "notification_item_id",
            intValue = ModInterface.Data.GetRuntimeDataId(GameDataType.Item, new RelativeId(Plugin.ModId, 0))

        }, CutsceneStepProceedType.AUTOMATIC));
        def.steps.Add(CutsceneStepUtility.MakeShowWindow(true, assetProvider.GetInternalAsset<UiWindowItemNotifier>(_itemNotifierWindow), CutsceneStepProceedType.AUTOMATIC));
    }

    /// <inheritdoc/>
    public void RequestInternals(AssetProvider assetProvider)
    {
        assetProvider.RequestInternal(typeof(UiWindowItemNotifier), _itemNotifierWindow);
    }
}
