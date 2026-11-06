using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public interface IGameStateLocation : IGameState
{
    public void ResolveGirlDefinitions(
        GirlPairDefinition pair,
        bool sidesFlipped,
        out GirlDefinition left,
        out GirlDefinition right);

    public (GirlStyleInfo left, GirlStyleInfo right, bool isCutsceneStyle) ResolveMeetingStyles(
        GirlPairDefinition pair, 
        bool sidesFlipped);

    public GirlStyleInfo ApplyStyleOverride(
        GirlDefinition def, 
        LocationDefinition loc, 
        GirlStyleInfo style, 
        bool isCutsceneStyle);

    public void ApplyDefaultDollStyles(ResolveDollStylesArgs args);

    public void ApplyStyleToDoll(GirlStyleInfo style, UiDoll doll, GirlDefinition def);

    public GirlStyleInfo BuildStyleFromFile(PlayerFileGirl file);

    /// <summary>
    /// Queues the state to switch to after departing and before arriving.
    /// </summary>
    /// <param name="payload"></param>
    public void QueueTransition(IGameStatePayload payload);

    /// <summary>
    /// Called once by ExpandedLocationManager, immediately before it fires the
    /// single post-arrival event, so a queued transition takes effect first and
    /// the new*active state is the one that receives the
    /// arrival event and runs its setup.
    /// </summary>
    bool TryConsumePendingState(out IGameStatePayload payload);
}