using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

/// <summary>
/// Active for LocationType.DATE locations.
/// </summary>
public sealed class GameStatePuzzle : GameStateLocation
{
    public override RelativeId Id => GameStateId.Puzzle;

    public override bool IsSavable => false;

    public override void Enter()
    {
        base.Enter();
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PreLocationSettled += OnPreLocationSettled;
        locationManager.ResolveDollStyles += OnResolveDollStyles;
        locationManager.ArrivalCompleted += On_ArrivalCompleted;
        locationManager.LocationArrived += On_LocationArrived;

        ModInterface.Events.FinderSlotSelected += On_FinderSlotSelected;
    }

    public override void Exit()
    {
        base.Exit();
        _pendingState = null;
        var locationManager = Game.Session.Location.GetExpansion();
        locationManager.PreLocationSettled -= OnPreLocationSettled;
        locationManager.ResolveDollStyles -= OnResolveDollStyles;
        locationManager.ArrivalCompleted -= On_ArrivalCompleted;
        locationManager.LocationArrived -= On_LocationArrived;

        ModInterface.Events.FinderSlotSelected -= On_FinderSlotSelected;
    }

    public override string GetProfileString(PlayerFile playerFile)
    {
        var pair = Game.Persistence.playerFile.girlPairDefinition;
        return pair == null
            ? "On a date"
            : $"On a date with {pair.girlDefinitionOne.girlName} & {pair.girlDefinitionTwo.girlName}";
    }

    private void OnPreLocationSettled(LocationSettledArgs args)
    {
        ModInterface.Log.Message("GameState.Puzzle: settled - starting puzzle");
        Game.Session.Puzzle.StartPuzzle();
    }

    private void OnResolveDollStyles(ResolveDollStylesArgs args)
    {
        if (args.GirlPair == null) return;

        var playerPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(args.GirlPair);
        if (playerPair == null) return;

        ResolveGirlDefinitions(args.GirlPair, args.SidesFlipped, out var leftDef, out var rightDef);

        (GirlStyleInfo left, GirlStyleInfo right, bool isCutsceneStyle) result;

        if (playerPair.relationshipType == GirlPairRelationshipType.UNKNOWN)
        {
            result = ResolveMeetingStyles(args.GirlPair, args.SidesFlipped);
        }
        else
        {
            var preDateArgs = BuildPreDateArgs(playerPair, args.Location);
            ModInterface.Events.NotifyPreDateDollReset(preDateArgs);

            result = preDateArgs.Style switch
            {
                PreDateDollResetArgs.StyleType.Sex =>
                    ResolveSexStyles(args.GirlPair, args.Location, leftDef, rightDef, args.SidesFlipped),

                PreDateDollResetArgs.StyleType.Location =>
                    ResolveLocationStyles(args.Location, leftDef, rightDef),

                _ => ResolveFileStyles()
            };
        }

        var leftStyle = ApplyStyleOverride(leftDef, args.Location, result.left, result.isCutsceneStyle);
        var rightStyle = ApplyStyleOverride(rightDef, args.Location, result.right, result.isCutsceneStyle);

        ApplyStyleToDoll(leftStyle, Game.Session.gameCanvas.dollLeft, leftDef);
        ApplyStyleToDoll(rightStyle, Game.Session.gameCanvas.dollRight, rightDef);
    }

    private void On_FinderSlotSelected(UiAppFinderSlot slot)
    {
        // I'd like a better way to check this. Maybe I could add
        // a flag to the hub slot itself?
        if (slot.locationDefinition?.ModId() == Locations.HotelRoom)
        {
            _pendingState = GameStateId.Hub;
        }
        else
        {
            _pendingState = GameStateId.Sim;
        }
    }

    /// <summary>
    /// Determines which style bucket a date should use. Mirrors the original
    /// branching: sex styles on the scheduled sex daytime, else location styles
    /// mid-puzzle (off the boss location), else the player's saved file style.
    /// </summary>
    private PreDateDollResetArgs BuildPreDateArgs(PlayerFileGirlPair playerPair, LocationDefinition currentLocation)
    {
        var args = new PreDateDollResetArgs();

        if (playerPair.relationshipType == GirlPairRelationshipType.ATTRACTED &&
            Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerPair.girlPairDefinition.sexDaytime)
        {
            args.Style = PreDateDollResetArgs.StyleType.Sex;
        }
        else if (!Game.Session.Puzzle.puzzleStatus.isEmpty &&
                 currentLocation != Game.Session.Puzzle.bossLocationDefinition)
        {
            args.Style = PreDateDollResetArgs.StyleType.Location;
        }
        else
        {
            args.Style = PreDateDollResetArgs.StyleType.File;
        }

        return args;
    }

    private (GirlStyleInfo, GirlStyleInfo, bool) ResolveSexStyles(
        GirlPairDefinition pair,
        LocationDefinition location,
        GirlDefinition leftDef,
        GirlDefinition rightDef,
        bool sidesFlipped)
    {
        var pairStyle = pair.GetExpansion().PairStyle;
        if (pairStyle == null) return (null, null, false);

        var left = sidesFlipped ? pairStyle.SexGirlTwo : pairStyle.SexGirlOne;
        var right = sidesFlipped ? pairStyle.SexGirlOne : pairStyle.SexGirlTwo;

        var (locLeft, locRight, _) = ResolveLocationStyles(location, leftDef, rightDef);

        if (left != null && left.OutfitId == RelativeId.Default && left.HairstyleId == RelativeId.Default)
        {
            left = locLeft;
        }

        if (right != null && right.OutfitId == RelativeId.Default && right.HairstyleId == RelativeId.Default)
        {
            right = locRight;
        }

        return (left, right, false);
    }

    private (GirlStyleInfo, GirlStyleInfo, bool) ResolveLocationStyles(
        LocationDefinition location, GirlDefinition leftDef, GirlDefinition rightDef)
    {
        var locationId = ModInterface.Data.GetDataId(GameDataType.Location, location.id);
        var puzzle = Game.Session.Puzzle.puzzleStatus;

        var left = ResolveSingleLocationStyle(puzzle.girlStatusLeft.playerFileGirl, leftDef, locationId, "left");
        var right = ResolveSingleLocationStyle(puzzle.girlStatusRight.playerFileGirl, rightDef, locationId, "right");

        return (left, right, false);
    }

    private GirlStyleInfo ResolveSingleLocationStyle(
        PlayerFileGirl playerFile, GirlDefinition girlDef, RelativeId locationId, string sideLabel)
    {
        if (!playerFile.stylesOnDates)
        {
            var expansion = girlDef.GetExpansion();

            if (expansion.GetCurrentBody().LocationIdToOutfitId.TryGetValue(locationId, out var style))
            {
                ModInterface.Log.Message($"Using location style for {sideLabel} girl: {style}");
                return style;
            }

            ModInterface.Log.Message($"No location style found for {sideLabel} girl");
            return null;
        }

        var fileStyle = BuildStyleFromFile(playerFile);
        ModInterface.Log.Message($"Using file style for {sideLabel} girl: {fileStyle}");
        return fileStyle;
    }

    private (GirlStyleInfo, GirlStyleInfo, bool) ResolveFileStyles()
    {
        var leftFile = Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl;
        var rightFile = Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl;

        return (BuildStyleFromFile(leftFile), BuildStyleFromFile(rightFile), false);
    }

    public override void DepartTransition(RelativeId? _nextStateId, Action continueTransitionCallback)
    {
        if (_nextStateId.HasValue)
        {
            _pendingState = _nextStateId;
        }
        
        continueTransitionCallback();
    }

    private void On_ArrivalCompleted(ArrivalCompletedArgs args)
    {
        Game.Session.Puzzle.puzzleGrid.ailmentsContainer.Activate();
        if (args.ArrivalCutscene == null)
        {
            if (!Game.Session.Puzzle.puzzleStatus.IsTutorial())
            {
                args.ArrivalCutscene = Game.Session.Puzzle.cutsceneIntro;
            }
            else
            {
                args.ArrivalCutscene = Game.Session.Puzzle.cutsceneTutorialIntro;
            }
        }
    }

    private void On_LocationArrived(LocationArriveArgs args)
    {
        // NOTE: this first Reset ran unconditionally in the old LocationArriving
        // handler, while the Reset just below only fires when isEmpty/initialArrive.
        // Kept both since they're not provably equivalent (the isEmpty check below
        // can be false here while this one still needs to run, e.g. moving between
        // locations mid-date) - but if you know puzzleStatus well enough to say the
        // first one is always superseded, it can go.
        if (args.girlPairDef != null)
        {
            ResolveGirlDefinitions(args.girlPairDef, args.sidesFlipped, out var left, out var right);
            Game.Session.Puzzle.puzzleStatus.Reset(left, right);
        }

        var locationManager = Game.Session.Location;

        if ((args.initialArrive || Game.Session.Puzzle.puzzleStatus.isEmpty) && args.girlPairDef != null)
        {
            Game.Session.Puzzle.puzzleStatus.Reset(locationManager.currentGirlLeft, locationManager.currentGirlRight);
        }

        if (args.girlPairDef == Game.Session.Puzzle.bossGirlPairDefinition || args.girlPairDef == null)
        {
            List<GirlDefinition> allBySpecial = Game.Data.Girls.GetAllBySpecial(special: false);
            ListUtils.ShuffleList(allBySpecial);
            if (args.girlPairDef == Game.Session.Puzzle.bossGirlPairDefinition)
            {
                while (allBySpecial.Count > 8)
                {
                    allBySpecial.RemoveAt(allBySpecial.Count - 1);
                }

                allBySpecial.Add(args.girlPairDef.girlDefinitionOne);
                allBySpecial.Add(args.girlPairDef.girlDefinitionTwo);
                Game.Session.Puzzle.puzzleStatus.Reset(allBySpecial, nonstop: false);
            }
            else
            {
                Game.Session.Puzzle.puzzleStatus.Reset(allBySpecial, nonstop: true);
            }
        }

        Game.Session.Puzzle.puzzleStatus.PopulateAilments();

        FinishArrival(args);
    }
}