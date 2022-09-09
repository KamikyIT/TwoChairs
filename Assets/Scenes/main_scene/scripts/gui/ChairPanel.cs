using Photon.Pun;
using System;
using UnityEngine;

public class ChairPanel : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI _chairText;

    [SerializeField]
    ButtonBase _occupateButton;

    [SerializeField]
    ButtonBase _grabButton;

    [SerializeField]
    ButtonBase _leaveButton;

    [SerializeField]
    ButtonBase _dropButton;

    public event Action<Chair> OnOccupateChair;
    public event Action<Chair> OnGrabChair;
    public event Action<Chair> OnLeaveChair;
    public event Action<Chair> OnDropChair;

    bool _initialized;

    Chair _targetChair;

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        _occupateButton.gameObject.SetActive(false);
        _leaveButton.gameObject.SetActive(false);

        _occupateButton.OnClick += () => { OnOccupateChair?.Invoke(_targetChair); };
        _grabButton.OnClick += () => { OnGrabChair?.Invoke(_targetChair); };
        _leaveButton.OnClick += () => { OnLeaveChair?.Invoke(_targetChair); };
        _dropButton.OnClick += () => { OnDropChair?.Invoke(_targetChair); };
    }

    public void ShowChairInfo(Chair chair)
    {
        _targetChair = chair;

        if (chair.OccupantPlayer == null && chair.GrabberPlayer == null)
        {
            _occupateButton.gameObject.SetActive(true);
            _grabButton.gameObject.SetActive(true);
            _leaveButton.gameObject.SetActive(false);
            _dropButton.gameObject.SetActive(false);
            _chairText.text = $"Вы можете занять этот стул.";
        }
        else if (chair.OccupantPlayer == PhotonNetwork.LocalPlayer)
        {
            _occupateButton.gameObject.SetActive(false);
            _grabButton.gameObject.SetActive(false);
            _leaveButton.gameObject.SetActive(true);
            _chairText.text = $"Вы сидите на удобном стуле. ";
        }
        else if (chair.GrabberPlayer == PhotonNetwork.LocalPlayer)
        {
            _occupateButton.gameObject.SetActive(false);
            _grabButton.gameObject.SetActive(false);
            _leaveButton.gameObject.SetActive(false);
            _dropButton.gameObject.SetActive(true);
            _chairText.text = $"Вы несете стул. ";
        }
        else
        {
            _occupateButton.gameObject.SetActive(false);
            _grabButton.gameObject.SetActive(false);
            _leaveButton.gameObject.SetActive(false);
            _dropButton.gameObject.SetActive(false);
            if (chair.OccupantPlayer != null)
                _chairText.text = $"Стул занят игроком {chair.OccupantPlayer.CustomProperties.PlayerName()}.";
            else if (chair.GrabberPlayer != null)
                _chairText.text = $"Стул занят игроком {chair.GrabberPlayer.CustomProperties.PlayerName()}.";
            else
                _chairText.text = string.Empty;
        }
    }

    public void ShowCanDropChair(bool canDropChair)
    {
        _occupateButton.gameObject.SetActive(false);
        _grabButton.gameObject.SetActive(false);
        _leaveButton.gameObject.SetActive(false);

        _dropButton.gameObject.SetActive(canDropChair);

        if (canDropChair)
            _chairText.text = $"Вы можете отпустить стул.";
        else
            _chairText.text = $"Вы не можете отпустить стул.";
    }
}
