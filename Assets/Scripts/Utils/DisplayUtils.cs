using UnityEngine;
using System.Collections;

class DisplayUtils
{
    public static Vector2 GetFrustumSizeAtZ(float z, Camera camera)
    {
        float distance = Mathf.Abs(camera.transform.position.z) - z;
        var frustumHeight = 2.0f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumHeight * camera.aspect;
        return new Vector2(frustumWidth, frustumHeight);
    }
}