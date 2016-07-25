using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class fftAnalyzer : MonoBehaviour
{
    public AudioSource audio;
    public float sensitivity = 100;
    public float loudness = 0;
    [Range(1, 64)]
    public int bins;
    float Resolution;
    public float[] spectrum;
    public float[] spectrumBinned;
    [Range(0, 1)]
    public float lerpSpeed;

    public bool maxMode;

    void Start()
    {
        if(!audio) audio = GetComponent<AudioSource>();
        audio.clip = Microphone.Start(null, true, 10, 44100);
        audio.loop = true;
        //audio.mute = true;
        spectrum = new float[64];
        string AudioInputDevice = null;
        while (!(Microphone.GetPosition(AudioInputDevice) > 0)) { }
        audio.Play();
    }

    float GetAveragedVolume()
    {
        float[] data = new float[spectrum.Length];
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
        float[] data = new float[spectrum.Length]; 
        audio.GetSpectrumData(data, 0, FFTWindow.BlackmanHarris);
        for(int i = 0; i < spectrum.Length; i++)
        {
            spectrum[i] = Mathf.Lerp(spectrum[i], data[i], lerpSpeed);
        }
    }

    void Update()
    {
        loudness = GetAveragedVolume() * sensitivity;
        FillSpectrum();
        if(bins > 0)
        {
            spectrumBinned = new float[bins];
            int binsPerGroup = spectrum.Length / bins;
            for(int i = 0; i < bins; i++)
            {
                spectrumBinned[i] = 0.0f;
                for(int j = binsPerGroup * i; j < (i+1) * binsPerGroup; j++)
                {
                    if(maxMode)
                    {
                        if (spectrum[j] > spectrumBinned[i])
                        {
                            spectrumBinned[i] = spectrum[j];
                        }
                    }
                    else
                    {
                        spectrumBinned[i] += spectrum[j];
                    }
                }
                spectrumBinned[i] /= binsPerGroup;
            }
            for (int i = 0; i < spectrumBinned.Length; i++)
            {
                float x = i / 10.0f;
                Debug.DrawLine(new Vector3(x, 0, 0), new Vector3(x, spectrumBinned[i] * 100, 0));
            }
        } else
        {
            for (int i = 0; i < spectrum.Length; i++)
            {
                float x = i / 10.0f;
                Debug.DrawLine(new Vector3(x, 0, 0), new Vector3(x, spectrum[i] * 100, 0));
            }
        }

    }
}