using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod
{
    /// <summary>
    /// Handles metadata about mods
    /// </summary>
    public class ModData
    {
        /// <summary>
        /// Maps a pair's id to its style info
        /// </summary>
        private readonly static Dictionary<RelativeId, PairStyleInfo> _pairIdToPairStyleInfo = new Dictionary<RelativeId, PairStyleInfo>();

        /// <summary>
        /// Maps a location's id to its style info
        /// </summary>
        private readonly static Dictionary<RelativeId, Dictionary<RelativeId, GirlStyleInfo>> _locationIdToLocationStyleInfo = new Dictionary<RelativeId, Dictionary<RelativeId, GirlStyleInfo>>();

        /// <summary>
        /// Maps a girl's id to its dialogTrigger index
        /// </summary>
        private readonly static Dictionary<RelativeId, int> _girlIdToDialogTriggerIndex = new Dictionary<RelativeId, int>();
        private readonly static Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<RelativeId, int>>> _dtIdToGirlIdToLineIndexLookup = new Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<RelativeId, int>>>();
        private readonly static Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<int, RelativeId>>> _dtIdToGirlIdToLineIdLookup = new Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<int, RelativeId>>>();

        /// <summary>
        /// Maps a girls id to a lookup from a <see cref="RelativeId"/> to an index of the girl's outfits
        /// </summary>
        private readonly Dictionary<RelativeId, Dictionary<RelativeId, int>> _girlIdToOutfitIndexLookup = new Dictionary<RelativeId, Dictionary<RelativeId, int>>();
        private readonly Dictionary<RelativeId, Dictionary<int, RelativeId>> _girlIdToOutfitIdLookup = new Dictionary<RelativeId, Dictionary<int, RelativeId>>();

        /// <summary>
        /// Maps a girls id to a lookup from a <see cref="RelativeId"/> to an index of the girl's hairstyles
        /// </summary>
        private readonly Dictionary<RelativeId, Dictionary<RelativeId, int>> _girlIdToHairstyleIndexLookup = new Dictionary<RelativeId, Dictionary<RelativeId, int>>();
        private readonly Dictionary<RelativeId, Dictionary<int, RelativeId>> _girlIdToHairstyleIdLookup = new Dictionary<RelativeId, Dictionary<int, RelativeId>>();

        /// <summary>
        /// Maps a girls id to a lookup from a <see cref="RelativeId"/> to an index of the girl's parts
        /// </summary>
        private readonly Dictionary<RelativeId, Dictionary<RelativeId, int>> _girlIdToPartIndexLookup = new Dictionary<RelativeId, Dictionary<RelativeId, int>>();
        private readonly Dictionary<RelativeId, Dictionary<int, RelativeId>> _girlIdToPartIdLookup = new Dictionary<RelativeId, Dictionary<int, RelativeId>>();

        /// <summary>
        /// Maps a girls id to a lookup from a <see cref="RelativeId"/> to an index of the girl's expressions
        /// </summary>
        private readonly Dictionary<RelativeId, Dictionary<RelativeId, int>> _girlIdToExpressionIndexLookup = new Dictionary<RelativeId, Dictionary<RelativeId, int>>();
        private readonly Dictionary<RelativeId, Dictionary<int, RelativeId>> _girlIdToExpressionIdLookup = new Dictionary<RelativeId, Dictionary<int, RelativeId>>();

        /// <summary>
        /// Map fruit id and index
        /// </summary>
        private readonly Dictionary<RelativeId, int> _fruitIdToIndex = new Dictionary<RelativeId, int>();
        private readonly Dictionary<int, RelativeId> _fruitIndexToId = new Dictionary<int, RelativeId>();
        private int _fruitIndexSource = 0;

        /// <summary>
        /// Map affection id and index
        /// </summary>
        private readonly Dictionary<RelativeId, int> _affectionIdToIndex = new Dictionary<RelativeId, int>();
        private readonly Dictionary<int, RelativeId> _affectionIndexToId = new Dictionary<int, RelativeId>();
        private int _affectionIndexSource = 0;

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
            { GameDataType.Token, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1}, { RelativeId.Zero, 0 } } }
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
            { GameDataType.Token, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default }, { 0, RelativeId.Zero } } }
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
            { GameDataType.Token, 1000000}
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
            { GameDataType.Token, new HashSet<RelativeId>() }
        };

        #region registration

        internal void RegisterFruit(int index, RelativeId fruitId)
        {
            _fruitIdToIndex[fruitId] = index;
            _fruitIndexToId[index] = fruitId;
            _fruitIndexSource = Math.Max(index, _fruitIndexSource);
        }

        internal void RegisterAffection(int index, RelativeId fruitId)
        {
            _affectionIdToIndex[fruitId] = index;
            _affectionIndexToId[index] = fruitId;
            _affectionIndexSource = Math.Max(index, _affectionIndexSource);
        }

        internal void RegisterLocationStyles(RelativeId locationId, Dictionary<RelativeId, GirlStyleInfo> girlIdToStyleInfo)
        {
            if (_locationIdToLocationStyleInfo.ContainsKey(locationId))
            {
                foreach (var girl in girlIdToStyleInfo)
                {
                    if (_locationIdToLocationStyleInfo[locationId].ContainsKey(girl.Key))
                    {
                        var styleInfo = _locationIdToLocationStyleInfo[locationId][girl.Key];
                        girl.Value.SetData(ref styleInfo);
                    }
                    else
                    {
                        _locationIdToLocationStyleInfo[locationId].Add(girl.Key, girl.Value);
                    }
                }
            }
            else
            {
                _locationIdToLocationStyleInfo.Add(locationId, girlIdToStyleInfo);
            }
        }

        internal void RegisterPairStyle(RelativeId pairId, PairStyleInfo pairStyle)
        {
            if (pairStyle != null)
            {
                if (_pairIdToPairStyleInfo.ContainsKey(pairId))
                {
                    var currentStyle = _pairIdToPairStyleInfo[pairId];
                    pairStyle.SetData(ref currentStyle);
                }
                else
                {
                    _pairIdToPairStyleInfo.Add(pairId, pairStyle);
                }
            }
        }

        internal void RegisterDefaultData(GameDataType type, int localId)
        {
            var id = new RelativeId(-1, localId);
            _relativeIdToRuntimeId[type].Add(id, localId);
            _runtimeIdToRelativeId[type].Add(localId, id);
            _dataIds[type].Add(id);
        }

        internal bool TryRegisterData(GameDataType type, RelativeId id)
        {
            if (!_relativeIdToRuntimeId[type].ContainsKey(id))
            {
                var runtimeId = _runtimeIdSource[type]++;
                _relativeIdToRuntimeId[type].Add(id, runtimeId);
                _runtimeIdToRelativeId[type].Add(runtimeId, id);
                _dataIds[type].Add(id);

                return true;
            }

            return false;
        }

        internal bool TryRegisterGirlDialogTrigger(RelativeId girlId, int index)
        {
            if (!_girlIdToDialogTriggerIndex.ContainsKey(girlId))
            {
                _girlIdToDialogTriggerIndex.Add(girlId, index);
                return true;
            }
            return false;
        }

        internal bool TryRegisterOutfit(RelativeId girlId, int index, RelativeId outfitId)
        {
            if (!_girlIdToOutfitIndexLookup.ContainsKey(girlId))
            {
                _girlIdToOutfitIndexLookup.Add(girlId, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1 } });
                _girlIdToOutfitIdLookup.Add(girlId, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default } });
            }

            if (!_girlIdToOutfitIndexLookup[girlId].ContainsKey(outfitId))
            {
                _girlIdToOutfitIndexLookup[girlId].Add(outfitId, index);
                _girlIdToOutfitIdLookup[girlId].Add(index, outfitId);
                return true;
            }

            return false;
        }

        internal bool TryRegisterHairstyle(RelativeId girlId, int index, RelativeId hairstyleId)
        {
            if (!_girlIdToHairstyleIndexLookup.ContainsKey(girlId))
            {
                _girlIdToHairstyleIndexLookup.Add(girlId, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1 } });
                _girlIdToHairstyleIdLookup.Add(girlId, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default } });
            }

            if (!_girlIdToHairstyleIndexLookup[girlId].ContainsKey(hairstyleId))
            {
                _girlIdToHairstyleIndexLookup[girlId].Add(hairstyleId, index);
                _girlIdToHairstyleIdLookup[girlId].Add(index, hairstyleId);
                return true;
            }

            return false;
        }

        internal bool TryRegisterPart(RelativeId girlId, int index, RelativeId partId)
        {
            if (!_girlIdToPartIndexLookup.ContainsKey(girlId))
            {
                _girlIdToPartIndexLookup.Add(girlId, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1 } });
                _girlIdToPartIdLookup.Add(girlId, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default } });
            }

            if (!_girlIdToPartIndexLookup[girlId].ContainsKey(partId))
            {
                _girlIdToPartIndexLookup[girlId].Add(partId, index);
                _girlIdToPartIdLookup[girlId].Add(index, partId);
                return true;
            }
            return false;
        }

        internal bool TryRegisterExpression(RelativeId girlId, int index, RelativeId expressionId)
        {
            if (!_girlIdToExpressionIndexLookup.ContainsKey(girlId))
            {
                _girlIdToExpressionIndexLookup.Add(girlId, new Dictionary<RelativeId, int>() { { RelativeId.Default, -1 } });
                _girlIdToExpressionIdLookup.Add(girlId, new Dictionary<int, RelativeId>() { { -1, RelativeId.Default } });
            }

            if (!_girlIdToExpressionIndexLookup[girlId].ContainsKey(expressionId))
            {
                _girlIdToExpressionIndexLookup[girlId].Add(expressionId, index);
                _girlIdToExpressionIdLookup[girlId].Add(index, expressionId);
                return true;
            }
            return false;
        }

        internal bool TryRegisterLine(RelativeId dialogTriggerId, RelativeId girlId, int index, RelativeId lineId)
        {
            if (!_dtIdToGirlIdToLineIndexLookup.ContainsKey(dialogTriggerId))
            {
                _dtIdToGirlIdToLineIndexLookup.Add(dialogTriggerId, new Dictionary<RelativeId, Dictionary<RelativeId, int>>());
                _dtIdToGirlIdToLineIdLookup.Add(dialogTriggerId, new Dictionary<RelativeId, Dictionary<int, RelativeId>>());
            }

            if (!_dtIdToGirlIdToLineIndexLookup[dialogTriggerId].ContainsKey(girlId))
            {
                _dtIdToGirlIdToLineIndexLookup[dialogTriggerId].Add(girlId, new Dictionary<RelativeId, int>());
                _dtIdToGirlIdToLineIdLookup[dialogTriggerId].Add(girlId, new Dictionary<int, RelativeId>());
            }

            if (!_dtIdToGirlIdToLineIndexLookup[dialogTriggerId][girlId].ContainsKey(lineId))
            {
                _dtIdToGirlIdToLineIndexLookup[dialogTriggerId][girlId].Add(lineId, index);
                _dtIdToGirlIdToLineIdLookup[dialogTriggerId][girlId].Add(index, lineId);
                //_log.LogLine($"Girl with id {girlId} had line with id {lineId} assigned to index {index} of dialog trigger with id {dialogTriggerId}");
                return true;
            }

            return false;
        }

        #endregion

        #region lookup

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

        public int MaxFruitIndex => _fruitIndexSource;

        public bool TryGetFruitIndex(RelativeId id, out int index) => TryLookupIndex(_fruitIdToIndex, id, out index);
        public bool TryGetFruitId(int index, out RelativeId id) => TryLookupId(_fruitIndexToId, index, out id);

        public int MaxAffectionIndex => _affectionIndexSource;

        public bool TryGetAffectionIndex(RelativeId id, out int index) => TryLookupIndex(_affectionIdToIndex, id, out index);
        public bool TryGetAffectionId(int index, out RelativeId id) => TryLookupId(_affectionIndexToId, index, out id);

        public IEnumerable<RelativeId> GetIds(GameDataType type) => _dataIds[type];

        public int GetLineIndex(RelativeId dialogTriggerId, RelativeId girlId, RelativeId lineId) => _dtIdToGirlIdToLineIndexLookup[dialogTriggerId][girlId][lineId];
        public RelativeId GetLineId(RelativeId dialogTriggerId, RelativeId girlId, int lineIndex) => _dtIdToGirlIdToLineIdLookup[dialogTriggerId][girlId][lineIndex];

        public GirlStyleInfo GetLocationStyleInfo(RelativeId locationId, RelativeId girlId) => _locationIdToLocationStyleInfo[locationId][girlId];
        public bool TryGetLocationStyleInfo(RelativeId locationId, RelativeId girlId, out GirlStyleInfo girlStyleInfo)
        {
            if (_locationIdToLocationStyleInfo.TryGetValue(locationId, out var girlToStyle)
                && girlToStyle.TryGetValue(girlId, out girlStyleInfo))
            {
                return true;
            }

            girlStyleInfo = null;
            return false;
        }

        public PairStyleInfo GetPairStyleInfo(RelativeId pairId) => _pairIdToPairStyleInfo[pairId];
        public PairStyleInfo GetPairStyleInfo(int pairRuntimeId) => _pairIdToPairStyleInfo[_runtimeIdToRelativeId[GameDataType.GirlPair][pairRuntimeId]];

        public int GetGirlDialogTriggerIndex(RelativeId girlId) => _girlIdToDialogTriggerIndex[girlId];

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

        public int? GetOutfitIndex(RelativeId? girlId, RelativeId? id) => girlId.HasValue ? GetOutfitIndex(girlId.Value, id) : null;
        public int? GetOutfitIndex(RelativeId girlId, RelativeId? id) => id.HasValue ? (int?)GetOutfitIndex(girlId, id.Value) : null;
        public int GetOutfitIndex(RelativeId girlId, RelativeId id) => _girlIdToOutfitIndexLookup[girlId][id];
        public bool TryGetOutfitIndex(RelativeId girlId, RelativeId id, out int index)
        {
            if (_girlIdToOutfitIndexLookup.TryGetValue(girlId, out var idLookup)
                && idLookup.TryGetValue(id, out index))
            {
                return true;
            }

            index = -1;
            return false;
        }

        public IEnumerable<RelativeId> GetAllOutfitIds(RelativeId girlId) => _girlIdToOutfitIdLookup[girlId].Values;
        public RelativeId GetOutfitId(RelativeId girlId, int index) => _girlIdToOutfitIdLookup[girlId][index];
        public bool TryGetOutfitId(RelativeId girlId, int index, out RelativeId id)
        {
            if (_girlIdToOutfitIdLookup.TryGetValue(girlId, out var outfitIdLookup)
                && outfitIdLookup.TryGetValue(index, out id))
            {
                return true;
            }

            id = RelativeId.Default;
            return false;
        }

        public int? GetHairstyleIndex(RelativeId? girlId, RelativeId? id) => girlId.HasValue ? GetHairstyleIndex(girlId.Value, id) : null;
        public int? GetHairstyleIndex(RelativeId girlId, RelativeId? id) => id.HasValue ? (int?)GetHairstyleIndex(girlId, id.Value) : null;
        public int GetHairstyleIndex(RelativeId girlId, RelativeId id) => _girlIdToHairstyleIndexLookup[girlId][id];

        public IEnumerable<RelativeId> GetAllHairstyleIds(RelativeId girlId) => _girlIdToHairstyleIdLookup[girlId].Values;
        public RelativeId GetHairstyleId(RelativeId girlId, int index) => _girlIdToHairstyleIdLookup[girlId][index];
        public bool TryGetHairstyleId(RelativeId girlId, int index, out RelativeId id)
        {
            if (_girlIdToHairstyleIdLookup.TryGetValue(girlId, out var hairstyleIdLookup)
                && hairstyleIdLookup.TryGetValue(index, out id))
            {
                return true;
            }

            id = RelativeId.Default;
            return false;
        }

        public bool TryGetHairstyleIndex(RelativeId girlId, RelativeId id, out int index)
        {
            if (_girlIdToHairstyleIndexLookup.TryGetValue(girlId, out var idLookup)
                && idLookup.TryGetValue(id, out index))
            {
                return true;
            }

            index = -1;
            return false;
        }

        public int? GetPartIndex(RelativeId girlId, RelativeId? id) => id.HasValue ? (int?)GetPartIndex(girlId, id.Value) : null;
        public int GetPartIndex(RelativeId girlId, RelativeId id) => _girlIdToPartIndexLookup[girlId][id];
        public RelativeId GetPartId(RelativeId girlId, int index) => _girlIdToPartIdLookup[girlId][index];

        public int? GetExpressionIndex(RelativeId girlId, RelativeId? id) => id.HasValue ? (int?)GetExpressionIndex(girlId, id.Value) : null;
        public int GetExpressionIndex(RelativeId girlId, RelativeId id) => _girlIdToExpressionIndexLookup[girlId][id];
        public RelativeId GetExpressionId(RelativeId girlId, int index) => _girlIdToExpressionIdLookup[girlId][index];

        #endregion

        #region Cheatsy

        public void SwapGirlStyles(RelativeId girlA, RelativeId girlB)
        {
            //_girlIdToDialogTriggerIndex;
            //_girlIdToExpressionIdLookup;
            //_girlIdToExpressionIndexLookup;

            var hold = _girlIdToHairstyleIdLookup[girlA];
            _girlIdToHairstyleIdLookup[girlA] = _girlIdToHairstyleIdLookup[girlB];
            _girlIdToHairstyleIdLookup[girlB] = hold;

            var hold2 = _girlIdToHairstyleIndexLookup[girlA];
            _girlIdToHairstyleIndexLookup[girlA] = _girlIdToHairstyleIndexLookup[girlB];
            _girlIdToHairstyleIndexLookup[girlB] = hold2;

            hold = _girlIdToOutfitIdLookup[girlA];
            _girlIdToOutfitIdLookup[girlA] = _girlIdToOutfitIdLookup[girlB];
            _girlIdToOutfitIdLookup[girlB] = hold;

            hold2 = _girlIdToOutfitIndexLookup[girlA];
            _girlIdToOutfitIndexLookup[girlA] = _girlIdToOutfitIndexLookup[girlB];
            _girlIdToOutfitIndexLookup[girlB] = hold2;

            foreach (var locStyleInfo in _locationIdToLocationStyleInfo.Values)
            {
                if (!locStyleInfo.TryGetValue(girlA, out var a))
                {
                    a = new GirlStyleInfo();
                }

                if (!locStyleInfo.TryGetValue(girlB, out var b))
                {
                    b = new GirlStyleInfo();
                }

                locStyleInfo[girlA] = b;
                locStyleInfo[girlB] = a;
            }

            // lets not do pairs, the pairs are randomized anyways
            // foreach (var pairStyleInfo in _pairIdToPairStyleInfo.Values)
            // {

            // }

            //_girlIdToPartIdLookup;
            //_girlIdToPartIndexLookup;
            //_dtIdToGirlIdToLineIdLookup;
            //_dtIdToGirlIdToLineIndexLookup;
        }

        #endregion
    }
}
