using UnityEngine;

public static class ExtensionsHelper
{
    public static Color ToColor(this PlayerProfile.PlayerSkinColors color)
    {
        switch (color)
        {
            case PlayerProfile.PlayerSkinColors.white:
                return Color.white;
            case PlayerProfile.PlayerSkinColors.black:
                return Color.black;
            case PlayerProfile.PlayerSkinColors.red:
                return Color.red;
            case PlayerProfile.PlayerSkinColors.blue:
                return Color.blue;
            case PlayerProfile.PlayerSkinColors.green:
                return Color.green;
            default:
                Debug.LogError($"{nameof(ToColor)} : Unhandled {color}");
                return Color.black;
        }
    }

    public static string PlayerName(this ExitGames.Client.Photon.Hashtable hashtable)
    {
        var res = string.Empty;
        if (hashtable.TryGetValue(PhotonLauncher.PLAYER_NAME_CUSTOM_PROPERTY, out var resObj) && resObj is string)
            res = (string)resObj;
        return res;
    }

    public static PlayerProfile.PlayerSkinColors PlayerSkinColor(this ExitGames.Client.Photon.Hashtable hashtable)
    {
        PlayerProfile.PlayerSkinColors res = default;
        if (hashtable.TryGetValue(PhotonLauncher.PLAYER_SKIN_COLOR_CUSTOM_PROPERTY, out var resObj) && resObj is int)
            res = (PlayerProfile.PlayerSkinColors)resObj;
        return res;
    }
}
