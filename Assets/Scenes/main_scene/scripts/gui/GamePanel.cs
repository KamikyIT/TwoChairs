using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePanel : MonoBehaviour, IOpenCloseableUiPanel
{
    [SerializeField]
    RectTransform _messagesParent;

    [SerializeField]
    TMPro.TextMeshProUGUI _textMessagePrefab;

    [SerializeField]
    ChairPanel _chairPanel;

    bool _initialized;

    public event Action<Chair> OnOccupateChair;
    public event Action<Chair> OnGrabChair;
    public event Action<Chair> OnLeaveChair;
    public event Action<Chair> OnDropChair;


    #region IOpenCloseableUiPanel

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        this.gameObject.SetActive(true);
    }

    #endregion

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        _chairPanel.Initialize();

        _chairPanel.OnOccupateChair += (x) => OnOccupateChair?.Invoke(x);
        _chairPanel.OnGrabChair += (x) => OnGrabChair?.Invoke(x);
        _chairPanel.OnLeaveChair += (x) => OnLeaveChair?.Invoke(x);
        _chairPanel.OnDropChair += (x) => OnDropChair?.Invoke(x);
    }

    public void DisplayMessage(string message)
    {
        _ = FadeAnimationAndDestroyTextMessageAsync(message);
    }

    public void ShowChairInfo(Chair chair)
    {
        if (chair == null)
            return;

        _chairPanel.gameObject.SetActive(true);

        _chairPanel.ShowChairInfo(chair);
    }

    public void HideChairInfo()
    {
        _chairPanel.gameObject.SetActive(false);
    }

    public void PlayerSittedOnChair(PlayerMovement playerMovement, Chair chair)
    {
        ShowChairInfo(chair);
    }

    public void PlayerGrabbedChair(PlayerMovement playerMovement, Chair chair)
    {
        ShowChairInfo(chair);
    }

    public void ShowCanDropChair(bool canDropChair)
    {
        _chairPanel.gameObject.SetActive(true);

        _chairPanel.ShowCanDropChair(canDropChair);
    }

    async UniTask FadeAnimationAndDestroyTextMessageAsync(string message)
    {
        var textMessage = GameObject.Instantiate(_textMessagePrefab, _messagesParent);

        textMessage.color = new Color(1f, 1f, 1f, 0f);
        textMessage.text = message;

        await UniTask.SwitchToMainThread();

        textMessage.DOFade(1f, 0.5f);

        await UniTask.Delay(3500);

        textMessage.DOFade(0f, 2.5f);

        await UniTask.Delay(2500);

        GameObject.Destroy(textMessage.gameObject);
    }
}
