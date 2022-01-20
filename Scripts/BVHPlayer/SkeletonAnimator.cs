using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class SkeletonAnimator : MonoBehaviour
{
  public string BvhName;
  public int framedivider = 1;

  [Header("BVH debugging")]
  public int debugFrame;

  private Transform targetSkeleton;
  private BVHSkeleton skeleton;
  private List<BVHFrame> frames;
  private Dictionary<string, Quaternion> initial;
  private Vector3 initialPos, initialOffset;
  private VideoPlayer vid;
  private Dictionary<string, string> boneMappings;

  void Start()
  {
    // This maps bone names in BVH to bones in the model. For CMU BVH it should be boneMappingsDirect, for MocapNET BVH it will be boneMappingsBVH
    // For anything else you can figure it out yourself by looking at the skeleton hierarchies in the BVH file and in your unity model tree.
    boneMappings = boneMappingsDirect;
    
    ParseBVH parser = new ParseBVH(new StreamReader(BvhName));
    parser.parse();
    skeleton = parser.skeleton;
    targetSkeleton = transform;
    frames = parser.frames;
    Frame = 0;
    initial = new Dictionary<string, Quaternion>();
    getInitialRotations(targetSkeleton, skeleton);
    initialPos = new Vector3(
        transform.localPosition.x,
        transform.localPosition.y,
        transform.localPosition.z);
    initialOffset = (Vector3)frames[0].joints[skeleton.name].offset;

  }

  /*
   * The bvh frame that is currently being exposed.
   */
  public int Frame { get; private set; }

  void Update()
  {
    BVHFrame frame;
    Frame = debugFrame > 0 ? debugFrame : ((Frame + 1) / framedivider) % frames.Count;

    frame = frames[Frame];
    if (vid != null)
    {
      vid.frame = Frame;
    }

    applySkeleton(targetSkeleton, skeleton, frame, new Vector3(0, 0, 0));
  }

  private void applySkeleton(Transform targetSkeleton, BVHSkeleton skel, BVHFrame frame, Vector3 parentRot)
  {
    if (boneMappings.ContainsKey(skel.name))
    {
      Transform target = targetSkeleton;
      Vector3 currentRot = parentRot;
      string targetName = boneMappings[skel.name];
      if (targetName != null)
      {
        target = findRecursively(targetSkeleton, targetName);
        try
        {
          Quaternion rot = frame.joints[skel.name].rot;
          currentRot = parentRot + rot.eulerAngles;
          target.transform.eulerAngles = initial[skel.name].eulerAngles + currentRot;

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
          applySkeleton(target, skel2.Value, frame, currentRot);
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