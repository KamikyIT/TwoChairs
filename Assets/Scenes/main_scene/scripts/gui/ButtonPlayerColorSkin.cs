using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonPlayerColorSkin : ButtonBaseData<PlayerProfile.PlayerSkinColors>
{
    Image _imageComponent;
    Image ImageComponent
    {
        get
        {
            if (_imageComponent == null)
                _imageComponent = GetComponent<Image>();
            return _imageComponent;
        }
    }

    public void DisplayColor()
    {
        ImageComponent.color = base.Data.ToColor();
    }
}