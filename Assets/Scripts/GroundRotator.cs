using UnityEngine;
using System.Collections;

public class GroundRotator : MonoBehaviour
{

    public GameObject ground;
    [Range(-1, 1)]
    public float speedX;
    [Range(-1, 1)]
    public float speedY;
    [Range(-1, 1)]
    public float speedZ;
    public Vector3 vec;
    // Use this for initialization
    void Start()
    {
        if (!ground)
            ground = GetComponent<GameObject>();

        vec = ground.transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        vec.x += speedX;
        vec.y += speedY;
        vec.z += speedZ;

        ground.transform.eulerAngles = vec;
    }
}
