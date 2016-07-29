////using UnityEngine;
////using System.Collections;
////using System;
////using System.Net;

//using UnityEngine;
//using System;
//using System.Net;


//// Gerry Beauregard's C# in place FFT,
//// adapted to handle float arrays.
//// See the FFTModule class for simple,
//// pre-configured use in the inspector.

//public class fftAnalyzer : MonoBehaviour
//{

//    public int channel = 0; // the channel to analyze

//    public int OutputLength
//    {
//        get
//        {
//            if (_doFFT == false)
//                return 0;

//            return fftSize / 2;
//        }
//    }

//    // FFT Arrays
//    private float[] arrRe;
//    private float[] arrI;

//    //GZComment removed this
//    //public float[] finalFFT;

//    // FFT
//    private FloatFFT fft;

//    //GZComment: fft size will be grabbed from buffer length
//    private int fftSize;

//    // Windowing
//    private float[] window;

//    //GZComment: Changed this to private, camelCased
//    private float[] fftOutput;

//    private float[] threadSafeOutput;

//    private bool _doFFT; // set to true when all is initialised. That way, no need for an expensive try catch.

//    private object _lockObject = new object(); //locking token

//    // Init
//    void Awake()
//    {
//        //GZComment: grab dsp buffer size
//        int dspBufferLength, numBuffers;
//        AudioSettings.GetDSPBufferSize(out dspBufferLength, out numBuffers);

//        if (Debug.isDebugBuild)
//        {
//            Debug.Log("audio buffer size: " + dspBufferLength);
//        }
//        // We'll fft in real time, once per audio thread loop, so fft size is buffer size
//        fftSize = dspBufferLength;

//        arrRe = new float[fftSize];
//        arrI = new float[fftSize];

//        window = new float[fftSize];
//        window = Hanning(window);

//        fftOutput = new float[fftSize / 2];

//        //FFT
//        fft = new FloatFFT();
//        uint logN = (uint)Math.Log(fftSize, 2);

//        fft.init(logN);
//        _doFFT = true;
//    }




//    // Do FFT
//    void OnAudioFilterRead(float[] data, int channels)
//    {
//        if (_doFFT == false)
//            return;

//        // Step 1 : de-interleave
//        int j = 0;
//        for (int i = channel; i < data.Length; i += channels)
//        {
//            arrRe[j] = data[i]; //Assumes buffer is already allocated
//            Debug.Log(data[i]);
//            j++;
//        }

//        // Apply precalculated windowing
//        for (int i = 0; i < arrRe.Length; i++)
//            arrRe[i] *= window[i];

//        // Clear array, GZComment: don't allocate new, but clear!
//        System.Array.Clear(arrI, 0, fftSize);

//        // run the fft
//        fft.run(arrRe, arrI);

//        // Compute magnitude, in place
//        // Only compute half, as other half is useless
//        for (int i = 0; i < fftSize / 2; i++)
//            arrRe[i] = Mathf.Sqrt(arrRe[i] * arrRe[i] + arrI[i] * arrI[i]);

//        // We only lock the final copy, so that if the main thread requests
//        // spectrum data, it at worst will wait the time it takes to copy, not
//        // the time it takes to de-interleave + window + fft + compute mags
//        lock (_lockObject)
//        {
//            //finalFFT = arrRe;
//            // GZComment: don't assign, copy!
//            System.Array.Copy(arrRe, fftOutput, fftSize / 2);
//        }
//    }

//    // GZComment: Expects the caller to provide an already allocated output buffer of appropriate size.
//    // Don't return anything, we WANT the caller to know that the operation will copy data to
//    // his array. The caller can grab output length from the OutputLength property
//    public void GetSpectrumDataSynched(float[] data)
//    {
//        if (data.Length < fftSize / 2)
//        {
//            Debug.LogWarning("provided array length should be at least " + (fftSize / 2).ToString());
//            return;
//        }

//        lock (_lockObject)
//        {
//            Array.Copy(fftOutput, data, fftSize / 2); //No need to copy the mirrored part
//        }
//    }

//    // Windowing
//    private float[] Hanning(float[] input)
//    {
//        for (int i = 0; i < input.Length; i++)
//        {
//            input[i] = (0.5f * (1.0f - Mathf.Cos((2.0f * Mathf.PI * i) / fftSize)));
//        }
//        return input;
//    }

//    public class FloatFFT
//    {
//        /// Here goes the whole fft class
//        /// 
//        /// 


//        // Element for linked list in which we store the
//        // input/output data. We use a linked list because
//        // for sequential access it's faster than array index.
//        class FFTElement
//        {
//            public double re = 0.0;     // Real component
//            public double im = 0.0;     // Imaginary component
//            public FFTElement next;     // Next element in linked list
//            public uint revTgt;         // Target position post bit-reversal
//        }

//        private uint m_logN = 0;        // log2 of FFT size
//        private uint m_N = 0;           // FFT size
//        private FFTElement[] m_X;       // Vector of linked list elements

//        /**
//     *
//     */
//        public FloatFFT()
//        {
//        }

//        /**
//     * Initialize class to perform FFT of specified size.
//     *
//     * @param   logN    Log2 of FFT length. e.g. for 512 pt FFT, logN = 9.
//     */
//        public void init(
//            uint logN)
//        {
//            m_logN = logN;
//            m_N = (uint)(1 << (int)m_logN);

//            // Allocate elements for linked list of complex numbers.
//            m_X = new FFTElement[m_N];
//            for (uint k = 0; k < m_N; k++)
//                m_X[k] = new FFTElement();

//            // Set up "next" pointers.
//            for (uint k = 0; k < m_N - 1; k++)
//                m_X[k].next = m_X[k + 1];

//            // Specify target for bit reversal re-ordering.
//            for (uint k = 0; k < m_N; k++)
//                m_X[k].revTgt = BitReverse(k, logN);
//        }

//        /**
//     * Performs in-place complex FFT.
//     *
//     * @param   xRe     Real part of input/output
//     * @param   xIm     Imaginary part of input/output
//     * @param   inverse If true, do an inverse FFT
//     */
//        public void run(
//            float[] xRe,
//            float[] xIm,
//            bool inverse = false)
//        {
//            uint numFlies = m_N >> 1; // Number of butterflies per sub-FFT
//            uint span = m_N >> 1;     // Width of the butterfly
//            uint spacing = m_N;         // Distance between start of sub-FFTs
//            uint wIndexStep = 1;        // Increment for twiddle table index

//            // Copy data into linked complex number objects
//            // If it's an IFFT, we divide by N while we're at it
//            FFTElement x = m_X[0];
//            uint k = 0;
//            double scale = inverse ? 1.0 / m_N : 1.0;
//            while (x != null)
//            {
//                x.re = scale * xRe[k];
//                x.im = scale * xIm[k];
//                x = x.next;
//                k++;
//            }

//            // For each stage of the FFT
//            for (uint stage = 0; stage < m_logN; stage++)
//            {
//                // Compute a multiplier factor for the "twiddle factors".
//                // The twiddle factors are complex unit vectors spaced at
//                // regular angular intervals. The angle by which the twiddle
//                // factor advances depends on the FFT stage. In many FFT
//                // implementations the twiddle factors are cached, but because
//                // array lookup is relatively slow in C#, it's just
//                // as fast to compute them on the fly.
//                double wAngleInc = wIndexStep * 2.0 * Math.PI / m_N;
//                if (inverse == false)
//                    wAngleInc *= -1;
//                double wMulRe = Math.Cos(wAngleInc);
//                double wMulIm = Math.Sin(wAngleInc);

//                for (uint start = 0; start < m_N; start += spacing)
//                {
//                    FFTElement xTop = m_X[start];
//                    FFTElement xBot = m_X[start + span];

//                    double wRe = 1.0;
//                    double wIm = 0.0;

//                    // For each butterfly in this stage
//                    for (uint flyCount = 0; flyCount < numFlies; ++flyCount)
//                    {
//                        // Get the top & bottom values
//                        double xTopRe = xTop.re;
//                        double xTopIm = xTop.im;
//                        double xBotRe = xBot.re;
//                        double xBotIm = xBot.im;

//                        // Top branch of butterfly has addition
//                        xTop.re = xTopRe + xBotRe;
//                        xTop.im = xTopIm + xBotIm;

//                        // Bottom branch of butterly has subtraction,
//                        // followed by multiplication by twiddle factor
//                        xBotRe = xTopRe - xBotRe;
//                        xBotIm = xTopIm - xBotIm;
//                        xBot.re = xBotRe * wRe - xBotIm * wIm;
//                        xBot.im = xBotRe * wIm + xBotIm * wRe;

//                        // Advance butterfly to next top & bottom positions
//                        xTop = xTop.next;
//                        xBot = xBot.next;

//                        // Update the twiddle factor, via complex multiply
//                        // by unit vector with the appropriate angle
//                        // (wRe + j wIm) = (wRe + j wIm) x (wMulRe + j wMulIm)
//                        double tRe = wRe;
//                        wRe = wRe * wMulRe - wIm * wMulIm;
//                        wIm = tRe * wMulIm + wIm * wMulRe;
//                    }
//                }

//                numFlies >>= 1;   // Divide by 2 by right shift
//                span >>= 1;
//                spacing >>= 1;
//                wIndexStep <<= 1;     // Multiply by 2 by left shift
//            }

//            // The algorithm leaves the result in a scrambled order.
//            // Unscramble while copying values from the complex
//            // linked list elements back to the input/output vectors.
//            x = m_X[0];
//            while (x != null)
//            {
//                uint target = x.revTgt;
//                xRe[target] = (float)x.re;
//                xIm[target] = (float)x.im;
//                x = x.next;
//            }
//        }

//        /**
//     * Do bit reversal of specified number of places of an int
//     * For example, 1101 bit-reversed is 1011
//     *
//     * @param   x       Number to be bit-reverse.
//     * @param   numBits Number of bits in the number.
//     */
//        private uint BitReverse(
//            uint x,
//            uint numBits)
//        {
//            uint y = 0;
//            for (uint i = 0; i < numBits; i++)
//            {
//                y <<= 1;
//                y |= x & 0x0001;
//                x >>= 1;
//            }
//            return y;
//        }
//    }



//    public float[] data;

//   // Use this for initialization

//        public float speed = 10f;
//    void Start()
//    {

//        audio
//        data = new float[OutputLength];
//    }

//    // Update is called once per frame


//    void Update()
//    {
//        GetSpectrumDataSynched(data);//transform.Rotate(Vector3.up, speed * Time.deltaTime);
//    }

//}




using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class fftAnalyzer : MonoBehaviour
{
    private int audioSampleRate = 44100;
    public string microphone;
    public FFTWindow fftWindow;
    private int samples = 1024;
    private AudioSource audioSource;

    public float sensitivity = 100;
    public float loudness = 0;
    [Range(0, 64)]
    public int bins;
    float Resolution;
    public float[] spectrum;
    public float[] spectrumBinned;
    [Range(0, 1)]
    public float lerpSpeed;

    public bool maxMode;

    void Start()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        UpdateMicrophone();

        spectrum = new float[samples];
    }

    void UpdateMicrophone()
    {
        audioSource.Stop();
        //Start recording to audioclip from the mic
        audioSource.clip = Microphone.Start(microphone, true, 10, audioSampleRate);
        audioSource.loop = true;
        //Mute the sound with an Audio Mixer
        Debug.Log(Microphone.IsRecording(microphone).ToString());

        if (Microphone.IsRecording(microphone))
        {
            while (!(Microphone.GetPosition(microphone) > 0))
            {
                //Wait until the Recording has started
            }
            Debug.Log("recording started with " + microphone);

            //Start playing the audio source
            audioSource.Play();
        }
        else
        {
            Debug.Log(microphone + "doesn't work!");
        }
    }

    void Update()
    {
        audioSource.GetSpectrumData(spectrum, 0, fftWindow);
        if (bins > 0)
        {
            spectrumBinned = new float[bins];
            int binsPerGroup = spectrum.Length / bins;
            for (int i = 0; i < bins; i++)
            {
                spectrumBinned[i] = 0.0f;
                for (int j = binsPerGroup * i; j < (i + 1) * binsPerGroup; j++)
                {
                    if (maxMode)
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
        }
        else
        {
            for (int i = 0; i < spectrum.Length; i++)
            {
                float x = i / 10.0f;
                Debug.DrawLine(new Vector3(x, 0, 0), new Vector3(x, spectrum[i] * 100, 0));
            }
        }

    }
}