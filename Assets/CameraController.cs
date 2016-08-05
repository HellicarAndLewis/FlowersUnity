using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform mainCam;
    //public Transform secondaryCam;
    public fftAnalyzer fft;
    bool posConnected;
    bool rotConnected;

    public float ZOffset;
    public float ZRotation;

    // Use this for initialization
    void Start () {
        if (!mainCam)
            mainCam = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update () {
        if (posConnected)
        {
            ZOffset = fft.spectrumBinned[0] * 20;
        }
        if(rotConnected)
        {
            ZRotation = fft.spectrumBinned[0] * 360;
        }
        mainCam.localPosition = new Vector3(0.0f, 0.0f, -60 + ZOffset);
        mainCam.eulerAngles = new Vector3(0.0f, 0.0f, ZRotation);
    }

    public void OnPosConnected(bool _val)
    {
        posConnected = _val;
    }

    public void OnRotConnected(bool _val)
    {
        rotConnected = _val;
    }

    public void onOffset(float _val)
    {
        ZOffset = _val * 100;
    }

    public void OnRotation(float _val)
    {
        ZRotation = _val * 360;
    }
}
