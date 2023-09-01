using UnityEngine.UIElements;

public class MessageForm : UIForm
{
    Button _okButton;
    Label _messageLabel;

    public string MessageText
    {
        get => _messageLabel.text;
        set => _messageLabel.text = value;
    }
    public override bool Init()
    {
        if (!base.Init()) return false;
        if ((_okButton = Q<Button>("OkBtn")) == null) return false;
        if ((_messageLabel = Q<Label>("MessageLabel")) == null) return false;
        _okButton.RegisterCallback<ClickEvent>(e => Hide());
        return true;
    }
    public MessageForm(UIMainWindow _parent) : base(_parent, "ApplicationMessageForm")
    {}
}
