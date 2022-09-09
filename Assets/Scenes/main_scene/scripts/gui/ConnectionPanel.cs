using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionPanel : MonoBehaviour, IOpenCloseableUiPanel
{
    [SerializeField]
    ButtonBase _connectRandomButton;

    [SerializeField]
    ButtonBase _settingsButton;

    [SerializeField]
    ButtonBase _exitButton;

    bool _initialized;

    public Action<PhotonLauncher.RandomRoomConnectionResult> OnConnectRandom;
    public event Action OnSettings;
    public event Action OnExit;

    
    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        _connectRandomButton.OnClick += async () =>
        {
            if (!Singleton<PlayerProfile>.Instance.CanJoinGames)
            {
                OnConnectRandom?.Invoke(new PhotonLauncher.RandomRoomConnectionResult(false) { CanJoinRoom = false });
                return;
            }

            _connectRandomButton.Interactable = false;
            var result = await Singleton<ApplicationCore>.Instance.ConnectRandomRoomAsync();

            OnConnectRandom?.Invoke(result);
            _connectRandomButton.Interactable = true;
        };

        _settingsButton.OnClick += () =>
        {
            OnSettings?.Invoke();
        };

        _exitButton.OnClick += () => OnExit?.Invoke();
    }


    #region IOpenCloseableUiPanel

    public void Open()
    {
        this.gameObject.SetActive(true);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    #endregion
}
