using UnityEngine;
using System.Collections;

public class onsetDetector : MonoBehaviour {

    public fftAnalyzer fft;

    int lookBackWindow = 5;

    [HideInInspector]
    public float[] lastSpectrum;
    [HideInInspector]
    public float[] currentSpectrum;
    [Range(1.0f, 5.0f)]
    public float cutoff;
    //[HideInInspector]
    public float onsetTotal;
    [HideInInspector]
    public float onsetBass;
    [HideInInspector]
    public float onsetMid;
    [HideInInspector]
    public float onsetTreble;
    [Range(0.1f, 1.0f)]
    public float attackUp;
    [Range(0.01f, 1.0f)]
    public float attackDown;
    Queue spectralFlux;
    Queue spectralFluxBass;
    Queue spectralFluxMid;
    Queue spectralFluxTreble;


    // Use this for initialization
    void Start () {
        if (!fft) fft = GetComponent<fftAnalyzer>();
        lastSpectrum = new float[512];
        currentSpectrum = new float[lastSpectrum.Length];
        spectralFlux = new Queue(lookBackWindow);
        spectralFluxBass = new Queue(lookBackWindow);
        spectralFluxMid = new Queue(lookBackWindow);
        spectralFluxTreble = new Queue(lookBackWindow);
    }

    // Update is called once per frame
    void Update() {
        currentSpectrum = fft.spectrum;

        float flux = 0;
        float fluxBass = 0;
        float fluxMid = 0;
        float fluxTreble = 0;

        for (int i = 0; i < currentSpectrum.Length; i++)
        {
            float value = (currentSpectrum[i] - lastSpectrum[i]);
            flux += (value < 0) ? 0 : value;
            if (i < currentSpectrum.Length / 3)
            {
                fluxBass += (value < 0) ? 0 : value;
            }
            else if (i < currentSpectrum.Length * 2 / 3)
            {
                fluxMid += (value < 0) ? 0 : value;
            }
            else
            {
                fluxTreble += (value < 0) ? 0 : value;
            }
        }

        float confidence = CalculateSpectrum(flux, spectralFlux, lookBackWindow, 1);
        onsetTotal = getConfidenceSmoothed(confidence, onsetTotal);

        confidence = CalculateSpectrum(fluxBass, spectralFluxBass, lookBackWindow, 2);
        onsetBass = getConfidenceSmoothed(confidence, onsetBass);

        confidence = CalculateSpectrum(fluxMid, spectralFluxMid, lookBackWindow, 3);
        onsetMid = getConfidenceSmoothed(confidence, onsetMid);

        confidence = CalculateSpectrum(fluxMid, spectralFluxTreble, lookBackWindow, 4);
        onsetTreble = getConfidenceSmoothed(confidence, onsetTreble);

        currentSpectrum.CopyTo(lastSpectrum, 0);
    }

    float CalculateSpectrum(float flux, Queue spectralFlux, int window, int offset = 0)
    {
        spectralFlux.Enqueue(flux);
        if (spectralFlux.Count > window)
            spectralFlux.Dequeue();

        float mean = 0;
        foreach (float f in spectralFlux)
        {
            mean += f;
        }
        mean /= window;
        float threshold = mean * cutoff;

        int step = 0;
        float diff;
        foreach (float f in spectralFlux)
        {
            diff = f - threshold;
            Debug.DrawLine(new Vector3(step / 10.0f, 0, 1.0f + offset), new Vector3(step / 10.0f, 10.0f * diff, 1.0f + offset), Color.red);
            step++;
        }
        diff = flux - threshold;
        float confidence = map(diff, 0.0f, 0.2f, 0.0f, 1.0f, true);

        return confidence;
    }

    float getConfidenceSmoothed(float newConfidence, float oldConfidence)
    {
        float output;
        if (newConfidence > oldConfidence)
        {
            return output = Mathf.Lerp(oldConfidence, newConfidence, attackUp);
        }
        else
        {
            return output = Mathf.Lerp(oldConfidence, newConfidence, attackDown);
        }
    }

    float map(float val, float minIn, float maxIn, float minOut, float maxOut, bool clamp)
    {
        float output = (val - minIn) / (maxIn - minIn) * (maxOut - minOut) + minOut;
        if(clamp)
        {
            if (output < minOut)
            {
                output = minOut;
            }
            else if (output > maxOut)
            {
                output = maxOut;
            }
        }

        return output;
    }
}
