using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    public void Tweak(GirlDataMod girlMod, int nativeId)
    {
        var body = (GirlBodyDataMod)girlMod.bodies[0];

        switch (nativeId)
        {
            case 1://tiffany
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_tiffany.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_tiffany.png"))).GetSprite());
                break;
            case 2://aiko
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_aiko.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_aiko.png"))).GetSprite());
                break;
            case 3://kyanna
                ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
                            new RelativeId(Plugin.ModId, 0),
                            new RelativeId(Plugin.ModId, 2),
                            new RelativeId(Plugin.ModId, 3),
                            new RelativeId(Plugin.ModId, 4),
                        };

                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_kyanna.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_kyanna.png"))).GetSprite());
                break;
            case 4://audrey
                ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
                            new RelativeId(Plugin.ModId, 0)
                        };

                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_audrey.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_audrey.png"))).GetSprite());
                break;
            case 5://lola
                break;
            case 6://nikki
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_nikki.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_nikki.png"))).GetSprite());
                break;
            case 7://jessie
                ((GirlSpecialPartDataMod)body.specialParts[0]).RequiredHairstyles = new List<RelativeId>(){
                            new RelativeId(Plugin.ModId, 1)
                        };
                var celebrityFrontHair = (GirlPartDataMod)((HairstyleDataMod)body.hairstyles[3]).FrontHairPart;
                celebrityFrontHair.X = celebrityFrontHair.X.Value + 82;
                celebrityFrontHair.Y = celebrityFrontHair.Y.Value - 25;
                break;
            case 8: //beli
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_beli.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_beli.png"))).GetSprite());
                break;
            case 9://kyu
                body.BackPosition = new VectorInfo(225f, 454f);
                girlMod.SpecialCharacter = false;
                break;
            case 10://momo
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_momo.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_momo.png"))).GetSprite());
                break;
            case 11://celeste
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_celeste.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_celeste.png"))).GetSprite());
                break;
            case 12://venus
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_venus.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_venus.png"))).GetSprite());
                break;
        }
    }
}