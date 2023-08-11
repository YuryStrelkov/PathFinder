using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementUtils
{
    // создаёт что угодно
    public static InstanceType Create<InstanceType, ParentType>(ParentType parent, Vector4 bounds, Vector4 padding, LengthUnit sizeType = LengthUnit.Percent,
                                                         Position positionType = Position.Absolute)
                                                         where ParentType : VisualElement
                                                         where InstanceType : VisualElement, new()
    {
        InstanceType instance = new InstanceType();
        instance.style.position = positionType;
        SetElementSize(instance, bounds, padding, sizeType);
        if (parent == null) return instance;
        parent.Add(instance);
        return instance;
    }

    // Задаёт размер чего угодно. Margin  всегда 0
    public static void SetElementSize<T>(this T element, Vector4 bounds, Vector4 padding, LengthUnit sizeType = LengthUnit.Percent) where T : VisualElement
    {
        element.style.marginLeft = new StyleLength(new Length(0.0f, sizeType));
        element.style.marginRight = new StyleLength(new Length(0.0f, sizeType));
        element.style.marginTop = new StyleLength(new Length(0.0f, sizeType));
        element.style.marginBottom = new StyleLength(new Length(0.0f, sizeType));

        element.style.left = new StyleLength(new Length(bounds.x + padding.x, sizeType));
        element.style.top = new StyleLength(new Length(bounds.y + padding.z, sizeType));
        element.style.width = new StyleLength(new Length(bounds.z - padding.x - padding.y, sizeType));
        element.style.height = new StyleLength(new Length(bounds.w - padding.w - padding.z, sizeType));

        element.style.paddingLeft = new StyleLength(new Length(padding.x, sizeType));
        element.style.paddingRight = new StyleLength(new Length(padding.y, sizeType));
        element.style.paddingTop = new StyleLength(new Length(padding.z, sizeType));
        element.style.paddingBottom = new StyleLength(new Length(padding.w, sizeType));
    }

    // кнопка
    public static Button CreateButton<T>(T parent = null, string name = "button") where T : VisualElement
    {
        Button btn = new Button();
        btn.AddToClassList("ButtonsPathViewer");
        btn.text = name;
        //btn.style.backgroundColor = new StyleColor(new Color(1.0f, 0.0f, 1.0f));
        btn.style.flexShrink = new StyleFloat(1.0f);
        btn.style.flexGrow = new StyleFloat(1.0f);
        //btn.style.fontSize = new StyleLength(new Length(14.0f, LengthUnit.Pixel));
        if (parent == null) return btn;
        parent.Add(btn);
        return btn;
    }

    //scrollview
    public static ScrollView CreateScrollView<T>(T parent = null, Position positionType = Position.Relative) where T : VisualElement
    {
        ScrollView scrollView = new ScrollView();        
        scrollView.style.position = new StyleEnum<Position>(positionType);
        scrollView.style.flexShrink = new StyleFloat(1.0f);
        scrollView.style.flexGrow = new StyleFloat(0.0f);
        //btn.style.fontSize = new StyleLength(new Length(14.0f, LengthUnit.Pixel));
        if (parent == null) return scrollView;
        parent.Add(scrollView);
        return scrollView;
    }

    //dropdown
    public static DropdownField CreateDropDown<T>(T parent = null, string name = "dropdown", Position positionType = Position.Relative, List<string> items = null) where T : VisualElement
    {
        DropdownField ddf = new DropdownField(name, items, 0);
        ddf.style.position = new StyleEnum<Position>(positionType);
        ddf.AddToClassList("DropDowns");
        //ddf.text = name;
        ddf.style.flexShrink = new StyleFloat(1.0f);
        ddf.style.flexGrow = new StyleFloat(0.0f);
        if (parent == null) return ddf;
        parent.Add(ddf);
        return ddf;
    }

    // Создаёт группу
    public static GroupBox CreateGroup<T>(T parent = null, bool vertical = false, Position positionType = Position.Relative) where T : VisualElement
    {
        GroupBox group = new GroupBox();
        group.AddToClassList("Containers");
        // Отступ внурти и снаружи 3pix
        // Размер - 100% родителя
        //SetElementSize(group, new Vector4(0, 0, 100, 100), new Vector4(0, 0, 0, 0), LengthUnit.Percent);
        group.style.position = new StyleEnum<Position>(positionType);
        group.style.flexShrink = new StyleFloat(1.0f);
        group.style.flexGrow = new StyleFloat(1.0f);
        group.style.flexDirection = new StyleEnum<FlexDirection>(vertical ? FlexDirection.Column : FlexDirection.Row);
        if (parent == null) return group;
        parent.Add(group);
        return group;
    }

    public static Label[] InitTextNamedField<T>(T parent = null, string textFieldName = "labelname", string textFieldDefaultValue = "labelvalue") where T : VisualElement
    {
        GroupBox namedTextField = CreateGroup(parent, false);
        namedTextField.AddToClassList("namedTextField");

        Label name = new Label();
        name.AddToClassList("namedTextFieldName");
        name.text = textFieldName;
        name.name = "TextLabel";
        name.style.width = new StyleLength(new Length(30.0f, LengthUnit.Percent));
        name.style.height = new StyleLength(new Length(100.0f, LengthUnit.Percent));
        Label value = new Label();
        value.AddToClassList("namedTextFieldValue");
        value.style.width = new StyleLength(new Length(30.0f, LengthUnit.Percent));
        value.style.height = new StyleLength(new Length(100.0f, LengthUnit.Percent));
        value.text = textFieldDefaultValue;
        value.name = "TextValue";

        namedTextField.Add(name);
        namedTextField.Add(value);

        return new Label[] {name, value};
    }
}
