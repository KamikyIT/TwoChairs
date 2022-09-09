using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhotonLauncher : MonoBehaviourPunCallbacks, IConnectionCallbacks
{
    const int MAX_PLAYERS_IN_ROOM = 2;

    public const string PLAYER_NAME_CUSTOM_PROPERTY = "PlayerName";
    public const string PLAYER_SKIN_COLOR_CUSTOM_PROPERTY = "PlayerSkin";

    [SerializeField]
    PhotonView _photonLauncherView;

    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    string _gameVersion = "1";
    bool _initialized;
    bool _bucyRunningConnectAsync;
    RandomRoomConnectionResult _randomRoomConnectionResult;

    public async UniTask<RandomRoomConnectionResult> ConnectRandomRoomAsync()
    {
        if (_bucyRunningConnectAsync)
            return null;

        _bucyRunningConnectAsync = true;

        Connect();

        await UniTask.Delay(2000);

        await UniTask.WaitUntil(() => _bucyRunningConnectAsync == false);

        return _randomRoomConnectionResult;
    }
    public void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;

        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void SetPlayerProfile(string playerName, PlayerProfile.PlayerSkinColors playerSkinColor)
    {
        var customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties.Add(PLAYER_NAME_CUSTOM_PROPERTY, playerName);
        customProperties.Add(PLAYER_SKIN_COLOR_CUSTOM_PROPERTY, playerSkinColor);
        PhotonNetwork.SetPlayerCustomProperties(customProperties);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public bool FindChairByPlayer(Player player, out Chair chair)
    {
        chair = FindObjectsOfType<Chair>().FirstOrDefault(x => x.OccupantPlayer == player || x.GrabberPlayer == player);

        return chair != null;
    }

    #region IConnectionCallbacks

    public override void OnConnected()
    {
        Debug.Log($"OnConnected");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");

        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"OnDisconnected : {cause}");

        _bucyRunningConnectAsync = false;

        _randomRoomConnectionResult = new RandomRoomConnectionResult(success: false);
    }

    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log($"OnRegionListReceived : {regionHandler.EnabledRegions.Count}");
    }

    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.Log($"OnCustomAuthenticationResponse");
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.Log($"OnCustomAuthenticationFailed");
    }

    public override void OnJoinedRoom()
    {
        _bucyRunningConnectAsync = false;

        _randomRoomConnectionResult = new RandomRoomConnectionResult(success: true);

        Singleton<ApplicationCore>.Instance.ThisPlayerEnteredRoom();

        SetPlayerProfile(Singleton<PlayerProfile>.Instance.PlayerName, Singleton<PlayerProfile>.Instance.PlayerSkinColor);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);

        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MAX_PLAYERS_IN_ROOM, });

        Debug.Log($"OnJoinRandomFailed : {returnCode} '{message}'");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Когда другой игрок зашел.
        base.OnPlayerEnteredRoom(newPlayer);

        Singleton<ApplicationCore>.Instance.PlayerEnteredRoom(newPlayer);
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Когда другой игрок вышел.
        base.OnPlayerLeftRoom(otherPlayer);

        Singleton<ApplicationCore>.Instance.OnPlayerLeftRoom(otherPlayer);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        var str = $"OnPlayerPropertiesUpdate : {targetPlayer.ActorNumber} : \n";
        foreach (var pro in changedProps)
            str += pro.Key.ToString() + " " + pro.Value.ToString() + "\n";

        Debug.Log(str);

        Singleton<ApplicationCore>.Instance.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
    }

    #endregion

    public void LeaveChair(Chair chair)
    {
        var parameters = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, chair.ChairId };

        _photonLauncherView.RPC(nameof(LeaveChairRPC), RpcTarget.AllBuffered, parameters);
    }

    public void DropChair(Chair chair)
    {
        var parameters = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, chair.ChairId };

        _photonLauncherView.RPC(nameof(DropChairRPC), RpcTarget.AllBuffered, parameters);
    }

    public void OccupateChair(Chair chair)
    {
        var parameters = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, chair.ChairId };

        _photonLauncherView.RPC(nameof(OccupateChairRPC), RpcTarget.AllBuffered, parameters);
    }

    public void GrabChair(Chair chair)
    {
        var parameters = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, chair.ChairId };

        _photonLauncherView.RPC(nameof(GrabChairRPC), RpcTarget.AllBuffered, parameters);
    }

    bool FindPlayerAndChair(int actorNumber, string chairId, out Player targetPlayer, out Chair targetChair)
    {
        targetPlayer = null;
        targetChair = null;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorNumber)
            {
                targetPlayer = player;
                break;
            }
        }

        if (targetPlayer == null)
        {
            Debug.LogError($"Not found player with actorNumber : {actorNumber}");
            return false;
        }

        targetChair = Singleton<ChairsObjectsManager>.Instance.FindChair(chairId);

        if (targetChair == null)
        {
            Debug.LogError($"Not found chair with chairId : {chairId}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    void Connect()
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = _gameVersion;

            Debug.Log($"PhotonNetwork.ConnectUsingSettings();");
        }
    }

    [PunRPC]
    public void LeaveChairRPC(int actorNumber, string chairId)
    {
        if (FindPlayerAndChair(actorNumber, chairId, out var targetPlayer, out var targetChair))
        {
            if (targetPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                var playerComponent = Singleton<PlayersObjectsManager>.Instance.FindPlayerGameObject(targetPlayer);

                playerComponent.StandUpFromChair(targetChair);
            }

            targetChair.OccupantPlayer = null;
        }
    }

    [PunRPC]
    public void DropChairRPC(int actorNumber, string chairId)
    {
        if (FindPlayerAndChair(actorNumber, chairId, out var targetPlayer, out var targetChair))
        {

            var playerComponent = Singleton<PlayersObjectsManager>.Instance.FindPlayerGameObject(targetPlayer);

            playerComponent.DropChair(targetChair);

            targetChair.GrabberPlayer = null;
        }
    }

    [PunRPC]
    public void OccupateChairRPC(int actorNumber, string chairId)
    {
        if (FindPlayerAndChair(actorNumber, chairId, out var targetPlayer, out var targetChair))
            targetChair.OccupantPlayer = targetPlayer;
    }

    [PunRPC]
    public void GrabChairRPC(int actorNumber, string chairId)
    {
        if (FindPlayerAndChair(actorNumber, chairId, out var targetPlayer, out var targetChair))
            targetChair.GrabberPlayer = targetPlayer;
    }

    #region Inner Types

    public class RandomRoomConnectionResult
    {
        public RandomRoomConnectionResult(bool success, short returnCode = 0, string message = "")
        {
            Success = success;
            ReturnCode = returnCode;
            ErrorMessage = ErrorMessage;
        }

        public bool CanJoinRoom { get; set; }
        public bool Success { get; set; }
        public short ReturnCode { get; set; }
        public string ErrorMessage { get; set; }

    }

    #endregion
}
