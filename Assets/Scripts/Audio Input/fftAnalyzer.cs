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
    }

    void Update()
    {
        OSCHandler.Instance.UpdateLogs();
        servers = OSCHandler.Instance.Servers;
        var packets = servers["fftInput"].packets;

        if (packets.Count > 0)
        {
            int newestIndex = packets.Count - 1;
            var packet = packets[newestIndex];
            int i = 0;
            foreach (object data in packet.Data)
            {
                float height = float.Parse(data.ToString());
                spectrum[i] = height;
                Debug.DrawLine(new Vector3(i / 10.0f, 0.0f, 0.0f), new Vector3(i / 10.0f, height, 0.0f));
                i++;
            }
        }
        
    }
}