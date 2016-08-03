using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class fftAnalyzer : MonoBehaviour
{
    private int audioSampleRate = 44100;
    private int samples = 256;
    public float[] spectrum;
    public float[] spectrumBinned;
    [Range(1, 256)]
    public int bins;

    // --------------------------------------------------------------------------------------------------------
    //
    private float average;
    private float peak;
    private Dictionary<string, ServerLog> servers;

    // --------------------------------------------------------------------------------------------------------
    //
    void Start()
    {
        spectrum = new float[samples];
        OSCHandler.Instance.Init();
    }

    // --------------------------------------------------------------------------------------------------------
    //
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
            average = 0;
            peak = -1;
            foreach (object data in packet.Data)
            {
                float height = float.Parse(data.ToString());
                spectrum[i] = height;
                average += height;
                peak = Mathf.Max(peak, height);
                Debug.DrawLine(new Vector3(i / 10.0f, 0.0f, 0.0f), new Vector3(i / 10.0f, spectrum[i], 0.0f));
                i++;
            }
            average /= i;

            if (bins > 0)
            {
                spectrumBinned = new float[bins];
                int spectrumChannelsPerBin = spectrum.Length / bins;
                for (i = 0; i < bins; i++)
                {
                    for (int j = i * spectrumChannelsPerBin; j < (i + 1) * spectrumChannelsPerBin; j++)
                    {
                        spectrumBinned[i] += spectrum[j];
                    }
                    spectrumBinned[i] /= spectrumChannelsPerBin;

                    Debug.DrawLine(new Vector3(i / 10.0f, 0.0f, 1.0f), new Vector3(i / 10.0f, spectrumBinned[i], 1.0f));
                }
            }
        }
    }
    

    // --------------------------------------------------------------------------------------------------------
    //
    public int Bins
    {
        get
        {
            return this.bins;
        }

        set
        {
            bins = Bins;
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public float Average
    {
        get
        {
            return this.average;
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public float Peak
    {
        get
        {
            return this.peak;
        }
    }
}