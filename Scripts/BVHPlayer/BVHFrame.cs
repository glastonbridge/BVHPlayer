using System.Collections.Generic;
using UnityEngine;

public class BVHFrame 
{
    public class Joint
    {
        public Quaternion rot;
        public Vector3? offset;
        public Joint(Vector3 offset, Quaternion rot)
        {
            this.rot = rot;
            this.offset = offset;
        }
        public Joint(Quaternion rot)
        {
            this.rot = rot;
            this.offset = null;
        }
    }
    public Dictionary<string, Joint> joints = new Dictionary<string, Joint>();
}
