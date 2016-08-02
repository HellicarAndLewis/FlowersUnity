using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Captures textures from the specified camera and pipes them to an externall FFMPEG process
/// Requires there to be a copy of ffmpeg.exe in ".Capture/bin/ffmpeg.exe" relative to the data path
/// Requires a ".Capture/Videos/" directory to exist relative to the data path
/// </summary>
public class CaptureToPipe : Capture
{
    // --------------------------------------------------------------------------------------------------------
	//
    
	// Capture framerate
    public int frameRate = 25;

    // Constant Rate Factor
    // From https://trac.ffmpeg.org/wiki/Encode/H.264
    // The range of the quantizer scale is 0-51: where 0 is lossless, 23 is default, and 51 is worst possible.
    // A lower value is a higher quality and a subjectively sane range is 18-28.
    // Consider 18 to be visually lossless or nearly so: it should look the same or nearly the same as the input but it isn't technically lossless.
    [Range(0, 51)]
    public int crf = 15;

    // Duration in seconds
    public float duration = 6;

    // show the process window for debugging only
    public bool showProcessWindow = false;

    // --------------------------------------------------------------------------------------------------------
    //
    System.Diagnostics.Process process;


    // --------------------------------------------------------------------------------------------------------
    //
    protected override void OnTextureUpdated()
    {

        if (isCapturing)
        {
            var deltaTime = 1.0f / (float)frameRate;
            timeElapsed += deltaTime;
            realtimeElapsed += Time.deltaTime;
            CaptureTime.IsCapturing = true;
            CaptureTime.Delta = deltaTime;
            CaptureTime.Elapsed = timeElapsed;
            if (timeElapsed < duration)
            {
                isCapturing = true;
            }
            else
            {
                // recording is finished
                // fire an OnComplete event
                RaiseOnComplete();
                isCapturing = false;
                CaptureTime.IsCapturing = false;
                Debug.Log(string.Format("Raw frames to FFMPEG in {0} seconds", realtimeElapsed));
                // close the external ffmpeg process
                if (IsRunning(process)) process.Close();
            }
        }

        if (isCapturing)
        {
            if (!IsRunning(process)) Init();
            byte[] bytes = screenShot.GetRawTextureData();
            process.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
        }
        else
        {
            timeElapsed = 0;
            realtimeElapsed = 0;
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public virtual void OnGUI()
    {
        if (isGuiEnabled)
		{
            //GUI.FocusWindow();
			GUI.contentColor = guiTextColour;
            float padding = 10;
            int uiWidth = 256;
            int componentHeight = 20;
            Vector2 uiPos = new Vector2(padding, padding);
            UIUtils.drawLabel("Capture Frames To Pipe", ref uiPos, uiWidth, componentHeight * 2);

            if (screenShot && isCapturing)
            {
                // capture status label
                GUI.Label(new Rect(uiPos.x, uiPos.y, uiWidth, componentHeight), "In progress: piping to FFMEG");
                uiPos.y += componentHeight;
                var label = string.Format("Framerate:{2} Size:{0}x{1} crf:{3} duration:{4}",
                    width, height,
                    frameRate, crf, duration
                    );
                GUI.Label(new Rect(uiPos.x, uiPos.y, uiWidth, componentHeight), label);
                uiPos.y += componentHeight;
                GUI.Label(new Rect(uiPos.x, uiPos.y, uiWidth, componentHeight), string.Format("Time remaining: {0}", duration - CaptureTime.Elapsed));
                uiPos.y += componentHeight;
                // Draw the current frame being piped
                int guiFrameWidth = 324;
                int guiFrameHeight = 180;
                GUI.Box(new Rect(uiPos.x, uiPos.y, guiFrameWidth, guiFrameHeight), "");
                var framePadding = 6;
                GUI.DrawTexture(new Rect(uiPos.x + framePadding / 2, uiPos.y + framePadding / 2, guiFrameWidth - framePadding, guiFrameHeight - framePadding), screenShot, ScaleMode.ScaleToFit);
                uiPos.y += padding + guiFrameHeight;
                // cancel capture button
                if (UIUtils.DrawButton("cancel", ref uiPos)) isCapturing = false;

            }
            else
            {
                // sliders: width, height, framerate, CRF, duration
                // toggles: show process window,
                UIUtils.DrawTextField("width", ref width, ref uiPos, uiWidth / 2, componentHeight);
                UIUtils.DrawTextField("height", ref height, ref uiPos, uiWidth / 2, componentHeight);
                UIUtils.DrawTextField("framerate", ref frameRate, ref uiPos, uiWidth / 2, componentHeight);
                UIUtils.DrawTextField("CRF (0-51)", ref crf, ref uiPos, uiWidth / 2, componentHeight);
                UIUtils.DrawTextField("duration", ref duration, ref uiPos, uiWidth / 2, componentHeight);
                // process window toggle
                showProcessWindow = GUI.Toggle(new Rect(uiPos.x, uiPos.y, uiWidth, componentHeight), showProcessWindow, "show process window");
                uiPos.y += componentHeight + padding;
                // capture button
                if (UIUtils.DrawButton("capture", ref uiPos)) isCapturing = true;
            }

        }
    }


    // --------------------------------------------------------------------------------------------------------
    //
    void OnApplicationQuit()
    {
        if (IsRunning(process)) process.Close();
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void PreCapture()
    {
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void StartCapturing()
    {
        isCapturing = true;
    }


    // --------------------------------------------------------------------------------------------------------
    //
    public void Init()
    {
        // Set FFMPEG application path, media output path
		// default is for PC / Windows
		var ffmpegPath = string.Format("{0}/.Capture/bin/ffmpeg.exe", Application.dataPath);
		var outputPath = string.Format("\"{0}\"/.Capture/Videos/{1}.mp4", Application.dataPath, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

		// change the paths if this is OSX
        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
        {
			ffmpegPath = string.Format("{0}/.Capture/bin/ffmpeg", Application.dataPath);
			outputPath = string.Format("{0}/.Capture/Videos/{1}.mp4", Application.dataPath, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }

        // new Process to launch FFMPEG
        process = new System.Diagnostics.Process();
        process.StartInfo.FileName = ffmpegPath;
        process.EnableRaisingEvents = false;
        process.StartInfo.WorkingDirectory = string.Format("{0}/.Capture/bin", Application.dataPath);
        // This allows us to pipe in raw RGB24 data for each frame
        // the frames are upside-down so we need to vflip each one
        var args = string.Format(@"-f rawvideo -pix_fmt rgb24 -video_size {0}x{1} -framerate {2} -i pipe: -an -vcodec libx264 -s {0}x{1} -crf {3} -preset medium -pix_fmt yuv420p -vf 'vflip' -y {4}",
            width,
            height,
            frameRate,
            crf,
            outputPath);
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = !showProcessWindow;
        process.StartInfo.RedirectStandardInput = true;
        process.Start();

    }


    // --------------------------------------------------------------------------------------------------------
    //
    public static bool IsRunning(System.Diagnostics.Process proc)
    {
        try
        {
            return (!proc.HasExited && proc.Id != 0);
        }
        catch
        {
            return false;
        }
    }

}