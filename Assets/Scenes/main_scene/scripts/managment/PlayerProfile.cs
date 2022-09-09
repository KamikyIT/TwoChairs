using System;
using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    const PlayerSkinColors DEFAULT_SKIN_COLOR = PlayerSkinColors.white;

    public string PlayerName { get; private set; }
    public PlayerSkinColors PlayerSkinColor { get; private set; }

    /// <summary>
    /// Пользователь может заходить в игру(задано имя и цвет).
    /// </summary>
    public bool CanJoinGames
    {
        get
        {
            return !string.IsNullOrEmpty(PlayerName);
        }
    }

    public void Initialize()
    {
        PlayerName = PlayerPrefs.GetString(nameof(PlayerName), string.Empty);

        if (PlayerPrefs.HasKey(nameof(PlayerSkinColor)) && Enum.TryParse<PlayerSkinColors>(PlayerPrefs.GetString(nameof(PlayerSkinColor)), out var skinColor))
            PlayerSkinColor = skinColor;
        else
            PlayerSkinColor = DEFAULT_SKIN_COLOR;
    }

    public void SetPlayerProfile(string playerName, PlayerSkinColors playerSkinColor)
    {
        this.PlayerName = playerName;
        this.PlayerSkinColor = playerSkinColor;

        PlayerPrefs.SetString(nameof(PlayerName), this.PlayerName);
        PlayerPrefs.SetString(nameof(PlayerSkinColor), this.PlayerSkinColor.ToString());
        PlayerPrefs.Save();
    }

    #region Inner Types

    public enum PlayerSkinColors
    {
        white,
        black,
        red,
        blue,
        green
    }

    #endregion
}
