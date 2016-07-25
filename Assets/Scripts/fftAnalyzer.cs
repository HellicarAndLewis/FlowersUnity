using UnityEngine;
using System.Collections;

// [RequireComponent(typeof(AudioSource))]

public enum Frequency
{
    Speech = 8000,
    LowQuality = 11025,
    Wideband = 16000,
    Record = 22050,
    DAT = 32000,
    CD = 44100,
    Pro = 48000,
    DVD = 96000

}

public class fftAnalyzer : MonoBehaviour
{

    public Frequency samplingRate = Frequency.Speech;
    public float Loudness;
    private string device;
    AudioClip clipRecord = new AudioClip();
    int sampleWindow = 128;
    public int lengthSeconds = 1;
    public GameObject cube;

    void InitMic()
    {
        if (device == null) device = Microphone.devices[0];
        clipRecord = Microphone.Start(device, true, 1, (int)samplingRate);
    }

    void StopMicrophone()
    {
        Microphone.End(device);
    }

    float LevelMax()
    {
        float levelMax = 0;
        float[] waveData = new float[sampleWindow];
        int micPosition = Microphone.GetPosition(null) - (sampleWindow + 1); // null means the first microphone
        if (micPosition < 0) return 0;
        clipRecord.GetData(waveData, micPosition);
        // Getting a peak on the last 128 samples
        for (int i = 0; i < sampleWindow; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }
        return levelMax;
    }

    void Start()
    {
        InitMic();
    }

    void Update()
    {
        Loudness = LevelMax() * 10f;
        Vector3 Scale;
        Scale = new Vector3();
        Scale.Set(1.0f + Loudness, 1.0f + Loudness, 1.0f + Loudness);
        cube.transform.localScale = Scale;
    }

    //stop mic when loading a new level or quit application
    void OnDisable()
    {
        StopMicrophone();
    }

    void OnDestroy()
    {
        StopMicrophone();
    }
}