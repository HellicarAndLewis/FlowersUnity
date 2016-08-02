using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToggleController : MonoBehaviour {

    public Toggle toggle;
    public int cc;
    bool keyWasDown;
    // Use this for initialization
    void Start()
    {
        if (!toggle)
            toggle = GetComponentInChildren<Toggle>();

        keyWasDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!toggle.interactable)
        {
            bool isDown = MidiJack.MidiMaster.GetKeyDown(cc);
            if (isDown && !keyWasDown)
            {
                toggle.isOn = !toggle.isOn;
                keyWasDown = true;
            }
            bool isUp = MidiJack.MidiMaster.GetKeyUp(cc);
            if(isUp && keyWasDown)
            {
                keyWasDown = false;
            }
        }
    }
}
