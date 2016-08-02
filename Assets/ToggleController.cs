using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToggleController : MonoBehaviour {

    public Toggle toggle;
    public int cc;
    bool toggleWasOn;
    // Use this for initialization
    void Start()
    {
        if (!toggle)
            toggle = GetComponentInChildren<Toggle>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!toggle.interactable)
        {
            bool test = MidiJack.MidiMaster.GetKeyDown(cc);
            if (test && !toggle.isOn)
            {
                toggle.isOn = true;
            }
            if (test && toggle.isOn)
            {
                toggle.isOn = false;
            }
        }
    }
}
