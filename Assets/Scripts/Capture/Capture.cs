using UnityEngine;
using System.Collections;

public class Capture : MonoBehaviour
{
    // --------------------------------------------------------------------------------------------------------
    // Capture & Encoding
    public bool isCapturing = false;
    public int width = 1280;
    public int height = 720;
    // Controls
    public bool isGuiEnabled = false;
	public Color guiTextColour = Color.black;
    public Camera captureCamera;
    [HideInInspector]
    public float realtimeElapsed = 0;

    // --------------------------------------------------------------------------------------------------------
    // Complete event: fired when a capture has completed
    [HideInInspector]
    public delegate void CompleteEvent();
    [HideInInspector]
    public event CompleteEvent OnComplete;

    // --------------------------------------------------------------------------------------------------------
    // privates
    protected float timeElapsed = 0;
    protected RenderTexture renderTexture;
    protected Texture2D screenShot;


    // --------------------------------------------------------------------------------------------------------
    //
    void Start()
    {
        AllocateTetures();
        if (!captureCamera)
            captureCamera = FindObjectOfType<Camera>();
    }

    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Updates the capture texture in a coroutine. 
    /// Late Update is used to make sure everything has finished updating, especially GPU trails
    /// </summary>
    void LateUpdate()
    {
        if (isCapturing)
        {
            StartCoroutine(UpdateTexture());
        }
    }

    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Override this to do things with the texture2d after it updates
    /// </summary>
    virtual protected void OnTextureUpdated()
    {

    }

    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Updates the render texture and reads pixels from it into the texture 2d
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateTexture()
    {
        // Wait for the end of the frame before capturing
        yield return new WaitForEndOfFrame();

        if (renderTexture.width != width)
            AllocateTetures();

        // Update the render texture
        captureCamera.targetTexture = renderTexture;
        captureCamera.Render();
        RenderTexture.active = renderTexture;

        // read pixels from the render texture into the texture2d
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        if (isGuiEnabled) screenShot.Apply();
        captureCamera.targetTexture = null;
        RenderTexture.active = null;

        OnTextureUpdated();
    }

    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Allocates a new render texture and texture2d
    /// </summary>
    protected void AllocateTetures()
    {
        // init with width, height, depth and texture format
        // For more texture formats, see: http://docs.unity3d.com/ScriptReference/TextureFormat.html
        renderTexture = new RenderTexture(width, height, 16);
        //renderTexture.antiAliasing = 4;
        screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    // --------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Fires an OnComplete event when a capture has completed
    /// </summary>
    protected void RaiseOnComplete()
    {
        if (OnComplete != null) OnComplete();
    }
}