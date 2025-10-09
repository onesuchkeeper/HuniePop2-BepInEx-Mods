using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseMod
{
    /// <summary>
    /// Handles metadata about mods
    /// </summary>
    public class ModData
    {
        private readonly Dictionary<RelativeId, List<IFunctionalAilmentDataMod>> _ailmentIdToFunctionalMods = new Dictionary<RelativeId, List<IFunctionalAilmentDataMod>>();

        /// <summary>
        /// Maps a source's local ids to the ids used at runtime.
        /// </summary>
        private readonly Dictionary<GameDataType, Dictionary<RelativeId, int>> _relativeIdToRuntimeId = new Dictionary<GameDataType, Dictionary<RelativeId, int>>()
        {
            { GameDataType.Ability, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Ailment, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Code, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Cutscene, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.DialogTrigger, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Dlc, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Energy, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Girl, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.GirlPair, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Item, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Location, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Photo, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Question, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Token, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } },
            { GameDataType.Fruit, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1} } },
            { GameDataType.Affection, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1} } },
            { GameDataType.SpecialPart, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1} } }
        };

        private readonly Dictionary<GameDataType, Dictionary<int, RelativeId>> _runtimeIdToRelativeId = new Dictionary<GameDataType, Dictionary<int, RelativeId>>()
        {
            { GameDataType.Ability, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Ailment, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Code, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Cutscene, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.DialogTrigger, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Dlc, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Energy, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Girl, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.GirlPair, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Item, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Location, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Photo, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Question, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Token, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } },
            { GameDataType.Fruit, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default } } },
            { GameDataType.Affection, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default } } },
            { GameDataType.SpecialPart, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default } } },
        };

        /// <summary>
        /// Keeps track of the next id to assign to new data defs of each type.
        /// Starts at 1,000,000 just for security in not conflicting with the base game ever. Hopefully.
        /// Overkill but cheap.
        /// </summary>
        private readonly Dictionary<GameDataType, int> _runtimeIdSource = new Dictionary<GameDataType, int>()
        {
            { GameDataType.Ability, 1000000},
            { GameDataType.Ailment, 1000000},
            { GameDataType.Code, 1000000},
            { GameDataType.Cutscene, 1000000},
            { GameDataType.DialogTrigger, 1000000},
            { GameDataType.Dlc, 1000000},
            { GameDataType.Energy, 1000000},
            { GameDataType.Girl, 1000000},
            { GameDataType.GirlPair, 1000000},
            { GameDataType.Item, 1000000},
            { GameDataType.Location, 1000000},
            { GameDataType.Photo, 1000000},
            { GameDataType.Question, 1000000},
            { GameDataType.Token, 1000000},
            { GameDataType.Fruit, 1000000},
            { GameDataType.Affection, 1000000},
            { GameDataType.SpecialPart, 1000000},
        };

        /// <summary>
        /// Maps the type of a definition to its corresponding <see cref="GameDataType"/>
        /// </summary>
        private readonly Dictionary<Type, GameDataType> _defTypeToGameDataType = new Dictionary<Type, GameDataType>()
        {
            {typeof(AbilityDefinition), GameDataType.Ability},
            {typeof(AilmentDefinition),GameDataType.Ailment},
            {typeof(CodeDefinition),GameDataType.Code},
            {typeof(CutsceneDefinition),GameDataType.Cutscene},
            {typeof(DialogTriggerDefinition),GameDataType.DialogTrigger},
            {typeof(DlcDefinition),GameDataType.Dlc},
            {typeof(EnergyDefinition),GameDataType.Energy},
            {typeof(GirlDefinition),GameDataType.Girl},
            {typeof(GirlPairDefinition),GameDataType.GirlPair},
            {typeof(ItemDefinition),GameDataType.Item},
            {typeof(LocationDefinition), GameDataType.Location},
            {typeof(PhotoDefinition), GameDataType.Photo},
            {typeof(QuestionDefinition), GameDataType.Question},
            {typeof(TokenDefinition), GameDataType.Token}
        };

        /// <summary>
        /// Keeps track of the ids in use for each <see cref="GameDataType"/>
        /// </summary>
        private readonly Dictionary<GameDataType, HashSet<RelativeId>> _dataIds = new Dictionary<GameDataType, HashSet<RelativeId>>()
        {
            { GameDataType.Ability, new HashSet<RelativeId>() },
            { GameDataType.Ailment, new HashSet<RelativeId>() },
            { GameDataType.Code, new HashSet<RelativeId>() },
            { GameDataType.Cutscene, new HashSet<RelativeId>() },
            { GameDataType.DialogTrigger, new HashSet<RelativeId>() },
            { GameDataType.Dlc, new HashSet<RelativeId>() },
            { GameDataType.Energy, new HashSet<RelativeId>() },
            { GameDataType.Girl, new HashSet<RelativeId>() },
            { GameDataType.GirlPair, new HashSet<RelativeId>() },
            { GameDataType.Item, new HashSet<RelativeId>() },
            { GameDataType.Location, new HashSet<RelativeId>() },
            { GameDataType.Photo, new HashSet<RelativeId>() },
            { GameDataType.Question, new HashSet<RelativeId>() },
            { GameDataType.Token, new HashSet<RelativeId>() },
            { GameDataType.Fruit, new HashSet<RelativeId>() },
            { GameDataType.Affection, new HashSet<RelativeId>() },
            { GameDataType.SpecialPart, new HashSet<RelativeId>() },
        };

        #region registration

        internal void RegisterDefaultData(GameDataType type, int localId)
        {
            var id = new RelativeId(-1, localId);
            _relativeIdToRuntimeId[type].Add(id, localId);
            _runtimeIdToRelativeId[type].Add(localId, id);
            _dataIds[type].Add(id);
        }

        /// <summary>
        /// Attempts to register an id for a GameDataType. Use <see cref="ModInterface.AddDataMod"/> instead
        /// if adding a data mod, which will automatically handle its id.
        /// </summary>
        public bool TryRegisterDataId(GameDataType type, RelativeId id)
        {
            if (id.SourceId != -1 && !_relativeIdToRuntimeId[type].ContainsKey(id))
            {
                var runtimeId = _runtimeIdSource[type]++;
                _relativeIdToRuntimeId[type].Add(id, runtimeId);
                _runtimeIdToRelativeId[type].Add(runtimeId, id);
                _dataIds[type].Add(id);

                return true;
            }

            return false;
        }

        internal void RegisterFunctionalAilments(IEnumerable<IFunctionalAilmentDataMod> mods)
        {
            foreach (var mod in mods)
            {
                _ailmentIdToFunctionalMods[mod.Id].Add(mod);
            }
        }

        #endregion

        #region lookup

        internal bool TryGetFunctionalAilment(RelativeId ailmentId, out IEnumerable<IFunctionalAilmentDataMod> mods)
        {
            if (_ailmentIdToFunctionalMods.TryGetValue(ailmentId, out var modsList))
            {
                mods = modsList;
                return true;
            }

            mods = null;
            return false;
        }

        private bool TryLookupIndex(Dictionary<RelativeId, int> dict, RelativeId? id, out int index)
        {
            if (id.HasValue && dict.TryGetValue(id.Value, out index))
            {
                return true;
            }

            index = -1;
            return false;
        }

        private bool TryLookupId(Dictionary<int, RelativeId> dict, int? index, out RelativeId id)
        {
            if (index.HasValue && dict.TryGetValue(index.Value, out id))
            {
                return true;
            }

            id = RelativeId.Default;
            return false;
        }

        public IEnumerable<RelativeId> GetIds(GameDataType type) => _dataIds[type];

        public int GetRuntimeDataId(GameDataType dataModType, RelativeId id) => _relativeIdToRuntimeId[dataModType][id];

        public int? GetRuntimeDataId(GameDataType dataModType, RelativeId? id) => id.HasValue
            ? (int?)GetRuntimeDataId(dataModType, id.Value)
            : null;

        public bool TryGetRuntimeDataId(GameDataType dataModType, RelativeId id, out int result) => _relativeIdToRuntimeId[dataModType].TryGetValue(id, out result);

        public bool TryGetRuntimeDataId(GameDataType dataModType, RelativeId? id, out int result)
        {
            if (id.HasValue)
            {
                return TryGetRuntimeDataId(dataModType, id.Value, out result);
            }

            result = -1;
            return false;
        }

        public RelativeId GetDataId(Definition definition)
        {
            if (!_defTypeToGameDataType.TryGetValue(definition.GetType(), out var gameDataType))
            {
                throw new ArgumentException("Unsupported definition type");
            }

            return GetDataId(gameDataType, definition.id);
        }

        public RelativeId GetDataId(GameDataType dataModType, int runtimeId) => _runtimeIdToRelativeId[dataModType][runtimeId];

        public bool TryGetDataId(GameDataType dataModType, int runtimeId, out RelativeId id)
        {
            if (_runtimeIdToRelativeId[dataModType].TryGetValue(runtimeId, out id))
            {
                return true;
            }

            id = RelativeId.Default;
            return false;
        }

        public bool IsRegistered(GameDataType type, RelativeId id) => _relativeIdToRuntimeId[type].ContainsKey(id);

        #endregion
    }
}
