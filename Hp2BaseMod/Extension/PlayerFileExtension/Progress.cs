namespace Hp2BaseMod.Extension;

public static class PlayerFileExtensions
{
    public static bool IsProgressWingsCollected(this PlayerFile file) => file != null && file.storyProgress >= PlayerFile.STORY_PROGRESS_WINGS_COLLECTED;
    public static bool IsProgressNymphojinnReady(this PlayerFile file) => file != null && file.storyProgress >= PlayerFile.STORY_PROGRESS_NYMPHOJINN_READY;
    public static bool IsProgressVolcano(this PlayerFile file) => file != null && file.storyProgress >= PlayerFile.STORY_PROGRESS_VOLCANO;
    public static bool IsProgressNymphojinnPreDate(this PlayerFile file) => file != null && file.storyProgress >= 10;
    public static bool IsProgressPoolsideEnding(this PlayerFile file) => file != null && file.storyProgress >= PlayerFile.STORY_PROGRESS_POOLSIDE_ENDING;
    public static bool IsProgressPostGame(this PlayerFile file) => file != null && file.storyProgress >= PlayerFile.STORY_PROGRESS_POST_GAME;
    public static bool IsProgressComplete(this PlayerFile file) => file != null && file.storyProgress >= PlayerFile.STORY_PROGRESS_COMPLETE;
}