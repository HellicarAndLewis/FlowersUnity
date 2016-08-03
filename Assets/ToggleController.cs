using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToggleController : MonoBehaviour {

    public Toggle toggle;
    private bool wasDown;
    public int cc;

	// Use this for initialization
	void Start () {
        if (!toggle)
            toggle = GetComponentInChildren<Toggle>();
        wasDown = false;
	}
	
	// Update is called once per frame
	void Update () {
        if(!toggle.IsInteractable())
        {
            bool test = MidiJack.MidiMaster.GetKeyDown(cc);
            if(test && !wasDown)
            {
                toggle.isOn = true;
                wasDown = true;
            }
            else if(test && wasDown)
            {
                toggle.isOn = false;
                wasDown = false;
            }
        }
	}
}
