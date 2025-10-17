using System;
using System.Linq;
using DG.Tweening;
using Hp2BaseMod;
using Hp2BaseMod.Ui;
using Hp2BaseModTweaks;
using UnityEngine;
using UnityEngine.UI;

public class UiCellphoneBodySelector : MonoBehaviour
{
    private static readonly Vector2 SIZE = new Vector2(472, 177);
    private const int BUTTON_OFFSET = 160;
    private const int OPEN_DISTANCE = 100;
    private const int CLOSED_DISTANCE = -16;
    private const float SLIDE_DURATION = 0.25f;
    private const int FONT_SIZE = 40;

    public static UiCellphoneBodySelector Create(Font font, AudioKlip buttonPressSound)
    {
        var go = new GameObject();
        var bodySelector = go.AddComponent<UiCellphoneBodySelector>();
        bodySelector.Font = font;
        bodySelector.ButtonPressedSound = buttonPressSound;
        return bodySelector;
    }

    public event Action BodyChanged;

    public Font Font
    {
        get => _font;
        set
        {
            if (_font != value)
            {
                _font = value;
            }
        }
    }
    private Font _font;

    public AudioKlip ButtonPressedSound
    {
        get => _buttonPressedSound;
        set
        {
            if (_buttonPressedSound != value)
            {
                _buttonPressedSound = value;
            }
        }
    }
    private AudioKlip _buttonPressedSound;

    public RectTransform RectTransform => _rectTransform;
    private RectTransform _rectTransform;

    private ExpandedGirlDefinition _girlExpansion;
    private int _bodyIndex;
    private (GirlBodySubDefinition, RelativeId)[] _bodies;

    private Image _bg;
    private Text _label;
    private Hp2ButtonWrapper _buttonLeft;
    private Hp2ButtonWrapper _buttonRight;
    private RectTransform _animationLayer;
    private bool _started = false;

    public void Awake()
    {
        _rectTransform = this.gameObject.AddComponent<RectTransform>();
    }

    public void Start()
    {
        var animationLayer_GO = new GameObject();
        animationLayer_GO.transform.SetParent(this.transform, false);
        _animationLayer = animationLayer_GO.AddComponent<RectTransform>();
        _animationLayer.anchoredPosition = new Vector2(0, CLOSED_DISTANCE);

        var background_GO = new GameObject();
        background_GO.transform.SetParent(_animationLayer, false);
        _bg = background_GO.AddComponent<Image>();
        _bg.rectTransform.sizeDelta = SIZE;
        _bg.sprite = UiPrefabs.WardrobeBodiesPanel;

        var label_GO = new GameObject();
        label_GO.transform.SetParent(_animationLayer, false);
        _label = label_GO.AddComponent<Text>();
        _label.rectTransform.sizeDelta = SIZE;
        _label.color = Color.white;
        _label.fontSize = FONT_SIZE;
        _label.fontStyle = FontStyle.Bold;
        _label.alignment = TextAnchor.MiddleCenter;
        _label.font = _font;

        _buttonLeft = Hp2ButtonWrapper.MakeCellphoneButton("Left",
            ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeft),
            ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowLeftOver),
            _buttonPressedSound);
        _buttonLeft.GameObject.transform.SetParent(_animationLayer, false);
        _buttonLeft.RectTransform.anchoredPosition = new Vector2(-BUTTON_OFFSET, 0);
        _buttonLeft.ButtonBehavior.ButtonPressedEvent += On_ButtonLeft_Pressed;

        _buttonRight = Hp2ButtonWrapper.MakeCellphoneButton("Right",
            ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRight),
            ModInterface.Assets.GetInternalAsset<Sprite>(Common.Ui_AppSettingArrowRightOver),
            _buttonPressedSound);
        _buttonRight.GameObject.transform.SetParent(_animationLayer, false);
        _buttonRight.RectTransform.anchoredPosition = new Vector2(BUTTON_OFFSET, 0);
        _buttonRight.ButtonBehavior.ButtonPressedEvent += On_ButtonRight_Pressed;

        _started = true;
        Refresh();
    }

    public void SetGirl(GirlDefinition girlDef)
    {
        if (girlDef != null)
        {
            _girlExpansion = girlDef.Expansion();

            if (_girlExpansion.Bodies.Count > 1)
            {
                _bodies = _girlExpansion.Bodies.OrderBy(x => x.Key).Select(x => (x.Value, x.Key)).ToArray();
                var currentBody = _girlExpansion.GetCurrentBody();
                _bodyIndex = Array.FindIndex(_bodies, x => x.Item1 == currentBody);

                if (_bodyIndex != -1)
                {
                    Refresh();
                    return;
                }
            }
        }

        _girlExpansion = null;
        _bodies = null;
        _bodyIndex = -1;
        Refresh();
    }

    private void Refresh()
    {
        if (!_started) { return; }
        _rectTransform.sizeDelta = SIZE;

        if (_bodies == null)
        {
            _buttonLeft.ButtonBehavior.Disable();
            _buttonRight.ButtonBehavior.Disable();
            DOTween.Kill(_animationLayer);
            _animationLayer.DOAnchorPosY(CLOSED_DISTANCE, SLIDE_DURATION).SetEase(Ease.InBack).Play();
            return;
        }

        var body = _bodies[_bodyIndex].Item1;

        if (_bodyIndex == 0)
        {
            _buttonLeft.ButtonBehavior.Disable();
        }
        else
        {
            _buttonLeft.ButtonBehavior.Enable();
        }

        if (_bodyIndex == _bodies.Length - 1)
        {
            _buttonRight.ButtonBehavior.Disable();
        }
        else
        {
            _buttonRight.ButtonBehavior.Enable();
        }

        _label.text = body.BodyName;

        DOTween.Kill(_animationLayer);
        _animationLayer.DOAnchorPosY(OPEN_DISTANCE, SLIDE_DURATION).SetEase(Ease.OutBack).Play();
    }

    private void On_ButtonLeft_Pressed(ButtonBehavior buttonBehavior)
    {
        _bodyIndex = (int)Mathf.Repeat(_bodyIndex - 1, _bodies.Length);
        _girlExpansion.SetBody(_bodies[_bodyIndex].Item2);
        Refresh();
        BodyChanged?.Invoke();
    }

    private void On_ButtonRight_Pressed(ButtonBehavior buttonBehavior)
    {
        _bodyIndex = (int)Mathf.Repeat(_bodyIndex + 1, _bodies.Length);
        _girlExpansion.SetBody(_bodies[_bodyIndex].Item2);
        Refresh();
        BodyChanged?.Invoke();
    }

    public void OnDestroy()
    {
        _buttonLeft.Destroy();
        _buttonRight.Destroy();

        UnityEngine.Object.Destroy(_bg.gameObject);
        UnityEngine.Object.Destroy(_label.gameObject);

        _buttonRight = null;
        _buttonLeft = null;
        _label = null;
        _bg = null;

        DOTween.Kill(_animationLayer);
    }
}
