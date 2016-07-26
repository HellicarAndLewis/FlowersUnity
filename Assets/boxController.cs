using UnityEngine;
using System.Collections;

public class boxController : MonoBehaviour {

    public onsetDetector onset;
    public GameObject box;
    public float onsetConfidence;

	// Use this for initialization
	void Start () {
        if (!onset)
            onset = FindObjectOfType<onsetDetector>();
        //if (!box)
            //box = GetComponent<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
        box.transform.localScale = new Vector3(1.0f, onset.onsetTotal,1.0f);
        onsetConfidence = onset.onsetTotal;

    }
}
