using UnityEngine;
using System.Collections;

public class JumpingRock : MonoBehaviour
{

    public Vector3 floorPos;
    public Vector3 floatPos;
    public Vector3 target;
    public float lerpSpeed = 0.1f;
    public float lerpSpeedUp = 0.6f;
    public float lerpSpeedDown = 0.05f;
    public float scale = 1;
    public float timeScale;
    public float magnitude;

    protected fftAnalyzer fft;
    protected int fftIndex = 0;


    void Start()
    {
        floorPos = transform.position;
        floatPos = transform.position;
        floatPos.y = Random.Range(1, 2);
        target = floatPos;
        timeScale = Random.Range(0.05f, 1);
        magnitude = Random.Range(0.5f, 2);
        fft = FindObjectOfType<fftAnalyzer>();
        fftIndex = Random.Range(0, fft.spectrum.Length);
    }


    void Update()
    {
        if (fft)
        {
            var volume = fft.spectrum[fftIndex];
            var y = floorPos.y + (magnitude * volume * scale);
            if (y > target.y)
                lerpSpeed = lerpSpeedUp;
            else
                lerpSpeed = lerpSpeedDown;
            target.y = y;
            transform.position = Vector3.Lerp(transform.position, target, lerpSpeed);
        }
        else
        {
            target.y = floatPos.y + Mathf.Sin(CaptureTime.Elapsed * timeScale) * magnitude;
            transform.position = Vector3.Lerp(transform.position, target, lerpSpeed);
        }
    }
}