using System;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

/// <summary>
/// The game state while the player is with a single pair at a sim location. It is <see cref="GameStateSim"/> with the
/// single date differences: only the right doll exists, the meeting is the single date meeting, a date costs stamina
/// and picks the sex date location when it is due, and dates are played in <see cref="SingleDatePuzzleState"/>.
/// </summary>
/// <remarks>
/// Nothing asks for this state directly. <see cref="ModEventHandles.On_ResolveStatePayload"/> turns every
/// <see cref="GameStateSim.Payload"/> for a single pair into a <see cref="Payload"/>, so the states and logic actions
/// that travel to a pair don't need to know about single dates.
/// </remarks>
public sealed class SingleDateSim : GameStateLocation
{
    public class Factory : IGameStateFactory<Payload>
    {
        public IGameState Create(Payload payload) => new SingleDateSim();

        public string GetProfileString(PlayerFile playerFile)
            => playerFile.girlPairDefinition?.girlDefinitionTwo?.girlName ?? "Alone";

        public bool ValidateSave(SaveFile saveFile, out string error)
        {
            if (!ModInterface.Data.TryGetDataId(GameDataType.GirlPair, saveFile.girlPairId, out var pairId)
                || !State.IsSingle(ModInterface.GameData.GetGirlPair(pairId)))
            {
                error = "Single date sim requires a single date pair";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }

    [Serializable]
    public class Payload(RelativeId pairId) : IGameStatePayload
    {
        public RelativeId PairId = pairId;
    }

    public override RelativeId Id => GameStateIds.SingleDateSim;

    public override bool IsSavable => true;

    // ---- what the player can use here: the same as at a sim ----

    public override bool HidesCellphone(UiCellphone cellphone) => false;

    // the slots are ready once the talk / date bubbles are showing
    public override FinderStatus GetFinderStatus()
        => FinderStatus.Present(Game.Manager.Windows.IsWindowActive(Game.Session.Location.actionBubblesWindow, true, false), 0);

    // Nothing special about picking a place, the player just leaves.
    public override bool OnFinderSlotChosen(UiCellphoneAppFinder app, UiAppFinderSlot slot) => false;

    public override bool AllowsStore => true;
    public override bool StoreShowsNotice => false;
    public override bool AllowsPairApp => true;
    public override bool HasGirlsPresent => true;
    public override bool ShowsHubMenu => false;

    public override string GetGiftError(ItemDefinition item)
    {
        if (Game.Session.Location.currentGirlPair == null) return "There are no girls nearby to give gifts to.";
        if (!Game.Manager.Windows.IsWindowActive(Game.Session.Location.actionBubblesWindow, true, false)) return "You can't give gifts while you're talking.";
        return null;
    }

    public override bool DialogIgnoresCellphone => false;
    public override bool DollFocusIsInteractive => false;
    public override bool HintsSimStaminaCost => true;
    public override bool AutosavesOnArrival => true;
    public override bool AllowsPause => true;
    public override bool UseStylesOnDatesFlag => false;

    private readonly SingleDateLocationBehavior _behavior;
    private bool _disposed;

    private Action _continueTransitionCallback;
    private UiDoll _targetDoll;

    public SingleDateSim() : base()
    {
        _behavior = new SingleDateLocationBehavior();

        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PostLocationSettled += OnPostLocationSettled;
        locationManager.ResolveDollStyles += OnResolveDollStyles;
        locationManager.LocationArrived += On_LocationArrived;

        ModInterface.Events.FinderSlotSelected += On_FinderSlotSelected;
        ModInterface.Events.DateLocationSelected += On_DateLocationSelected;
        ModInterface.Events.FavQuestionResponse += On_FavQuestionResponse;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            _pendingState = null;
            _behavior.Dispose();

            var locationManager = Game.Session.Location.GetExpansion();
            locationManager.PostLocationSettled -= OnPostLocationSettled;
            locationManager.ResolveDollStyles -= OnResolveDollStyles;
            locationManager.LocationArrived -= On_LocationArrived;

            ModInterface.Events.FinderSlotSelected -= On_FinderSlotSelected;
            ModInterface.Events.DateLocationSelected -= On_DateLocationSelected;
            ModInterface.Events.FavQuestionResponse -= On_FavQuestionResponse;
        }

        base.Dispose(disposing);
    }

    // ---- arrival ----

    private void On_LocationArrived(LocationArriveArgs args)
    {
        var locationManager = Game.Session.Location;

        if (args.girlPairDef != null)
        {
            // there is no puzzle state to ask here, so the status is built with the single date's rules, which also
            // keep the right girl focused
            using var rules = new SingleDateRules(this);
            Game.Session.Puzzle.puzzleStatus.GetExpansion().Reset(rules, locationManager.currentGirlLeft, locationManager.currentGirlRight);
            Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl.staminaFreeze = -1;
            Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.staminaFreeze = -1;
        }
        else
        {
            Game.Session.Puzzle.puzzleStatus.Clear();
        }

        FinishArrival(args, processArriveBundles: false);

        if (args.girlPairDef == null || args.girlPairDef.specialPair) return;

        var playerFile = Game.Persistence.playerFile;

        var girlOne = playerFile.GetPlayerFileGirl(args.girlPairDef.girlDefinitionOne);
        if (girlOne != null) girlOne.playerMet = true;

        var girlTwo = playerFile.GetPlayerFileGirl(args.girlPairDef.girlDefinitionTwo);
        if (girlTwo != null) girlTwo.playerMet = true;

        var pairFile = playerFile.GetPlayerFileGirlPair(args.girlPairDef);
        if (pairFile.relationshipType == GirlPairRelationshipType.UNKNOWN)
        {
            SingleDateRelationship.LevelUp(pairFile);
            args.arrivalCutscene = UiPrefabs.SingleCutsceneMeeting;
        }
    }

    private void OnResolveDollStyles(ResolveDollStylesArgs args) => ApplyDefaultDollStyles(args);

    private void OnPostLocationSettled(LocationSettledArgs args)
    {
        Game.Manager.Windows.ShowWindow(args.actionBubblesWindow ?? Game.Session.Location.actionBubblesWindow, false);

        if (args.hasArrivalCutscene) return;

        // The right doll is the only doll.
        var dtGreetings = Game.Session.Location.dtGreetings;
        var greetingIndex = Mathf.Clamp(Game.Persistence.playerFile.daytimeElapsed % 4, 0, dtGreetings.Length - 1);

        Game.Session.gameCanvas.dollRight.ReadDialogTrigger(dtGreetings[greetingIndex], DialogLineFormat.PASSIVE, -1);
        ModInterface.Log.Message("GameState.SingleDateSim: settled - greeted player");
    }

    // ---- leaving ----

    private void On_FinderSlotSelected(UiAppFinderSlot slot)
    {
        // The resolver turns this into a SingleDateSim.Payload if the pair is a single pair.
        _pendingState = slot.locationDefinition?.ModId() == Locations.HotelRoom
            ? new GameStateHub.Payload()
            : new GameStateSim.Payload(slot.playerFileFinderSlot.girlPairDefinition.ModId());
    }

    public override void DepartTransition(IGameStatePayload payload, Action continueTransitionCallback)
    {
        if (payload != null)
        {
            _pendingState = payload;
        }

        var dialogTriggerDefinition = _pendingState is GameStatePuzzle.Payload
            ? Game.Session.Location.dtAskDate
            : Game.Session.Location.dtValediction;

        if (dialogTriggerDefinition == null)
        {
            continueTransitionCallback();
            return;
        }

        _targetDoll = Game.Session.gameCanvas.dollRight;
        _continueTransitionCallback = continueTransitionCallback;
        _targetDoll.ReadDialogTrigger(dialogTriggerDefinition, DialogLineFormat.ACTIVE, -1);
        _targetDoll.DialogBoxHiddenEvent += OnValedictionDialogRead;
    }

    private void OnValedictionDialogRead(UiDoll doll)
    {
        if (_targetDoll == null)
        {
            ModInterface.Log.Warning("Valediction triggered with no target doll");
            return;
        }

        _targetDoll.DialogBoxHiddenEvent -= OnValedictionDialogRead;
        _continueTransitionCallback?.Invoke();
        _continueTransitionCallback = null;
    }

    // ---- asking for a date ----

    /// <summary>
    /// Controls how a date starts. Denies the date if the girl has no stamina, zeroes the stamina gain and sets the points
    /// for starting the date, and chooses the sex date location when the sex date is due.
    /// </summary>
    /// <remarks>
    /// Single dates follow HuniePop 1, where a date costs a stamina and starting one doesn't restore any.
    /// <para>When the sex date is due (attracted, one level below max, at the pair's sex time of day, and the girl has a
    /// <see cref="SingleDateGirl"/> entry) the location is picked in this order: a random location from her sex photos
    /// (<see cref="Plugin.AddGirlSexPhotos"/>) that isn't <see cref="RelativeId.Default"/>; else the pair's sex location;
    /// else a random valid date location that is not in her sex location blacklist
    /// (<see cref="Plugin.AddSexLocationBlackList"/>). If the sex date isn't due, the location is cleared so the normal
    /// date location logic applies.</para>
    /// </remarks>
    private void On_DateLocationSelected(DateLocationSelectedArgs args)
    {
        var pairFile = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);
        if (pairFile == null) return;

        var statusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(true);
        if (statusGirl.stamina < 1)
        {
            ModInterface.Log.Message("Single Date Deny due to no stamina");
            args.DenyDate = true;
            // exhausted
            Game.Session.gameCanvas.GetDoll(true).ReadDialogTrigger(
                ModInterface.GameData.GetDialogTrigger(Hp2BaseMod.DialogTriggers.StaminaInsufficient),
                DialogLineFormat.PASSIVE);
            return;
        }

        // Starting a single date gives no stamina back (as in HuniePop 1).
        args.LeftStaminaGain = 0;
        args.RightStaminaGain = 0;

        // Points for starting the date, all go to the player and the right girl as the left is Nobody.
        args.PlayerPoints = 5;
        args.LeftPoints = 0;
        args.RightPoints = 5;

        // null when the sex date isn't due, which clears what the base game chose and lets a normal location be picked.
        args.Location = ChooseSexDateLocation(pairFile);

        _pendingState = new GameStatePuzzle.Payload(PuzzleStateIds.SingleDatePuzzle);
    }

    private static LocationDefinition ChooseSexDateLocation(PlayerFileGirlPair pairFile)
    {
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, pairFile.girlPairDefinition.girlDefinitionTwo.id);
        var girlSave = State.SaveFile.GetGirl(girlId);
        var maxLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;

        if (!(pairFile.relationshipType == GirlPairRelationshipType.ATTRACTED
            && girlSave?.RelationshipLevel == maxLevel - 1
            // daytimeElapsed counts every time of day passed, there are 4 per day, so % 4 is the current ClockDaytimeType.
            && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)pairFile.girlPairDefinition.sexDaytime
            && Plugin.TryGetSingleDateGirl(girlId, out var singleDateGirl)))
        {
            return null;
        }

        // pick a photo with a location
        if (singleDateGirl.SexPhotos.Any()
            && singleDateGirl.SexPhotos.Select(x => x.LocationId).ToArray().GetRandom() is var randomLocId
            && randomLocId != RelativeId.Default)
        {
            return ModInterface.GameData.GetLocation(randomLocId);
        }

        // otherwise (no photos, or the photo has no location) use the pair's sex location, or if the pair has none either
        // pick a random valid date location that isn't blacklisted for this girl
        var location = pairFile.girlPairDefinition.sexLocationDefinition;
        if (location != null) return location;

        var time = (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4);
        Game.Data.Locations.GetAll()
            .Where(x => x.GetExpansion().IsValidForNormalDate(time)
                && !singleDateGirl.SexLocBlackList.Contains(ModInterface.Data.GetDataId(GameDataType.Location, x.id)))
            .ToArray()
            .TryGetRandom(out location);

        return location;
    }

    // ---- talking ----

    /// <summary>The empty other girl doesn't react to favorite question talk.</summary>
    private void On_FavQuestionResponse(TalkFavQuestionResponseArgs args) => args.OtherGirlResponds = false;

    public override IGameStatePayload GeneratePayload()
        => new Payload(Game.Persistence.playerFile.girlPairDefinition.ModId());
}