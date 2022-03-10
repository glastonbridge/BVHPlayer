using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class SkeletonAnimator : MonoBehaviour
{
  public string BvhName;
  public int framedivider = 1;
  public float scaleoffset = 0.1f;

  [Header("BVH debugging")]
  public int debugFrame;

  [Header("Bone debugging")]
  public string debugBone;
  public float debugBoneX;
  public float debugBoneY;
  public float debugBoneZ;

  private Dictionary<string, Quaternion> initial;
  private Vector3 initialPos, initialOffset;
  private VideoPlayer vid;
  private Dictionary<string, string> boneMappings;
  private BVHPoser bvhPoser;

  void Start()
  {
    // This maps bone names in BVH to bones in the model. For CMU BVH it should be boneMappingsDirect, for MocapNET BVH it will be boneMappingsBVH
    // For anything else you can figure it out yourself by looking at the skeleton hierarchies in the BVH file and in your unity model tree.
    boneMappings = boneMappingsBVH;
    
    bvhPoser = new BVHPoser(BvhName);
    Frame = 0;
    initial = new Dictionary<string, Quaternion>();
    getInitialRotations(transform, bvhPoser.Skeleton);
    initialPos = transform.localPosition;
    initialOffset = bvhPoser.getOffsetForFrame(0);
  }

  /*
   * The bvh frame that is currently being exposed.
   */
  public int Frame { get; private set; }

  void Update()
  {
    BVHFrame frame;

    bvhPoser.debugBone = debugBone;
    bvhPoser.debugBoneX = debugBoneX;
    bvhPoser.debugBoneY = debugBoneY;
    bvhPoser.debugBoneZ = debugBoneZ;

    int localFrame = debugFrame > 0 ? debugFrame - 1 : (int)((Frame) / framedivider) % bvhPoser.FrameCount;

    frame = bvhPoser.getRotationsForFrame(localFrame);
    if (vid != null)
    {
      vid.frame = localFrame;
    }
    applySkeleton(transform, bvhPoser.Skeleton, frame);
    transform.localPosition = Vector3.Scale(initialPos - initialOffset + bvhPoser.getOffsetForFrame(localFrame), new Vector3(scaleoffset,scaleoffset,scaleoffset));
    ++Frame;
  }

  /**
   * Skel is pre-rotatated into global space by the poser
   */
  private void applySkeleton(Transform targetSkeleton, BVHSkeleton skel, BVHFrame frame)
  {
    if (boneMappings.ContainsKey(skel.name))
    {
      Transform target = targetSkeleton;
      string targetName = boneMappings[skel.name];
      if (targetName != null)
      {
        target = findRecursively(targetSkeleton, targetName);
        try
        {
          target.transform.rotation = JointRotator.bvhRotToUnityRot(frame.joints[skel.name].rot) * initial[skel.name];
        }
        catch (KeyNotFoundException)
        {
          Debug.Log("Couldn't find " + targetName);
        }
      }
      if (target == null)
      {
        target = targetSkeleton;
      }
      if (skel.joints != null) foreach (KeyValuePair<string, BVHSkeleton> skel2 in skel.joints)
        {
          applySkeleton(target, skel2.Value, frame);
        }
    }
  }


  private Transform findRecursively(Transform targetSkeleton, string subj)
  {
    Transform target = targetSkeleton.Find(subj);

    if (target == null)
    {
      foreach (Transform subSkeleton in targetSkeleton)
      {
        target = findRecursively(subSkeleton, subj);
        if (target != null) break;
      }
    }

    return target;
  }

  private void getInitialRotations(Transform targetSkeleton, BVHSkeleton skel)
  {
    Transform joint = null;
    if (boneMappings.ContainsKey(skel.name))
    {
      string targetName = boneMappings[skel.name];
      if (targetName != null)
      {
        joint = findRecursively(targetSkeleton, targetName);
        initial[skel.name] = joint.rotation;
      }
    }
    if (joint == null)
    {
      joint = targetSkeleton;
    }
    if (skel != null && skel.joints != null) foreach (BVHSkeleton child in skel.joints.Values)
      {
        getInitialRotations(joint, child);
      }
  }

  private Dictionary<string, HumanBodyBones> boneMappingsAvatar = new Dictionary<string, HumanBodyBones>()
    {
        { "hip", HumanBodyBones.Hips },
        { "abdomen", HumanBodyBones.Spine },
        { "chest", HumanBodyBones.Chest },
        { "neck", HumanBodyBones.Neck },
        { "head", HumanBodyBones.Head },
        { "lCollar", HumanBodyBones.LeftShoulder},
        { "lShldr", HumanBodyBones.LeftUpperArm },
        { "lForeArm", HumanBodyBones.LeftLowerArm},
        { "lHand", HumanBodyBones.LeftHand },
        { "rCollar", HumanBodyBones.RightShoulder},
        { "rShldr", HumanBodyBones.RightUpperArm },
        { "rForeArm",HumanBodyBones.RightLowerArm},
        { "rHand", HumanBodyBones.RightHand },

        { "lThigh", HumanBodyBones.LeftUpperLeg },
        { "lShin", HumanBodyBones.LeftLowerLeg },
        { "lFoot", HumanBodyBones.LeftFoot },
        { "rThigh", HumanBodyBones.RightUpperLeg },
        { "rShin", HumanBodyBones.RightLowerLeg },
        { "rFoot", HumanBodyBones.RightFoot }
    };

  private Dictionary<string, string> boneMappingsDirect = new Dictionary<string, string>()
    {
        { "Hips", "Hips" },
        { "Spine", "Spine" },
        { "Spine1", null },
        { "Neck", "Neck" },
        { "Neck1", "Neck1" },
        { "Head", "Head" },
        { "LeftShoulder", "LeftShoulder"},
        { "LeftArm", "LeftArm" },
        { "LeftForeArm", "LeftForeArm"},
        { "LeftHand", "LeftHand" },
        { "RightShoulder", "RightShoulder"},
        { "RightArm", "RightArm" },
        { "RightForeArm", "RightForeArm"},
        { "RightHand", "RightHand" },

        { "LHipJoint", "LHipJoint" },
        { "LeftUpLeg", "LeftUpLeg" },
        { "LeftLeg", "LeftLeg" },
        { "LeftFoot", "LeftFoot" },
        { "RHipJoint", "RHipJoint" },
        { "RightUpLeg", "RightUpLeg" },
        { "RightLeg", "RightLeg" },
        { "RightFoot", "RightFoot" }

    };

  private Dictionary<string, string> boneMappingsBVH = new Dictionary<string, string>()
    {
        { "hip", "Hips" },
        { "abdomen", "LowerBack" },
        { "chest", "Spine" },
        { "neck", "Neck" },
        { "neck1", "Neck1" },
        { "head", "Head" },
        { "lCollar", "LeftShoulder"},
        { "lShldr", "LeftArm" },
        { "lForeArm", "LeftForeArm"},
        { "lHand", "LeftHand" },
        { "rCollar", "RightShoulder"},
        { "rShldr", "RightArm" },
        { "rForeArm", "RightForeArm"},
        { "rHand", "RightHand" },

        { "lButtock", "LHipJoint" },
        { "lThigh", "LeftUpLeg" },
        { "lShin", "LeftLeg" },
        { "lFoot", "LeftFoot" },
        { "rButtock", "RHipJoint" },
        { "rThigh", "RightUpLeg" },
        { "rShin", "RightLeg" },
        { "rFoot", "RightFoot" }
    };

}