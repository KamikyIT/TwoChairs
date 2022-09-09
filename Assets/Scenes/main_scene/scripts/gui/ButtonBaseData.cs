using System;

public abstract class ButtonBaseData<T> : ButtonBase
{
    public T Data { get; set; }

    public new event Action<T> OnClick;

    protected override void OnClickHandler()
    {
        OnClick?.Invoke(Data);
    }
}
