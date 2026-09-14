// namespace Hp2BaseMod;

// public class GameStateTitle : IGameState
// {
//     public RelativeId Id => GameStateId.Title;

//     // Title screen cannot be paused or saved [3]
//     public bool AllowPause => false;
//     public bool AllowSave => false;
//     public bool UseLeftCellphone => false;

//     public void Enter()
//     {
//         // Listen for save file selection from the title canvas
//         ModInterface.Events.PreLoadSaveFile += OnPreLoadSaveFile;
//     }

//     public void Exit()
//     {
//         // Clean up listeners when transitioning away from the title screen
//         ModInterface.Events.PreLoadSaveFile -= OnPreLoadSaveFile;
//     }

//     public void ResetDolls()
//     {
//         // Title screen manages its own cover art / canvas elements
//     }

//     private void OnPreLoadSaveFile(PlayerFile file)
//     {
//         if (file == null || file.locationDefinition == null) return;

//         // Determine target game state from the save file's current location
//         RelativeId targetStateId = GameStateId.From(file.locationDefinition.locationType);

//         ModInterface.Log.Message(
//             $"Save file selected ({file.locationDefinition.locationName}). " +
//             $"Transitioning from Title to GameState: {targetStateId}"
//         );

//         // Switch to destination state (e.g. GameStateSim, GameStateHub, or GameStateDate) [7]
//         ModInterface.GameState.ChangeState(targetStateId);
//     }
// }
