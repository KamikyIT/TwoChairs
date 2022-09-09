using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonBase: MonoBehaviour
{
    public event Action OnClick;

    private Button _button;
    public Button Button
    {
        get
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();

                if (_button == null)
                    _button = gameObject.AddComponent<Button>();
            }
            return _button;
        }
    }

    public virtual void Awake() { }

    public bool Interactable
    {
        get { return Button.interactable; }
        set { Button.interactable = value; }
    }

    private void OnEnable()
    {
        Button.onClick.AddListener(OnClickHandler);
    }

    private void OnDisable()
    {
        Button.onClick.RemoveAllListeners();
    }

    public void Click()
    {
        OnClickHandler();
    }

    protected virtual void OnClickHandler()
    {
        OnClick?.Invoke();
    }
}
