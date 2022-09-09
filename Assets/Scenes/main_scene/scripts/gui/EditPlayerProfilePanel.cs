using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEngine.UI;

public class EditPlayerProfilePanel : MonoBehaviour, IOpenCloseableUiPanel
{
    [SerializeField]
    TMPro.TMP_InputField _playerNameInput;
    [SerializeField]
    ButtonBase _applyButton;
    [SerializeField]
    ButtonBase _backButton;

    [SerializeField]
    RectTransform _skinColorsParentPanel;
    [SerializeField]
    ButtonPlayerColorSkin _colorButtonPrefab;

    [SerializeField]
    RectTransform _selectedColorSkinFrame;

    Dictionary<PlayerProfile.PlayerSkinColors, ButtonPlayerColorSkin> _colorButtonsDict;
    PlayerProfile.PlayerSkinColors _playerSkinColor;
    bool _initialized;

    string PlayerName { get; set; }
    PlayerProfile.PlayerSkinColors PlayerSkinColor
    {
        get { return _playerSkinColor; }
        set
        {
            _playerSkinColor = value;
            if (_colorButtonsDict.TryGetValue(_playerSkinColor, out var targetButton))
            {
                _selectedColorSkinFrame.transform.SetParent(targetButton.transform);
                _selectedColorSkinFrame.anchorMin = Vector2.zero;
                _selectedColorSkinFrame.anchorMax = Vector2.one;
                _selectedColorSkinFrame.offsetMin = Vector2.zero;
                _selectedColorSkinFrame.offsetMax = Vector2.zero;
            }
            else
                Debug.LogError($"{GetType()} : Не найдена кнопка для цвета {_playerSkinColor}");
        }
    }

    public event Action OnApply;
    public event Action OnBack;

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        _playerNameInput.onValueChanged.AddListener(PlayerNameChanged);
        _playerNameInput.text = PlayerName;

        _colorButtonsDict = new Dictionary<PlayerProfile.PlayerSkinColors, ButtonPlayerColorSkin>();
        var allSkinColors = Enum.GetValues(typeof(PlayerProfile.PlayerSkinColors));
        
        foreach (PlayerProfile.PlayerSkinColors color in allSkinColors)
        {
            var buttonGameObject = GameObject.Instantiate(_colorButtonPrefab, _skinColorsParentPanel);
            buttonGameObject.Data = color;
            buttonGameObject.DisplayColor();
            buttonGameObject.OnClick += (x) => { PlayerSkinColor = x; };
            _colorButtonsDict.Add(color, buttonGameObject);
        }

        _applyButton.OnClick += ApplyChangesClick;
        _backButton.OnClick += BackClick;
    }

    void PlayerNameChanged(string text)
    {
        PlayerName = text;

        _applyButton.Interactable = !string.IsNullOrEmpty(text);
    }

    void ApplyChangesClick()
    {
        Singleton<ApplicationCore>.Instance.SetPlayerProfile(playerName: PlayerName, playerSkinColor: PlayerSkinColor);

        OnApply?.Invoke();
    }

    void BackClick()
    {
        OnBack?.Invoke();
    }

    #region IOpenCloseableUiPanel

    public void Open()
    {
        this.gameObject.SetActive(true);

        PlayerName = Singleton<PlayerProfile>.Instance.PlayerName;

        _playerNameInput.text = PlayerName;

        PlayerSkinColor = Singleton<PlayerProfile>.Instance.PlayerSkinColor;
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    #endregion
}
