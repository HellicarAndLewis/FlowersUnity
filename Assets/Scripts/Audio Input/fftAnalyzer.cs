using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class fftAnalyzer : MonoBehaviour
{
    private int audioSampleRate = 44100;
    private int samples = 256;
    public float[] spectrum;
    public int bins;
    [Range(0, 1)]
    public float lerpSpeed;

    public Dictionary<string, ServerLog> servers;

    void Start()
    {
        spectrum = new float[samples];

        OSCHandler.Instance.Init();

        servers = new Dictionary<string, ServerLog>();
    }

    void Update()
    {
        OSCHandler.Instance.UpdateLogs();
        servers = OSCHandler.Instance.Servers;

        int newestIndex = servers["fftInput"].packets.Count - 1;
        UnityOSC.OSCPacket packet = servers["fftInput"].packets[newestIndex];
        int i = 0;
        foreach(object data in packet.Data)
        {
            float height = float.Parse(data.ToString());
            spectrum[i] = height;
            Debug.DrawLine(new Vector3(i / 10.0f, 0.0f, 0.0f), new Vector3(i / 10.0f, height, 0.0f));
            i++;
        }

        
    }
}