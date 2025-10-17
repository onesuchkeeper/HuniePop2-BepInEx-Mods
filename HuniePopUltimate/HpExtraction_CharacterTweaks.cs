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
                girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "tiffany_cellphoneHead.png")));
                girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "tiffany_cellphoneHeadMini.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_tiffany.png"))).GetSprite());
                body.BackPosition = new VectorInfo(230, 450);
                body.HeadPosition = new VectorInfo(290, 800);
                break;
            case 2://aiko
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_aiko.png")));
                girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "aiko_cellphoneHead.png")));
                girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "aiko_cellphoneHeadMini.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_aiko.png"))).GetSprite());
                body.BackPosition = new VectorInfo(240, 450);
                body.HeadPosition = new VectorInfo(300, 800);
                break;
            case 3://kyanna
                ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
                            new RelativeId(Plugin.ModId, 0),
                            new RelativeId(Plugin.ModId, 2),
                            new RelativeId(Plugin.ModId, 3),
                            new RelativeId(Plugin.ModId, 4),
                        };

                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_kyanna.png")));
                girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "kyanna_cellphoneHead.png")));
                girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "kyanna_cellphoneHeadMini.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_kyanna.png"))).GetSprite());
                body.BackPosition = new VectorInfo(215, 450);
                body.HeadPosition = new VectorInfo(280, 800);
                break;
            case 4://audrey
                ((GirlSpecialPartDataMod)body.specialParts[1]).RequiredHairstyles = new List<RelativeId>(){
                            new RelativeId(Plugin.ModId, 0)
                        };

                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_audrey.png")));
                girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "audrey_cellphoneHead.png")));
                girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "audrey_cellphoneHeadMini.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_audrey.png"))).GetSprite());
                body.BackPosition = new VectorInfo(190, 430);
                body.HeadPosition = new VectorInfo(210, 800);
                break;
            case 5://lola
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(230, 780);
                break;
            case 6://nikki
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_nikki.png")));
                girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "nikki_cellphoneHead.png")));
                girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "nikki_cellphoneHeadMini.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_nikki.png"))).GetSprite());
                body.BackPosition = new VectorInfo(195, 440);
                body.HeadPosition = new VectorInfo(230, 780);
                break;
            case 7://jessie
                ((GirlSpecialPartDataMod)body.specialParts[0]).RequiredHairstyles = new List<RelativeId>(){
                            new RelativeId(Plugin.ModId, 1)
                        };
                var celebrityFrontHair = (GirlPartDataMod)((HairstyleDataMod)body.hairstyles[3]).FrontHairPart;
                celebrityFrontHair.X = celebrityFrontHair.X.Value + 82;
                celebrityFrontHair.Y = celebrityFrontHair.Y.Value - 25;
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(230, 780);
                break;
            case 8: //beli
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_beli.png")));
                girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "beli_cellphoneHead.png")));
                girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "beli_cellphoneHeadMini.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_beli.png"))).GetSprite());
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(245, 800);
                break;
            case 9://kyu
                body.BackPosition = new VectorInfo(225f, 454f);
                body.HeadPosition = new VectorInfo(230, 780);
                girlMod.SpecialCharacter = false;
                body.SpecialEffectName = "FairyWingsKyu";
                break;
            case 10://momo
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_momo.png")));
                girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "momo_cellphoneHead.png")));
                girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "momo_cellphoneHeadMini.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_momo.png"))).GetSprite());
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(240, 780);
                break;
            case 11://celeste
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_celeste.png")));
                girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "celeste_cellphoneHead.png")));
                girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "celeste_cellphoneHeadMini.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_celeste.png"))).GetSprite());
                body.BackPosition = new VectorInfo(200, 450);
                body.HeadPosition = new VectorInfo(220, 820);
                break;
            case 12://venus
                girlMod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "ui_girl_portrait_venus.png")));
                girlMod.CellphoneHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "venus_cellphoneHead.png")));
                girlMod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "venus_cellphoneHeadMini.png")));
                m_SetCharmSprite?.Invoke(girlMod.Id, new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImgDir, "charm_venus.png"))).GetSprite());
                body.BackPosition = new VectorInfo(230, 450);
                body.HeadPosition = new VectorInfo(270, 810);
                break;
        }
    }
}