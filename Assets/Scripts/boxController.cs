using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class boxController : MonoBehaviour {

    public onsetDetector onset;
    public GameObject rock;
    public float onsetConfidence;
    [Range(0, 5.0f)]
    public float speed;

	// Use this for initialization
	void Start () {
        if (!onset)
            onset = FindObjectOfType<onsetDetector>();
        //if (!box)
            //box = GetComponent<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
        //box.transform.localScale = new Vector3(1.0f, onset.onsetTotal,1.0f);
        //onsetConfidence = onset.onsetTotal;

        float x = rock.transform.eulerAngles.x;
        float y = rock.transform.eulerAngles.y;
        float z = rock.transform.eulerAngles.z;

        y += speed;

        rock.transform.eulerAngles = new Vector3(x, y, z);
    }

    public void onSliderChanged(float _speed)
    {
        speed = _speed * 5.0f;
    }
}
