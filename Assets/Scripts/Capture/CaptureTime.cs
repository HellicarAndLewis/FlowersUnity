using UnityEngine;
using System.Collections;

public class CaptureTime : MonoBehaviour
{

    static float elapsed = 0;
    static float delta = 0;
    public static bool IsCapturing = false;

    public static float Delta
    {
        get
        {
            if (IsCapturing) return delta;
            else return Time.deltaTime;
        }
        set { delta = value; }
    }

    public static float Elapsed
    {
        get
        {
            if (IsCapturing) return elapsed;
            else return Time.fixedTime;
        }
        set { elapsed = value; }
    }

}