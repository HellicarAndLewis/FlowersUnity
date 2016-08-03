using UnityEngine;
using System.Collections;

public class QuickController : MonoBehaviour
{
    [Range(0,1)]
    public float flowerScale = 0;
    TerrainFlowers[] flowers;

    // Use this for initialization
    void Start()
    {
        flowers = FindObjectsOfType<TerrainFlowers>();

    }

    // Update is called once per frame
    void Update()
    {
        foreach (var flower in flowers)
        {
            flower.flowerAlpha = flowerScale;
        }
    }
}
