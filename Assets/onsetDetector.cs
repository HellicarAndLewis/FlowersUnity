using UnityEngine;
using System.Collections;

public class onsetDetector : MonoBehaviour {

    public fftAnalyzer fft;

    int lookBackWindow = 5;

    public float[] lastSpectrum;
    public float[] currentSpectrum;
    [Range(1.0f, 2.0f)]
    public float sensitivity;
    public float onsetConfidence;
    [Range(0.1f, 1.0f)]
    public float attackUp;
    [Range(0.1f, 1.0f)]
    public float attackDown;
    Queue spectralFlux;

	// Use this for initialization
	void Start () {
        if (!fft) fft = GetComponent<fftAnalyzer>();
        lastSpectrum = new float[1024];
        currentSpectrum = new float[1024];
        spectralFlux = new Queue(lookBackWindow);
	}
	
	// Update is called once per frame
	void Update () {
        currentSpectrum = fft.spectrum;

        float flux = 0;
        for(int i = 0; i < currentSpectrum.Length; i++)
        {
            float value = (currentSpectrum[i] - lastSpectrum[i]);
            flux += (value < 0) ? 0 : value;
        }
        spectralFlux.Enqueue(flux);
        if (spectralFlux.Count > lookBackWindow)
            spectralFlux.Dequeue();

        float mean = 0;
        foreach (float f in spectralFlux)
        {
            mean += f;
        }
        mean /= lookBackWindow;
        float threshold = mean * sensitivity;

        int step = 0;
        float diff;
        foreach (float f in spectralFlux)
        {
            diff = f - threshold;
            Debug.DrawLine(new Vector3(step / 10.0f, 0, 1.0f), new Vector3(step / 10.0f, 10.0f * diff, 1.0f), Color.red);
            step++;
        }
        diff = flux - threshold;
        float confidence = map(diff, 0.0f, 0.2f, 0.0f, 1.0f, true);
        if(confidence > onsetConfidence)
            onsetConfidence = Mathf.Lerp(onsetConfidence, confidence, attackUp);
        else
            onsetConfidence = Mathf.Lerp(onsetConfidence, confidence, attackDown);

        currentSpectrum.CopyTo(lastSpectrum, 0);
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
