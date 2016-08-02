using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RockGroupController : MonoBehaviour
{
    List<JumpingRock> rocks = new List<JumpingRock>();

    public float lerpSpeedUp = 0.6f;
    public float lerpSpeedDown = 0.05f;
    public float scale = 1;

    void Start()
    {
        var rockMeshes = gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var rockMesh in rockMeshes)
        {
            var jumpingRock = rockMesh.gameObject.AddComponent<JumpingRock>();
            rocks.Add(jumpingRock);
        }
    }


    void Update()
    {
        foreach (var rock in rocks)
        {
            rock.lerpSpeedUp = lerpSpeedUp;
            rock.lerpSpeedDown = lerpSpeedDown;
            rock.scale = scale;
        }
    }
}