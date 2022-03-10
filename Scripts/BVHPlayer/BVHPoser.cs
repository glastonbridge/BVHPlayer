using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

/**
 * This makes the BVH rotations global for a skeleton at a given frame. It does not consider anything to do with Unity models.
 * 
 */
public class BVHPoser
{
  private List<BVHFrame> frames;
  private const string ROOT_JOINT = "hip";
  public string debugBone;
  public float debugBoneX;
  public float debugBoneY;
  public float debugBoneZ;

  public BVHPoser(string bvhName)
  {
    ParseBVH parser = new ParseBVH(new StreamReader(bvhName));
    parser.parse();
    Skeleton = parser.skeleton;
    frames = parser.frames;
  }

  public BVHSkeleton Skeleton { get; private set; }
  
  public int FrameCount
  {
    get { return frames.Count; }
  }

  public Vector3 getOffsetForFrame(int frame)
  {
    // Throws InvalidOperationException if there is no root offset (desirable).
    return (Vector3) frames[frame].joints[ROOT_JOINT].offset;
  }

  public BVHFrame getRotationsForFrame(int frame)
  {
    BVHFrame results = new BVHFrame();
    recurseJointRotations(Skeleton, frames[frame], Quaternion.identity, results);
    return results;
  }

  private void recurseJointRotations(BVHSkeleton skel, BVHFrame frame, Quaternion parentRot, BVHFrame results)
  {
    if (skel.name == "End Site") return;
    BVHFrame.Joint jointLocal = frame.joints[skel.name];
    Quaternion localRot = jointLocal.rot;
    if (isBoneDebugging()) // override for testing
    {
      bool isTargetBone = skel.name == debugBone;
      float zr = isTargetBone ? (float)debugBoneZ : 0;
      float yr = isTargetBone ? (float)debugBoneY : 0;
      float xr = isTargetBone ? (float)debugBoneX : 0;
      localRot = JointRotator.bvhVectorToQuaternion(new Vector3(xr, yr, zr));
    }
    Quaternion currentRot = localRot*parentRot;
    BVHFrame.Joint jointWorld = new BVHFrame.Joint(jointLocal.offset, currentRot);
    results.joints.Add(skel.name, jointWorld);

    if (skel.joints != null) foreach (KeyValuePair<string, BVHSkeleton> skel2 in skel.joints)
    {
      recurseJointRotations(skel2.Value, frame, currentRot, results);
    }
  }

  private bool isBoneDebugging()
  {
    return debugBone != null && !debugBone.Equals("");
  }
}