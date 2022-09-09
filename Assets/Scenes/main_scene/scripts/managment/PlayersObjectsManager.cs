using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayersObjectsManager : MonoBehaviour
{
    const string PLAYER_PREFAB_NAME = @"Player";

    [SerializeField]
    Transform _playerSpawnPosition;

    Dictionary<Player, PlayerMovement> _players;
    bool _initialized;

    public void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;
        _players = new Dictionary<Player, PlayerMovement>();
    }

    public async UniTask InitializeOtherPlayersAsync()
    {
        var otherPlayers = PhotonNetwork.CurrentRoom.Players.Values;

        // Т.к. Создание игровых объектов других игроков произойдет на следующем кадре , то ждем.
        await Cysharp.Threading.Tasks.UniTask.NextFrame();

        if (otherPlayers != null && otherPlayers.Any())
        {
            foreach (var otherPlayer in otherPlayers)
            {
                if (otherPlayer == PhotonNetwork.LocalPlayer)
                    continue;

                var targetPlayerComponent = FindPlayerGameObject(otherPlayer);

                if (targetPlayerComponent != null)
                    targetPlayerComponent.Initialize(otherPlayer.CustomProperties.PlayerName(), otherPlayer.CustomProperties.PlayerSkinColor(), false);
            }
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer == PhotonNetwork.LocalPlayer)
            return;

        var targetPlayerComponent = FindPlayerGameObject(targetPlayer);

        if (targetPlayerComponent != null)
            targetPlayerComponent.Initialize(changedProps.PlayerName(), changedProps.PlayerSkinColor(), false);
    }

    public void PlayerLeftRoom(Player leftPlayer)
    {
        if (Singleton<PhotonLauncher>.Instance.FindChairByPlayer(leftPlayer, out var chair))
        {
            var leftPlayerComponent = Singleton<PlayersObjectsManager>.Instance.FindPlayerGameObject(leftPlayer);

            leftPlayerComponent.DropChair(chair);

            chair.GrabberPlayer = null;
        }

        if (_players.ContainsKey(leftPlayer))
            _players.Remove(leftPlayer);
    }

    public void ThisPlayerEnteredRoom()
    {
        Debug.Log($"ThisPlayerEnteredRoom");

        var go = PhotonNetwork.Instantiate(PLAYER_PREFAB_NAME, _playerSpawnPosition.position, Quaternion.identity, 0);

        var playerMovement = go.GetComponent<PlayerMovement>();

        playerMovement.SetCameraOnPlayer(Camera.main);
        playerMovement.Initialize(Singleton<PlayerProfile>.Instance.PlayerName, Singleton<PlayerProfile>.Instance.PlayerSkinColor, true);

        _players.Add(PhotonNetwork.LocalPlayer, playerMovement);

        _ = InitializeOtherPlayersAsync();
    }

    public PlayerMovement FindPlayerGameObject(Player player)
    {
        if (_players.TryGetValue(player, out var targetPlayerComponent))
            return targetPlayerComponent;

        // Не совсем понимаю, на каком кадре фотон создает игровой объект другого игрока.
        targetPlayerComponent = FindObjectsOfType<PlayerMovement>().SingleOrDefault(x => x.GetComponent<PhotonView>().Owner == player);
        if (targetPlayerComponent == null)
            Debug.LogError($"Not found targetPlayerComponent for {player.ActorNumber}");
        else
            _players.Add(player, targetPlayerComponent);

        return targetPlayerComponent;
    }
}
