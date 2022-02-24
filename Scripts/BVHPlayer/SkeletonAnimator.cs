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

  [Header("Bone debugging")]
  public string debugBone;
  public float debugBoneX;
  public float debugBoneY;
  public float debugBoneZ;

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
    boneMappings = boneMappingsBVH;
    
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


    if (isBoneDebugging())
    {
      // TODO:
      // iterate over entire skeleton, setting values to 0
      // if bone = debugBone then set the rotations as one would in BVHSkeleton
      frame = new BVHFrame();
      Frame = 0;
      generateDebugFrameRecursively(skeleton, frame);
    }
    else
    {

      int localFrame = debugFrame > 0 ? debugFrame - 1 : (int)((Frame) / framedivider) % frames.Count;

      frame = frames[localFrame];
      if (vid != null)
      {
        vid.frame = localFrame;
      }

    }
      applySkeleton(targetSkeleton, skeleton, frame, Quaternion.identity);
      ++Frame;
  }

  private void applySkeleton(Transform targetSkeleton, BVHSkeleton skel, BVHFrame frame, Quaternion parentRot)
  {
    if (boneMappings.ContainsKey(skel.name))
    {
      Transform target = targetSkeleton;
      Quaternion currentRot = parentRot;
      string targetName = boneMappings[skel.name];
      if (targetName != null)
      {
        target = findRecursively(targetSkeleton, targetName);
        try
        {
          /** 
           * Oh gods this is driving me nuts
           * 
           * there is the t-pose reference frame, which is what the BVH uses, everything is 0 at t-pose and has the same axes
           * there are child rotation frames, which are the t-pose rotations, applied recursively. the child rotation is applied, followed by
           * the parent.
           * 
           * 
           * The rotation has to happen in the t-pose reference frame
           * But it then has to be rotated by the parent rotation
           * The parent rotation that it passes to children must then be 
           *   * applicable in the t-pose reference frame
           *   * not be clouded by the initial value we used to map onto the Unity model
           *   
           * Anything that does not include an initial, can be considered "pure bvh" and we only apply its rotation to world space
           * at the last minute, by combining with initial values
           * 
           */
          currentRot = frame.joints[skel.name].rot * parentRot;

          target.transform.rotation = currentRot * initial[skel.name];
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
      if (parentRot == currentRot) Debug.Log("skel " + skel.name + " has identical parent rot");
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


  private bool isBoneDebugging()
  {
    return debugBone != null && !debugBone.Equals("");
  }

  private void generateDebugFrameRecursively(BVHSkeleton skel, BVHFrame currentFrame)
  {
    bool isTargetBone = skel.name == debugBone;
    if (skel.name == "End Site") return;
    float zr = isTargetBone ? (float)debugBoneZ : 0;
    float yr = isTargetBone ? (float)debugBoneY : 0;
    float xr = isTargetBone ? (float)debugBoneX : 0;
    Quaternion rot = JointRotator.getUnityQuat(JointRotator.jointToSpace(skel.name, new Vector3(xr, yr, zr)));
    if (skel.channels.Length == 6)
    {
      Vector3 offs = new Vector3(0, 0, 0);
      //Debug.Log("" + xr + "," + yr + "," + zr);
      currentFrame.joints[skel.name] = new BVHFrame.Joint(
          offs,
          rot
      );
    }
    else
    {
      currentFrame.joints[skel.name] = new BVHFrame.Joint(rot);
    }

    foreach (KeyValuePair<string, BVHSkeleton> kvp in skel.joints)
    {
      generateDebugFrameRecursively(kvp.Value, currentFrame);
    }
  }
}