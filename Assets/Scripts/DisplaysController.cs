using UnityEngine;
using System.Collections;

public class DisplaysController : MonoBehaviour
{

    public Camera primaryCam;
    public bool isPrimaryFullscreen = true;
    public Vector4 primaryViewport = new Vector4(0, 0, 1872, 1584);

    public Camera secondaryCam;
    public bool isSecondaryFullscreen = true;
    public Vector4 secondaryViewport = new Vector4(0, 0, 1872, 1584);

    void Awake()
    {
        Refresh();
    }

    public void Refresh()
    {
        SetViewport(primaryCam, isPrimaryFullscreen, primaryViewport);
        SetViewport(secondaryCam, isSecondaryFullscreen, secondaryViewport);
    }
    
    void SetViewport(Camera cam, bool isFullscreen, Vector4 viewport)
    {
        if (isFullscreen)
        {
            cam.rect = new Rect(0, 0, 1, 1);
            cam.ResetAspect();
        }
        else
        {
            var aspect = viewport.z / viewport.w;
            cam.aspect = aspect;

            var wScale = viewport.z / Screen.width;
            var hScale = viewport.w / Screen.height;
            cam.rect = new Rect(0, 1 - hScale, wScale, hScale);

        }
        cam.enabled = true;
    }
}