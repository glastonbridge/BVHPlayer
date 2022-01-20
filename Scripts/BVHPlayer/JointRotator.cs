using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class JointRotator : ScriptableObject
{
    // Via https://github.com/emilianavt/BVHTools/blob/master/Plugins/BVH%20Tools/Scripts/BVHAnimationLoader.cs
    // BVH to Unity
    public static Quaternion getUnityQuat(Vector3 s)
    {
        return Quaternion.AngleAxis(s.z, Vector3.forward) * Quaternion.AngleAxis(s.x, Vector3.right) * Quaternion.AngleAxis(s.y, Vector3.up);
    }
  
    public static Vector3 jointToSpace(string jointName, Vector3 rot)
    {
        float[] mf = new float[]
        {
            1, 0, 0,
            0, -1, 0,
            0, 0, -1
        };

        return new Vector3(
            rot.x * mf[0] + rot.y * mf[1] + rot.z * mf[2],
            rot.x * mf[3] + rot.y * mf[4] + rot.z * mf[5],
            rot.x * mf[6] + rot.y * mf[7] + rot.z * mf[8]);
    }
}