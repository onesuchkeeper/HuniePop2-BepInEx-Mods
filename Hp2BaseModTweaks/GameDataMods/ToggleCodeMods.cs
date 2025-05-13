using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace Hp2BaseModTweaks;

public static class ToggleCodeMods
{
    public static RelativeId FemaleJizzToggleCodeID => _femaleJizzToggleCodeID;
    private static RelativeId _femaleJizzToggleCodeID;

    public static RelativeId SlowAffectionDrainToggleCodeID => _slowAffectionDrainToggleCodeID;
    private static RelativeId _slowAffectionDrainToggleCodeID;

    public static RelativeId RunInBackgroundCodeId => _runInBackgroundCodeId;
    private static RelativeId _runInBackgroundCodeId;

    public static RelativeId FairyWingsCodeId => _fairyWingsCodeId;
    private static RelativeId _fairyWingsCodeId;

    public static RelativeId KyuHoleCodeId => _kyuHoleCodeId;
    private static RelativeId _kyuHoleCodeId;

    public static void AddMods(int modId)
    {
        _femaleJizzToggleCodeID = new RelativeId(modId, 0);
        _slowAffectionDrainToggleCodeID = new RelativeId(modId, 1);
        _runInBackgroundCodeId = new RelativeId(modId, 2);
        _fairyWingsCodeId = new RelativeId(modId, 3);
        _kyuHoleCodeId = new RelativeId(modId, 4);

        ModInterface.AddDataMod(new CodeDataMod(_femaleJizzToggleCodeID, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("JIZZ FOR ALL"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "Female 'wet' photos enabled.",
            OffMessage = "Female 'wet' photos disabled."
        });

        ModInterface.AddDataMod(new CodeDataMod(_runInBackgroundCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("STAY FOCUSED"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "The game will continue running while unfocused.",
            OffMessage = "The game will pause when unfocused."
        });

        ModInterface.AddDataMod(new CodeDataMod(_kyuHoleCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("POR QUE NO LOS TRES"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "All three Kyu photos will be available when unlocked.",
            OffMessage = "Only the selected Kyu photo will be available when unlocked."
        });

        ModInterface.AddDataMod(new CodeDataMod(_fairyWingsCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("PINK BITCH!"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "Awh yeah! She's unstoppable! [The game must be restarted in order to take effect]",
            OffMessage = "Lack of hunies rivets us firmly to the ground, ones wings are clipped."
        });
    }
}