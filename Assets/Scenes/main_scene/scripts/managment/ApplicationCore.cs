using Cysharp.Threading.Tasks;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationCore : MonoBehaviour
{
    #region UNITY
    void Start()
    {
        Singleton<UiManager>.Instance.Initialize();

        Singleton<PhotonLauncher>.Instance.Initialize();

        Singleton<PlayerProfile>.Instance.Initialize();

        Singleton<PlayersObjectsManager>.Instance.Initialize();
   }
    #endregion

    public UniTask<PhotonLauncher.RandomRoomConnectionResult> ConnectRandomRoomAsync()
    {
        return Singleton<PhotonLauncher>.Instance.ConnectRandomRoomAsync();
    }

    public void SetPlayerProfile(string playerName, PlayerProfile.PlayerSkinColors playerSkinColor)
    {
        Singleton<PlayerProfile>.Instance.SetPlayerProfile(playerName, playerSkinColor);
        Singleton<PhotonLauncher>.Instance.SetPlayerProfile(playerName, playerSkinColor);
    }

    public void PlayerEnteredRoom(Player newPlayer)
    {
        Singleton<UiManager>.Instance.PlayerEnteredRoom(newPlayer);
    }

    public void OnPlayerLeftRoom(Player newPlayer)
    {
        Singleton<UiManager>.Instance.OnPlayerLeftRoom(newPlayer);

        Singleton<PlayersObjectsManager>.Instance.PlayerLeftRoom(newPlayer);
    }

    public void ThisPlayerEnteredRoom()
    {
        Singleton<PlayersObjectsManager>.Instance.ThisPlayerEnteredRoom();
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Singleton<PlayersObjectsManager>.Instance.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
    }

    public void PlayerSittedOnChair(PlayerMovement playerMovement, Chair chair)
    {
        Singleton<UiManager>.Instance.PlayerSittedOnChair(playerMovement, chair);
    }

    public void ShowChairInfo(Chair chair)
    {
        Singleton<UiManager>.Instance.ShowChairInfo(chair);
    }

    public void HideChairInfo()
    {
        Singleton<UiManager>.Instance.HideChairInfo();
    }

    public void OccupateChair(Chair chair)
    {
        Singleton<PhotonLauncher>.Instance.OccupateChair(chair);
    }

    public void PlayerGrabbedChair(PlayerMovement playerMovement, Chair chair)
    {
        Singleton<UiManager>.Instance.PlayerGrabbedChair(playerMovement, chair);
    }

    public void GrabChair(Chair chair)
    {
        Singleton<PhotonLauncher>.Instance.GrabChair(chair);
    }

    public void LeaveChair(Chair chair)
    {
        Singleton<PhotonLauncher>.Instance.LeaveChair(chair);
    }

    public void DropChair(Chair chair)
    {
        Singleton<PhotonLauncher>.Instance.DropChair(chair);
    }

    public void ShowCanDropChair(bool v)
    {
        Singleton<UiManager>.Instance.ShowCanDropChair(v);
    }
}
