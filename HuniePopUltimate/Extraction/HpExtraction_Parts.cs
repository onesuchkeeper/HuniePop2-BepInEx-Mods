using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    private List<RelativeId> StyleSequence = new()
    {
        Hp2BaseMod.Styles.Activity,
        Hp2BaseMod.Styles.Relaxing,
        Hp2BaseMod.Styles.Romantic,
        Hp2BaseMod.Styles.Party,
        Hp2BaseMod.Styles.Water,
        Hp2BaseMod.Styles.Sexy,
        Styles.Topless,
        Hp2BaseMod.Styles.Bonus1,
        Hp2BaseMod.Styles.Bonus2,
        Hp2BaseMod.Styles.Bonus3,
        Hp2BaseMod.Styles.Bonus4,
    };

    private string GetUnderwearName(int nativeId)
    {
        switch (nativeId)
        {
            case 2:
                return "Naughty Teacher";
            case 3:
                return "Pure Sin";
            case 6:
                return "Unprepared";
            case 8:
                return "Underwear";
            case 9:
                return "Pink, Bitch";
            case 10:
                return "Satin Strands";
            case 11:
                return "Undergarments";
            case 12:
                return "Divine Blessing";
        }

        return "Underwear";
    }

    private GirlPartDataMod MergeParts(string cachePath, (GirlPartDataMod, SpriteInfoTexture)[] parts)
    {
        if (parts.Length == 0)
        {
            return null;
        }

        if (parts.Length == 1)
        {
            return parts[0].Item1;
        }

        var bottomLeft = new Vector2Int(parts[0].Item1.X.Value,
            Mathf.FloorToInt(parts[0].Item1.Y.Value - parts[0].Item2._rect.Value.height));

        var topRight = new Vector2Int(Mathf.CeilToInt(bottomLeft.x + parts[0].Item2._rect.Value.width),
            Mathf.CeilToInt(parts[0].Item1.Y.Value));

        foreach (var (part, sprite) in parts.Skip(1))
        {
            bottomLeft.x = Mathf.Min(bottomLeft.x, part.X.Value);
            bottomLeft.y = Mathf.FloorToInt(Mathf.Min(bottomLeft.y, part.Y.Value - sprite._rect.Value.height));

            topRight.x = Mathf.CeilToInt(Mathf.Max(topRight.x, part.X.Value + sprite._rect.Value.width));
            topRight.y = Mathf.CeilToInt(Mathf.Max(topRight.y, part.Y.Value));
        }

        var merged = new GirlPartDataMod(new RelativeId(Plugin.ModId, _partCount++), InsertStyle.replace);

        merged.SpriteInfo = new SpriteInfoTexture(new TextureInfoCache(cachePath,
            new TextureInfoRender(
                new TextureInfoComposite(topRight - bottomLeft, false,
                    parts.Select(x => ((ITextureInfo)new TextureInfoSprite(x.Item2, false), new Vector2Int(x.Item1.X.Value - 1, Mathf.FloorToInt(x.Item1.Y.Value - x.Item2._rect.Value.height) - 1) - bottomLeft))),
                false,
                [new TextureRsPad(1)])));

        merged.X = bottomLeft.x;
        merged.Y = topRight.y;

        return merged;
    }

    private bool TryMakePartDataMod(GirlPartType partType,
        OrderedDictionary girlPieceArt,
        Dictionary<string, OrderedDictionary> spriteLookup,
        TextureInfoRaw spriteTextureInfo,
        out GirlPartDataMod part,
        out SpriteInfoTexture spriteInfo)
    {
        using (ModInterface.Log.MakeIndent())
        {
            if (girlPieceArt.TryGetValue("x", out int x)
                && girlPieceArt.TryGetValue("y", out int y)
                && girlPieceArt.TryGetValue("spriteName", out string spriteName))
            {
                if (x == 0 && y == 0)
                {
                    part = null;
                    spriteInfo = null;
                    return false;
                }

                if (spriteLookup.TryGetValue(spriteName, out var spriteDef)
                    && TryMakeSpriteInfo(spriteDef, spriteTextureInfo, false, out spriteInfo))
                {
                    part = new(new RelativeId(Plugin.ModId, _partCount++), InsertStyle.append);
                    part.X = x - 400;
                    part.Y = -y + 824;
                    part.SpriteInfo = spriteInfo;
                    part.PartType = partType;

                    return true;
                }
                else
                {
                    ModInterface.Log.Message($"Failed get sprite");
                }
            }
            else
            {
                ModInterface.Log.Message($"Failed to read definition");
            }
        }

        spriteInfo = null;
        part = null;
        return false;
    }

    private int _expressionCount = 0;

    private bool TryExtractExpression(OrderedDictionary piece,
            Dictionary<string, OrderedDictionary> spriteLookup,
            TextureInfoRaw spriteTextureInfo,
            out GirlExpressionDataMod expression,
            out GirlPartDataMod brows,
            out GirlPartDataMod eyes,
            out GirlPartDataMod mouth,
            out GirlPartDataMod face)
    {
        brows = null;
        eyes = null;
        mouth = null;
        face = null;
        expression = new GirlExpressionDataMod(new RelativeId(Plugin.ModId, _expressionCount++), InsertStyle.append);

        if (piece.TryGetValue("expressionType", out int expressionType)
            && expressionType == 3)//excited has eyes closed
        {
            expression.EyesClosed = true;
        }

        if (piece.TryGetValue("art", out List<object> artCollection))
        {
            var it = artCollection.OfType<OrderedDictionary>().GetEnumerator();
            if (!it.MoveNext()) { return false; }
            var eyeArt = it.Current;
            if (!it.MoveNext()) { return false; }
            var browArt = it.Current;
            if (!it.MoveNext()) { return false; }
            var mouthArt = it.Current;
            if (!it.MoveNext()) { return false; }
            var faceArt = it.Current;

            if (!TryMakePartDataMod(GirlPartType.EYEBROWS, browArt, spriteLookup, spriteTextureInfo, out brows, out _))
            {
                return false;
            }
            expression.PartEyebrows = brows;

            if (!TryMakePartDataMod(GirlPartType.EYES, eyeArt, spriteLookup, spriteTextureInfo, out eyes, out _))
            {
                return false;
            }
            expression.PartEyes = eyes;

            if (!TryMakePartDataMod(GirlPartType.MOUTH, mouthArt, spriteLookup, spriteTextureInfo, out mouth, out _))
            {
                return false;
            }
            expression.PartMouthClosed = mouth;

            if (!TryMakePartDataMod(GirlPartType.BLUSHLIGHT, faceArt, spriteLookup, spriteTextureInfo, out face, out _))
            {
                return false;
            }

            return true;
        }

        return false;
    }
}