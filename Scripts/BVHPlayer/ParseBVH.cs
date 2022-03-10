using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ParseBVH
{
  public BVHSkeleton skeleton;
  public List<BVHFrame> frames;

  private int scopes;
  private Tokeniser tokeniser;

  public ParseBVH(StreamReader data)
    {
    tokeniser = new Tokeniser(data);
        scopes = 0;
        skeleton = new BVHSkeleton();
        frames = new List<BVHFrame>();
    }

    public void parse()
    {
        parseSkeleton();
        parseFrames();
    }

    private void parseSkeleton()
    {
        tokeniser.consumeToken("HIERARCHY");
        parseRoot();
    }

    private void parseRoot()
    {
        BVHSkeleton root = skeleton;
        tokeniser.consumeToken("ROOT ");
        root.name = tokeniser.consumeString();
        consumeScopeIn();
        root.offsets = consumeOffsets();
        root.channels = consumeChannels();
        root.setJoints(consumeJoints());
        consumeScopeOut();
    }

    private void parseFrames()
    {
        tokeniser.consumeWhitespace();
        tokeniser.consumeToken("MOTION");
        tokeniser.consumeWhitespace();
        tokeniser.consumeToken("Frames: ");
        int numFrames = tokeniser.consumeInt();
        tokeniser.consumeWhitespace();
        tokeniser.consumeToken("Frame Time: ");
        float frameTime = tokeniser.consumeFloat();
        do
        {
            BVHFrame frame = new BVHFrame();

            try
            {

                parseFrameRecursively(skeleton, frame);
            }
            catch (ParseException pe)
            {
                Debug.LogError(pe.Message);
                Debug.LogError("On frame " + frames.Count + 1);
            }
            frames.Add(frame);
            tokeniser.expectToken("\n");
            tokeniser.consumeWhitespace();
        } while (!tokeniser.EndOfStream);
    }

    private void parseFrameRecursively(BVHSkeleton skel, BVHFrame currentFrame)
    {
        if (skel.name == "End Site") return;
        if (skel.channels.Length == 6)
        {
            Vector3 offs = new Vector3(tokeniser.consumeFloat(), tokeniser.consumeFloat(), tokeniser.consumeFloat());
            float zr = tokeniser.consumeFloat();
            float yr = tokeniser.consumeFloat();
            float xr = tokeniser.consumeFloat();
            Vector3 s = new Vector3(xr, yr, zr);
            currentFrame.joints[skel.name] = new BVHFrame.Joint(
                offs,
                JointRotator.bvhVectorToQuaternion(s)
            );
        }
        else currentFrame.joints[skel.name] = new BVHFrame.Joint(parseJointRotation(skel.name));

        foreach (KeyValuePair<string, BVHSkeleton> kvp in skel.joints)
        {
            parseFrameRecursively(kvp.Value, currentFrame);
        }
    }

    private Quaternion parseJointRotation(string name)
    {
        float z = tokeniser.consumeFloat();
        float x = tokeniser.consumeFloat();
        float y = tokeniser.consumeFloat();

        Vector3 s = new Vector3(x, y, z);

        return JointRotator.bvhVectorToQuaternion(s);
        
    }

    private List<BVHSkeleton> consumeJoints()
    {
        List<BVHSkeleton> joints = new List<BVHSkeleton>();
        tokeniser.consumeWhitespace();
        while (tokeniser.expectToken("JOINT"))
        {
            joints.Add(consumeJoint());
        }
        if (tokeniser.expectToken("End Site"))
        {
            joints.Add(consumeEndSite());
        }
        return joints;
    }

    private BVHSkeleton consumeJoint()
    {
        BVHSkeleton joint = new BVHSkeleton();
        tokeniser.consumeToken("JOINT ");
        joint.name = tokeniser.consumeString();
        consumeScopeIn();
        joint.offsets = consumeOffsets();
        joint.channels = consumeChannels();
        joint.setJoints(consumeJoints());
        consumeScopeOut();
        return joint;
    }

    private BVHSkeleton consumeEndSite()
    {
        BVHSkeleton joint = new BVHSkeleton();
        tokeniser.consumeToken("End Site");
        joint.name = "End Site";
        consumeScopeIn();
        joint.offsets = consumeOffsets();
        consumeScopeOut();
        return joint;
    }

    private float[] consumeOffsets()
    {
        tokeniser.consumeWhitespace();
        tokeniser.consumeToken("OFFSET ");
        return new[] { tokeniser.consumeFloat(), tokeniser.consumeFloat(), tokeniser.consumeFloat() };
    }

    private string[] consumeChannels()
    {
        tokeniser.consumeWhitespace();
        tokeniser.consumeToken("CHANNELS ");
        int numChannels = tokeniser.consumeInt();
        string[] channels = new string[numChannels];
        for (int i = 0; i < numChannels; ++i)
        {
            channels[i] = tokeniser.consumeString();
        }
        return channels;
    }

    private void consumeScopeIn()
    {
        tokeniser.consumeWhitespace();
        tokeniser.consumeToken("{");
        ++scopes;
        tokeniser.consumeWhitespace();
    }

    private void consumeScopeOut()
    {
        tokeniser.consumeWhitespace();
        tokeniser.consumeToken("}");
        --scopes;
        tokeniser.consumeWhitespace();
    }

}
