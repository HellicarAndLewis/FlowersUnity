using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class fftAnalyzer : MonoBehaviour
{
    public AudioSource audio;
    public float sensitivity = 100;
    public float loudness = 0;
    public int bins;
    float Resolution;
    public static float[] spectrum;

    void Start()
    {
        if(!audio) audio = GetComponent<AudioSource>();
        audio.clip = Microphone.Start(null, true, 10, 44100);
        audio.loop = true;
        //audio.mute = true;
        spectrum = new float[1024];
        string AudioInputDevice = null;
        while (!(Microphone.GetPosition(AudioInputDevice) > 0)) { }
        audio.Play();
    }

    float GetAveragedVolume()
    {
        float[] data = new float[1024];
        audio.GetOutputData(data, 0);
        float a = 0;
        foreach(float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / 256;
    }

    void FillSpectrum()
    {
        audio.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
    }

    void Update()
    {
        loudness = GetAveragedVolume() * sensitivity;
        FillSpectrum();
        for(int i = 0; i < spectrum.Length; i++)
        {
            float x = i / 10.0f;
            Debug.DrawLine(new Vector3(x, 0, 0), new Vector3(x, spectrum[i] * 100, 0));
        }

    }
}