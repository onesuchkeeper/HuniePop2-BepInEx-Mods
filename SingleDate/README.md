# SingleDate Mod

**SingleDate** is a mod for *HuniePop 2: Double Date* that introduces 1-on-1 "single dates" alongside the vanilla double-date mechanics. Built on **Hp2BaseMod**, it adds dedicated single-girl progression, custom cutscenes, a new Sensitivity stat, and unique UI features.

---

## Key Features

### 1-on-1 Single Dates
* **Solo Date Loop**: Dates paired with `GirlNobody` (Internal ID 0) are converted into single-girl dates.
* **Adapted Camera & Canvas**: Centers the active date partner on the right side of the screen, hiding the left doll and adjusting the puzzle grid and drop zone accordingly.
* **Action Bubbles & UI**: Replaces standard double-date interaction bubbles with streamlined single-date action bubbles.
* **Stamina Mechanics**: Stamina costs for token matches are suppressed on single dates, while stamina food and interaction limits remain balanced.

### Sensitivity System & Sensitivity Smoothies
* **Sensitivity Smoothie**: A custom item (+1 Sensitivity EXP) that can be given to girls during SIM locations.
* **Sensitivity EXP & Level**: Displayed in the cellphone app as "SENSTIV. LVL". Increasing Sensitivity EXP unlocks higher Sensitivity Levels.
* **Broken Heart Penalty**: On single dates, matching Broken Heart tokens removes a percentage of Affection based on your current Sensitivity Level.

### Progression & Photo Unlocks
* **Relationship Progression**: Track individual relationship levels with girls on single dates up to a configurable maximum level.
* **Custom Cutscenes**: Includes dedicated Single Date Meeting, Date Success, Attract, Pre-Sex, Post-Sex, and Bonus Round Success cutscenes.
* **Photo Gallery Integration**: Unlocks unique single date date photos and sex photos in the photo gallery.

---

## Deep Dive: Custom States & Scripted Ailments

### `SingleDateAilment`
`SingleDateAilment` implements `IScriptedAilment` and manages all puzzle grid overrides and reward modifications during single dates. It is applied globally at puzzle start via `ExpandedAilmentManager.AddGlobal()`.

* **Grid Configuration**:
  * Enables `SuppressStaminaCost`, `SuppressStaminaWarning`, `SuppressExhaustionWarning`, and `SuppressUpsetWarning` on `ExpandedUiPuzzleGrid`.
  * Enables `AutoRevertExhaustion` and locks doll focus switching (`SuppressFocusSwitch()`).
  * Disables stamina token spawns by adding the stamina token definition to the girl's `invalidTokenDefs`.
* **Broken Heart Penalty Handling**:
  * Listens to `_ailmentManager.PostMatchReward`.
  * When Broken Heart tokens are matched, calculates a scaled negative affection allotment:
    $$\text{penalty} = -\max\left(1, \left\lfloor \text{currentAffection} \times \text{State.GetBrokenMult()} \right\rfloor\right)$$
  * Sets `entry.Reward.ResourceValue` to the penalty so UI splash text renders `-X Affection`.
  * Deducts affection directly via `ExpandedPuzzleStatus.AddResourceValue(PuzzleResourceId.AffectionTalent, ...)`.

### `SingleDateUpsetGirlState`
`SingleDateUpsetGirlState` implements `IPuzzleStatusGirlState` to replace standard double-date upset behavior on single dates.

* **State Redirection**: Whenever a state transition to `PuzzleStatusGirlStateId.Upset` is requested on a single date, `ModEventHandles.On_RequestGirlStateTransition` redirects it to `PuzzleStatusGirlStateId.SingleDateUpset`.
* **State Lifecycle**:
  * `OnEnter()`: Targets the right doll (`gameCanvas.GetDoll(true)`), sets `GirlExpressionType.UPSET` for 4 seconds, triggers `upsetEmitter` particles, and plays dialogue trigger `dtBrokenExhausted` formatted as `UNCHECKED` so grid refreshes don't interrupt voice lines.
  * `OnMoveCompleted()`: Tracks move count in state (`_movesInState`). The upset state persists through the turn in which the match occurred ($\text{\_movesInState} = 1$) and clears at the end of the following move ($\text{\_movesInState} \ge 2$), returning the girl to `PuzzleStatusGirlStateId.Normal`.
  * `OnBrokenHeartMatched()`: Resets `_movesInState = 0`, re-applies the upset mood, and restarts the dialogue trigger.
  * `OnExit()`: Clears mood expressions and halts the particle emitter.

---

## Developer API & Interop Methods

Sub-mods (such as *HuniePopUltimate*) can interact with `SingleDate` by querying `ModInterface.TryGetInterModValue` or invoking interop methods on the mod GUID (`OSK.BepInEx.SingleDate`).

### Interop Methods

| Method Signature | Description |
| :--- | :--- |
| `AddGirlSexPhotos(RelativeId girlId, IEnumerable<(RelativeId photoId, RelativeId locationId)> photoIds)` | Registers sex photo definitions and their target location IDs for a girl. |
| `AddGirlDatePhotos(RelativeId girlId, IEnumerable<(RelativeId photoId, float relationshipPercentage)> photoIds)` | Registers date photo definitions unlocked at specific relationship milestones ($0.0$ to $1.0$). |
| `AddSexLocationBlackList(RelativeId girlId, IEnumerable<RelativeId> locationIds)` | Blacklists specific date locations from being chosen as sex date locations for a girl. |
| `SetCutsceneSuccessAttracted(RelativeId girlId, RelativeId cutsceneId)` | Overrides the Attracted date success cutscene for a specific girl. |
| `SetBonusRoundSuccessCutscene(RelativeId girlId, RelativeId cutsceneId)` | Overrides the Bonus Round success cutscene for a specific girl. |
| `SwapGirls(RelativeId girlIdA, RelativeId girlIdB)` | Swaps single date metadata and configuration between two girl definitions. |
| `SetGirlCharm(RelativeId girlId, Sprite charmSprite)` | Assigns a custom character charm sprite displayed in the cellphone status app. |
| `RegisterCharmAnimation(float weight, float cost, Func<Sequence, CharmAnimationContext, bool> build, HashSet<RelativeId> allowedCharacters)` | Registers custom DOTween idle animations for character charms on the status app. |
| `IsSexDateValid(RelativeId girlId)` | Returns `true` if the girl meets relationship level requirements for a sex date. |
| `MakeSexPhotoCutsceneStep()` | Returns an `IGameDefinitionInfo<CutsceneStepSubDefinition>` step that displays unlocked sex photos during cutscenes. |
| `MakeDatePhotoCutsceneStep()` | Returns an `IGameDefinitionInfo<CutsceneStepSubDefinition>` step that displays unlocked date photos during cutscenes. |

---

## Configuration (`SingleDate.cfg`)

The mod creates a configuration file in your BepInEx `config` directory with the following settings:

| Setting | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `SingleDateBaggage` | `bool` | `true` | Enables or disables baggage/ailments on single dates. |
| `RequireLoversBeforeThreesome` | `bool` | `false` | When enabled, both girls must reach Lovers status on single dates before double dates/threesomes can occur. |
| `ShowSingleUpsetHint` | `bool` | `false` | Toggles whether upset hints are displayed during single dates. |
| `MaxSingleGirlRelationshipLevel` | `int` | `3` | Maximum relationship level reachable on single dates. |
| `MaxSensitivityLevel` | `int` | `4` | Maximum level cap for player Sensitivity EXP. |

---

## Requirements & Dependencies

* **HuniePop 2: Double Date**
* **BepInEx 5**
* **Hp2BaseMod** (v1.0.1 or higher)
* *(Optional)* **Hp2BaseModTweaks** (for credit menu integration and charm features)

---

## Installation

1. Install **BepInEx** and **Hp2BaseMod** into your *HuniePop 2* directory.
2. Extract the `SingleDate` folder into `HuniePop 2 - Double Date/BepInEx/plugins/`.
3. Launch the game.

---

## Credits

* **Developer**: OneSuchKeeper (OSK)
* **Framework**: Hp2BaseMod
