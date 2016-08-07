using UnityEngine;
using System.Collections;

public class videoPlay : MonoBehaviour {

    public MovieTexture movTexture;
    void Start()
    {
        GetComponent<Renderer>().material.mainTexture = movTexture;
        movTexture.loop = true;
        movTexture.Play();
    }
}
