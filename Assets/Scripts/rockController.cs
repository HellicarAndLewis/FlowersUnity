using UnityEngine;
using System.Collections;

public class rockController : MonoBehaviour {

    public GameObject rock;
    [Range(-5, 5)]
    public float speed;
	// Use this for initialization
	void Start () {
        if (!rock)
            rock = GetComponent<GameObject>();
	}

    // Update is called once per frame
    void Update()
    {
        float x = rock.transform.eulerAngles.x;
        float y = rock.transform.eulerAngles.y;
        float z = rock.transform.eulerAngles.z;

        y += speed;

        rock.transform.eulerAngles = new Vector3(x, y, z);
    }
}
