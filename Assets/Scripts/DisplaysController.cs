using UnityEngine;
using System.Collections;

public class DisplaysController : MonoBehaviour
{

    public Camera primaryCam;
    public bool isPrimaryFullscreen = false;
    public Vector4 primaryViewport = new Vector4(0, 0, 1872, 1584);

    public Camera secondaryCam;
    public bool isSecondaryFullscreen = false;
    public Vector4 secondaryViewport = new Vector4(0, 0, 1872, 1584);

    void Awake()
    {
        Refresh();
    }

    public void Refresh()
    {
        SetViewport(primaryCam, isPrimaryFullscreen, primaryViewport);
        if (secondaryCam) SetViewport(secondaryCam, isSecondaryFullscreen, secondaryViewport);
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

            var hScale = viewport.w / Screen.height;
            var wScale = viewport.z / Screen.width;

            if (Screen.height < viewport.w)
            {
                wScale *= (1/hScale);
                hScale = 1;
            }

            cam.rect = new Rect(0, 1 - hScale, wScale, hScale);

        }
        cam.enabled = true;
    }

    public void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 400, 400), string.Format("screen is {0} x {1}", Screen.width, Screen.height));
    }
}