using System;
using System.Collections.Generic;
using System.Linq;
using ETAR.AssetStudioPlugin.Extractor;
using Hp2BaseMod;

[Serializable]
public struct DialogSequence
{
    [Serializable]
    public struct Node()
    {
        public UnityAssetPath AssetPath;
        public float Start;
        public float Duration;
    }

    public List<Node> Nodes;
    public int HpGirlId;
    public RelativeId FavoriteId;

    public DialogSequence(int hpGirlId, RelativeId favoriteId, IEnumerable<Node> nodes = null)
    {
        HpGirlId = hpGirlId;
        FavoriteId = favoriteId;
        Nodes = nodes?.ToList() ?? new();
    }
}