using UnityEngine;
using System.Collections;
using System;

public class UIUtils
{
    public const int uiDefaultWidth = 180;
    public const int uiDefaultHeight = 30;
    public const int uiDefaultPadding = 10;

    // --------------------------------------------------------------------------------------------------------
    static public void wrap(ref Vector2 _pos, int _uiWidth = uiDefaultWidth, int _uiHeight = uiDefaultHeight)
    {
        if (_pos.y + _uiHeight > Screen.height)
        {
            int padding = 10;
            _pos.x += _uiWidth + padding;
            _pos.y = padding;
        }
    }

    // --------------------------------------------------------------------------------------------------------
    static public bool drawToggle(string _name, ref bool _value,
                                  ref Vector2 _pos,
                                  int _uiWidth = uiDefaultWidth,
                                  int _uiHeight = uiDefaultHeight)
    {
        Rect tmpRect = new Rect(_pos, new Vector2(_uiHeight * 0.5f, _uiHeight * 0.5f));
        return GUI.Toggle(tmpRect, _value, _name);
    }

    // --------------------------------------------------------------------------------------------------------
    static public bool drawHorizontalSlider(string _name, ref float _value, float _min, float _max,
                                            ref Vector2 _pos,
                                            int _uiWidth = uiDefaultWidth,
                                            int _uiHeight = uiDefaultHeight)
    {
        bool valueChanged = false;

        //GUI.Label(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), _name + " " + _value.ToString() );
        GUI.Box(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight * (0.7f + 0.6f)), _name + ": " + _value.ToString("F3"));
        _pos.y += (float)_uiHeight * 0.7f; wrap(ref _pos, _uiWidth, _uiHeight);

        float tmpNewVal = GUI.HorizontalSlider(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), _value, _min, _max);
        if (tmpNewVal != _value)
        {
            _value = tmpNewVal;
            valueChanged = true;
        }
        _pos.y += _uiHeight * 0.7f; wrap(ref _pos, _uiWidth, _uiHeight);

        return valueChanged;
    }

    // --------------------------------------------------------------------------------------------------------
    static public bool drawHorizontalSliderMinMax(string _name, ref float _startValue, float _startMin, float _startMax,
                                                                ref float _endValue, float _endMin, float _endMax,
                                                                ref Vector2 _pos,
                                                                int _uiWidth = uiDefaultWidth, int _uiHeight = uiDefaultHeight)
    {
        bool valueChanged = false;

        int longLabelWidth = _uiWidth * 2; // TEMP, this label tends to spread over two lines breaking everything, so for now just let that run x 2 as wide

        GUI.Box(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight * (0.7f + 0.8f + 0.7f)), "");

        GUI.Label(new Rect(_pos.x, _pos.y, longLabelWidth, _uiHeight), _name + "  Min " + _startValue.ToString("F3") + "  Max " + _endValue.ToString("F3"));
        _pos.y += (float)_uiHeight * 0.7f; wrap(ref _pos, _uiWidth, _uiHeight);

        float tmpNewStartVal = GUI.HorizontalSlider(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight * 0.6f), _startValue, _startMin, _startMax);
        if (tmpNewStartVal != _startValue)
        {
            _startValue = tmpNewStartVal;
            valueChanged = true;
        }

        _pos.y += _uiHeight * 0.8f; wrap(ref _pos, _uiWidth, _uiHeight);

        float tmpNewEndVal = GUI.HorizontalSlider(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight * 0.6f), _endValue, _endMin, _endMax);
        if (tmpNewEndVal != _endValue)
        {
            _endValue = tmpNewEndVal;
            valueChanged = true;
        }

        _pos.y += _uiHeight * 0.7f; wrap(ref _pos, _uiWidth, _uiHeight);

        return valueChanged;
    }

    // --------------------------------------------------------------------------------------------------------
    static public bool drawHorizontalSlider(string _name, ref int _value, int _min, int _max, ref Vector2 _pos, int _uiWidth = uiDefaultWidth, int _uiHeight = uiDefaultHeight)
    {
        bool valueChanged = false;

        //GUI.Label(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), _name + " " + _value.ToString());
        GUI.Box(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight * (0.7f + 0.6f)), _name + ": " + _value.ToString());

        _pos.y += (float)_uiHeight * 0.7f; wrap(ref _pos, _uiWidth, _uiHeight);

        int tmpNewVal = (int)GUI.HorizontalSlider(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), (float)_value, (float)_min, (float)_max);
        if (tmpNewVal != _value)
        {
            _value = tmpNewVal;
            valueChanged = true;
        }
        _pos.y += _uiHeight * 0.7f; wrap(ref _pos, _uiWidth, _uiHeight);

        return valueChanged;
    }

    // --------------------------------------------------------------------------------------------------------
    static public void drawLabel(string _name, ref Vector2 _pos, int _uiWidth = uiDefaultWidth, int _uiHeight = uiDefaultHeight)
    {
        GUI.Label(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), _name);
        _pos.y += (float)_uiHeight * 0.7f;
    }

    // --------------------------------------------------------------------------------------------------------
    static public bool drawHorizontalSlider(string _name, ref Vector3 _vec, float _min, float _max, ref Vector2 _pos, int _uiWidth = uiDefaultWidth, int _uiHeight = uiDefaultHeight)
    {
        bool valueChanged = false;

        GUI.Box(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight * (0.7f + 0.8f + 0.8f + 0.8f)), _name + " " + _vec.x.ToString("F3") + ", " + _vec.y.ToString("F3") + ", " + _vec.z.ToString("F3"));
        _pos.y += (float)_uiHeight * 0.7f; wrap(ref _pos, _uiWidth, _uiHeight);

        // X
        float tmpNewX = GUI.HorizontalSlider(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight * 0.5f), _vec.x, _min, _max);
        if (tmpNewX != _vec.x)
        {
            _vec.x = tmpNewX;
            valueChanged = true;
        }
        _pos.y += _uiHeight * 0.8f; wrap(ref _pos, _uiWidth, _uiHeight);


        // Y
        float tmpNewY = GUI.HorizontalSlider(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight * 0.5f), _vec.y, _min, _max);
        if (tmpNewY != _vec.y)
        {
            _vec.y = tmpNewY;
            valueChanged = true;
        }
        _pos.y += _uiHeight * 0.8f; wrap(ref _pos, _uiWidth, _uiHeight);


        // X
        float tmpNewZ = GUI.HorizontalSlider(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight * 0.5f), _vec.z, _min, _max);
        if (tmpNewZ != _vec.z)
        {
            _vec.z = tmpNewZ;
            valueChanged = true;
        }
        _pos.y += _uiHeight * 0.8f; wrap(ref _pos, _uiWidth, _uiHeight);

        _pos.y += _uiHeight * 0.8f; wrap(ref _pos, _uiWidth, _uiHeight);

        return valueChanged;
    }

    // --------------------------------------------------------------------------------------------------------
    public static bool DrawTextField(string label, ref int value, ref Vector2 _pos, int _uiWidth = uiDefaultWidth, int _uiHeight = uiDefaultHeight)
    {
        var xReset = _pos.x;
        GUI.Label(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), label);
        _pos.x += _uiWidth;
        var textFieldValue = GUI.TextField(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), value.ToString());
        _pos.y += _uiHeight * 1.2f;
        _pos.x = xReset;
        int textFieldValuei = -1;
        if (Int32.TryParse(textFieldValue, out textFieldValuei) && textFieldValuei != value)
        {
            value = textFieldValuei;
            return true;
        }
        else
        {
            return false;
        }
    }

    // --------------------------------------------------------------------------------------------------------
    public static bool DrawTextField(string label, ref float value, ref Vector2 _pos, int _uiWidth = uiDefaultWidth, int _uiHeight = uiDefaultHeight)
    {
        var xReset = _pos.x;
        GUI.Label(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), label);
        _pos.x += _uiWidth;
        var textFieldValue = GUI.TextField(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), value.ToString());
        _pos.y += _uiHeight * 1.2f;
        _pos.x = xReset;
        float textFieldValuef = -1;
        if (float.TryParse(textFieldValue, out textFieldValuef) && textFieldValuef != value)
        {
            value = textFieldValuef;
            return true;
        }
        else
        {
            return false;
        }
    }


    // --------------------------------------------------------------------------------------------------------
    public static bool DrawButton(string label, ref Vector2 _pos, int _uiWidth = uiDefaultWidth, int _uiHeight = uiDefaultHeight)
    {
        var isPressed = GUI.Button(new Rect(_pos.x, _pos.y, _uiWidth, _uiHeight), label);
        _pos.y += _uiHeight * 1.2f;
        return isPressed;
    }

}
