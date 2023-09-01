using UnityEngine.UIElements;

public class UIForm
{
    private string        _containerName;
    private VisualElement _container;
    private Button        _cancelBtn;
    private UIMainWindow  _parent;
    public UIMainWindow Parent => _parent;
    public virtual bool Init() 
    {
        if ((_container = Parent.Root.Q<VisualElement>(_containerName)) == null)return false;
        if ((_cancelBtn = _container.Q<Button>("CancelBtn")) == null) return false;
        _cancelBtn.RegisterCallback<ClickEvent>(evt => Hide());
        return true;
    }
    public void Hide() 
    {
        if (!IsShown) return;
        _container.style.display = DisplayStyle.None;
        _parent.active = null;
    }
    public void Show() 
    {   
        if (_parent.active != null) return;
        _container.style.display = DisplayStyle.Flex;
        _parent.active = this;
    }
    public bool IsShown => _container.style.display == DisplayStyle.Flex;
    public T Q<T>(string name) where T: VisualElement => _container.Q<T>(name);
    public UIForm(UIMainWindow _parent, string _containerName = "formContainer")
    {
        this._parent = _parent;
        this._containerName = _containerName;
        if (!Init()) throw new System.Exception("UIMainWindow elemente init error...");
    }
}