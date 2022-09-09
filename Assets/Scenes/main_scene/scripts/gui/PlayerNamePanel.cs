using System;
using UnityEngine;

public class PlayerNamePanel : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshPro _playerNameText;

    Camera _mainPlayerCamera;

    public void DisplayPlayerName(string playerName, Camera mainPlayerCamera)
    {
        _playerNameText.text = playerName;
        _mainPlayerCamera = mainPlayerCamera;
    }

    void LateUpdate()
    {
        if (_mainPlayerCamera != null)
            this.transform.LookAt(_mainPlayerCamera.transform);
    }
}