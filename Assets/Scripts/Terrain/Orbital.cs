using UnityEngine;
using System.Collections;

public class Orbital : MonoBehaviour
{
    [Range(0, 5)]
    public float speed = 1;
    
    void Start()
    {

    }
    
    void Update()
    {
        transform.Rotate(speed, 0, 0);
    }
}