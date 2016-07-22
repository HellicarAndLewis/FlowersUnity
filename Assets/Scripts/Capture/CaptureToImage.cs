using UnityEngine;
using System.Collections;
using System.IO;

public class CaptureToImage : Capture
{
    public string directory = "";
    public string path = "";
    public string filename = "";
    public bool hasCaptured = false;

    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            isCapturing = true;
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    protected override void OnTextureUpdated()
    {
        if (isCapturing)
        {
            isCapturing = false;
            byte[] data = screenShot.EncodeToPNG();
            if (directory == "")
                directory = string.Format("{0}/.Capture/Images", Application.dataPath);
            if (!File.Exists(directory))
                Directory.CreateDirectory(directory);
            filename = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            path = string.Format("{0}/{1}.png", directory, filename);
            File.WriteAllBytes(path, data);
            hasCaptured = true;
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void CaptureFrame()
    {
        isCapturing = true;
    }

    public Texture2D GetTexture()
    {
        return screenShot;
    }
}