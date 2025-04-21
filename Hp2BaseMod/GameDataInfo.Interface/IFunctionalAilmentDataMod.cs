namespace Hp2BaseMod.GameDataInfo.Interface;

public interface IFunctionalAilmentDataMod : IGameDataMod<AilmentDefinition>
{
    public void Enable(Ailment ailment, bool fromTrigger);
    public void Disable(Ailment ailment, bool fromTrigger);
    public void Trigger();
}