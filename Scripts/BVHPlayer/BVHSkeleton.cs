using System.Collections;
using System.Collections.Generic;
using System.Numerics;

public class BVHSkeleton
{
    public float[] offsets;
    public string[] channels;
    public string name;
    private Dictionary<string, BVHSkeleton> _joints;

    public Dictionary<string, BVHSkeleton> joints {
        get => _joints;
    }

    public void setJoints(List<BVHSkeleton> joints)
    {
        _joints = new Dictionary<string, BVHSkeleton>();
        foreach (BVHSkeleton j in joints)
        {
            _joints.Add(j.name, j);
        }
    }
}
