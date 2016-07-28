using UnityEngine;
using System.Collections;

public class DisplayScript : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON.
        // Check if additional displays are available and activate each.
        for(int i = 1; i < Display.displays.Length; i++)
        {
            Debug.Log("display " + i + " activated.");
            Display.displays[i].Activate();
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}