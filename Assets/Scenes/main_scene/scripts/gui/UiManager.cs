using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    UiManagerState DEFAULT_STATE = UiManagerState.ConnectionPanel;
    

    [Header("Панели состояний")]
    [SerializeField]
    ConnectionPanel _connectionPanel;
    [SerializeField]
    EditPlayerProfilePanel _editPlayerProfilePanel;
    [SerializeField]
    GamePanel _gamePanel;

    UiManagerState _currentState;
    IOpenCloseableUiPanel _currentPanel;
    List<GameObject> _statePanels;
    UiManagerStateCalculator _uiManagerStateCalculator = new UiManagerStateCalculator();
    bool _initialized;

    UiManagerState CurrentState
    {
        get
        {
            return _currentState;
        }
        set
        {
            _statePanels.ForEach(x => x.gameObject.SetActive(false));

            IOpenCloseableUiPanel targetPanel = null;

            switch (value)
            {
                case UiManagerState.ConnectionPanel:
                    targetPanel = _connectionPanel;
                    break;
                case UiManagerState.EnterPlayerNamePanel:
                    targetPanel = _editPlayerProfilePanel;
                    break;
                case UiManagerState.GamePanel:
                    targetPanel = _gamePanel;
                    break;
                default:
                    Debug.LogError($"Unhandled {value}");
                    return;
            }

            if (_currentPanel == targetPanel)
                return;

            if (_currentPanel != null)
                _currentPanel.Close();

            _currentPanel = targetPanel;

            _currentPanel.Open();

            _currentState = value;
        }
    }

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        _connectionPanel.Initialize();
        _connectionPanel.OnConnectRandom += (connectionResult) =>
        {
            if (!Singleton<PlayerProfile>.Instance.CanJoinGames)
                _uiManagerStateCalculator.CannotJoinRoom();
            else
                _uiManagerStateCalculator.ConnectRandomRoom(connectionResult);

            CurrentState = _uiManagerStateCalculator.NextTargetState;
        };
        _connectionPanel.OnSettings += () =>
        {
            _uiManagerStateCalculator.OnSettingsSelect();

            CurrentState = _uiManagerStateCalculator.NextTargetState;
        };
        _connectionPanel.OnExit += Application.Quit;

        _editPlayerProfilePanel.Initialize();
        _editPlayerProfilePanel.OnApply += () =>
        {
            _uiManagerStateCalculator.AppliedEnterPlayerProfile();
            CurrentState = _uiManagerStateCalculator.NextTargetState;
        };
        _editPlayerProfilePanel.OnBack += () =>
        {
            _uiManagerStateCalculator.BackFromEnterPlayerProfile();
            CurrentState = _uiManagerStateCalculator.NextTargetState;
        };

        _gamePanel.Initialize();
        _gamePanel.OnOccupateChair += (x) => { Singleton<ApplicationCore>.Instance.OccupateChair(x); };
        _gamePanel.OnGrabChair += (x) => { Singleton<ApplicationCore>.Instance.GrabChair(x); };
        _gamePanel.OnLeaveChair += (x) => { Singleton<ApplicationCore>.Instance.LeaveChair(x); };
        _gamePanel.OnDropChair += (x) => { Singleton<ApplicationCore>.Instance.DropChair(x); };


        _statePanels = new List<GameObject>()
        {
            _connectionPanel.gameObject,
        };

        CurrentState = DEFAULT_STATE;
    }

    public void PlayerSittedOnChair(PlayerMovement playerMovement, Chair chair)
    {
        if (CurrentState == UiManagerState.GamePanel)
            _gamePanel.PlayerSittedOnChair(playerMovement, chair);
    }

    public void PlayerGrabbedChair(PlayerMovement playerMovement, Chair chair)
    {
        if (CurrentState == UiManagerState.GamePanel)
            _gamePanel.PlayerGrabbedChair(playerMovement, chair);
    }

    public void HideChairInfo()
    {
        if (CurrentState == UiManagerState.GamePanel)
            _gamePanel.HideChairInfo();
    }

    public void ShowChairInfo(Chair chair)
    {
        if (CurrentState == UiManagerState.GamePanel)
            _gamePanel.ShowChairInfo(chair);
    }

    public void OnPlayerLeftRoom(Player newPlayer)
    {
        if (newPlayer.CustomProperties.TryGetValue(PhotonLauncher.PLAYER_NAME_CUSTOM_PROPERTY, out var playerName))
            DisplaGameMessage($"Игрок {playerName} покинул игру.");
        else
            DisplaGameMessage($"Игрок покинул игру.");
    }

    public void PlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer.CustomProperties.TryGetValue(PhotonLauncher.PLAYER_NAME_CUSTOM_PROPERTY, out var playerName))
            DisplaGameMessage($"Игрок {playerName} вошел в игру.");
        else
            DisplaGameMessage($"Игрок вошел в игру.");
    }

    public void ShowCanDropChair(bool canDropChair)
    {
        if (CurrentState == UiManagerState.GamePanel)
            _gamePanel.ShowCanDropChair(canDropChair);
    }

    void DisplaGameMessage(string message)
    {
        if (CurrentState == UiManagerState.GamePanel)
            _gamePanel.DisplayMessage(message);
    }

    #region Inner Types

    enum UiManagerState
    {
        ConnectionPanel,
        EnterPlayerNamePanel,
        GamePanel,
    }

    class UiManagerStateCalculator
    {
        public bool EnteringRandomRoom;
        public bool EnteringConcreteRoom;


        public UiManagerState NextTargetState { get; private set; }
        public void AppliedEnterPlayerProfile()
        {
            if (EnteringRandomRoom || EnteringConcreteRoom)
                NextTargetState = UiManagerState.GamePanel;
            else
                NextTargetState = UiManagerState.ConnectionPanel;
        }

        public void BackFromEnterPlayerProfile()
        {
            EnteringRandomRoom = false;
            EnteringConcreteRoom = false;

            NextTargetState = UiManagerState.ConnectionPanel;
        }

        public void ConnectRandomRoom(PhotonLauncher.RandomRoomConnectionResult connectionResult)
        {
            if (connectionResult == null)
            {
                NextTargetState = UiManagerState.ConnectionPanel;
                return;
            }

            if (connectionResult.Success)
            {
                EnteringRandomRoom = true;

                if (Singleton<PlayerProfile>.Instance.CanJoinGames)
                    NextTargetState = UiManagerState.GamePanel;
                else
                    NextTargetState = UiManagerState.EnterPlayerNamePanel;
            }
            else
            {
                NextTargetState = UiManagerState.ConnectionPanel;
            }
        }

        public void OnSettingsSelect()
        {
            EnteringRandomRoom = false;
            EnteringConcreteRoom = false;

            NextTargetState = UiManagerState.EnterPlayerNamePanel;
        }

        public void CannotJoinRoom()
        {
            NextTargetState = UiManagerState.EnterPlayerNamePanel;
        }
    }

    #endregion
}
