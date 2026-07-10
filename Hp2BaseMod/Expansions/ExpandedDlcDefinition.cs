namespace Hp2BaseMod;
/**
CLASS: DlcDefinition
--------------------------------------------------------------------------------------------------------------------------------------------
The DlcDefinition class is a ScriptableObject-based data container used by the game engine to define properties for downloadable content 
(DLC). It acts as a structural identifier for specific content packages, allowing the engine to track and verify the presence of 
optional game data.

INHERITANCE HIERARCHY
    UnityEngine.ScriptableObject
        Definition (Adds the 'int id' field) [1]
            DlcDefinition [2]

SERIALIZED FIELDS AND PROPERTIES
    - id (int): Inherited from Definition. Used as a unique key for lookups within the global DlcData dictionary [1, 3].

    - dlcName (string): The internal or display name of the DLC package (e.g., specific expansion names or content patches) [2].

DATA MANAGEMENT AND RETRIEVAL
--------------------------------------------------------------------------------------------------------------------------------------------
DLC definitions are managed globally through the DlcData class, which follows the standard IData<T> pattern used throughout the engine.

    - Master Dictionary: DlcData maintains a Master dictionary mapping integer IDs to DlcDefinition instances [3, 4].
    
    - Resource Initialization: Like other definitions, DlcData is initialized by scanning the game's Resources folder for 
      DlcDefinition assets [4].
      
    - Global Access: Content gating systems can retrieve specific DLC metadata via 'Game.Data.Dlc.Get(id)' [3, 5].

MODDING AND EXTENSIONS (Hp2BaseMod)
--------------------------------------------------------------------------------------------------------------------------------------------
Under the Hp2BaseMod framework, DlcDefinition is integrated into the modular data pipeline, allowing mods to define their own 
content "keys" or modify existing ones.

    - DlcDataMod: Modders can register new DLC definitions or modify existing ones using the DlcDataMod class [6].
    
    - RelativeId Support: Custom DLC definitions are assigned a RelativeId, which provides a unique namespace for the mod to prevent 
      ID collisions with the base game or other plugins [6, 7].

    - GameDataType: The 'Dlc' type is explicitly supported in the GameDataType enum, allowing it to be used with the base mod's 
      generic data retrieval and modification methods [8].

    - Content Verification: Mods can utilize these definitions to create complex dependency checks, ensuring that specific 
      features or character sets only activate if their associated modded DLC "package" is successfully loaded [9].
--------------------------------------------------------------------------------------------------------------------------------------------
*/
[Expansion(typeof(DlcDefinition), HasModId = true)]
public partial class ExpandedDlcDefinition
{
    
}