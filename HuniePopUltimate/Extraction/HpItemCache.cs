using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AssetStudio;
using AssetStudio.Extractor;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public class HpItemCache
{
    private readonly Extractor _extractor;

    public IReadOnlyDictionary<RelativeId, ItemDataMod> Mods => _itemMods;
    private readonly Dictionary<RelativeId, ItemDataMod> _itemMods = new();
    private readonly HpSpriteCache _spriteCache;
    public HpItemCache(Extractor extractor,
        HpSpriteCache spriteCache)
    {
        _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        _spriteCache = spriteCache ?? throw new ArgumentNullException(nameof(spriteCache)); 
    }

    public void Extract(SerializedFile itemFile,
        IEnumerable<(RelativeId, int)> itemIds, 
        (SerializedFile, OrderedDictionary) itemIconSpriteCollection)
    {
        if (_spriteCache.TryGetSpriteLookup(itemIconSpriteCollection, out var itemIconSpriteLookup, out var textureInfoRaw))
        {
            var materializedIds = itemIds.ToList();
            foreach ((RelativeId id, OrderedDictionary def) in
                materializedIds.Select(x => x.Item1)
                    .Zip(_extractor.ExtractMonoBehaviors(itemFile, "ItemDefinition",
                        materializedIds.Select(x => (long)x.Item2)), (a, b) => (a, b)))
            {
                if (_itemMods.ContainsKey(id))
                {
                    ModInterface.Log.Warning($"Skipping Duplicate HpUltimate item extraction {id}");
                    continue;
                }
                
                var mod = ExtractItem(def, id, itemIconSpriteLookup, textureInfoRaw);
                _itemMods[id] = mod;
                ModInterface.AddDataMod(mod);
            }
        } 
    }

    private ItemDataMod ExtractItem(OrderedDictionary def, RelativeId id, Dictionary<string, OrderedDictionary> spriteLookup, TextureInfoRaw textureInfo)
    {
        if (def.TryGetValue("iconName", out string iconName)
            && def.TryGetValue("description", out string description)
            && spriteLookup.TryGetValue(iconName, out var iconSpriteDef)
            && _spriteCache.TryMakeSpriteInfo(iconSpriteDef, textureInfo, out var spriteInfo)
            && def.TryGetValue("name", out string name))
        {
            return new ItemDataMod(id, Hp2BaseMod.Utility.InsertStyle.append, 0)
            {
                ItemSpriteInfo = spriteInfo,
                ItemName = name,
                ItemDescription = description,
            };
        }

        throw new Exception();
    }
}