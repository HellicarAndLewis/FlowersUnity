using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderController : MonoBehaviour {

    public Slider slider;
    public int cc;

	// Use this for initialization
	void Start () {
        if (!slider)
            slider = GetComponentInChildren<Slider>();
	}
	
	// Update is called once per frame
	void Update () {
        if(!slider.interactable)
        {
            float test = MidiJack.MidiMaster.GetKnob(cc, 0);
            slider.value = test;
        }
	}
}
