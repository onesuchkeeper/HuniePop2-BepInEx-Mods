namespace Hp2BaseMod;

public interface IGirlTalkHandler
{
    TalkWithType SelectTalkType(PlayerFileGirl playerFileGirl);
    ItemDefinition GetBaggageItem(PlayerFileGirl playerFileGirl);
}