using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MathUtils;

public class Skeleton
{
    public List<Joint> joints = new List<Joint>();
    public int jointCount = 0;
    public int frameCount = 0;
    public float frameTime = 1.0f;
    /// <summary>
    /// scale down to fit unity, used only in calculation, like FK, IK, etc.
    /// </summary>
    public float scale = 1.0f / 100.0f;
    /*    public Skeleton(GameObject skeleton)
        {
            names = new List<string>();
            positions = new List<Vector3>();
            rotations = new List<Quaternion>();
            SearchSkeleton(skeleton.transform);
            jointCount = names.Count;
        }
        private void SearchSkeleton(Transform cur)
        {
            names.Add(cur.name);
            positions.Add(cur.position);
            rotations.Add(cur.rotation);
            for (int i = 0; i < cur.childCount; i++)
            {
                SearchSkeleton(cur.GetChild(i));
            }
        }*/

    public Skeleton(string[] lines)
    {
        string[] data;
        string l;
        Stack<Joint> s = new Stack<Joint>();
        Joint curJoint = null;
        int i;
        //HIERARCHY
        for (i = 0; i < lines.Length; i++)
        {
            l = lines[i].Trim();
            if (l == "MOTION")
            {
                i++;
                break;
            }
            else if (l.Equals("End Site"))
            {
                i += 3;
            }
            else if (l.StartsWith("JOINT"))
            {
                if (s.Count == 0)
                    Debug.LogError("JOINT before ROOT?");
                curJoint = new Joint(l.Split(' ')[1], s.Peek());
                s.Peek().children.Add(curJoint);
                s.Peek().childCount++;
                joints.Add(curJoint);
                jointCount++;
            }
            else if (l.StartsWith("OFFSET"))
            {
                data = l.Split(' ');
                if (s.Count == 0)
                    Debug.LogError("stack empty while getting offset");
                s.Peek().offset = new float[] { float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]) };
            }
            else if (l.StartsWith("CHANNELS"))
            {
                if (s.Count == 0)
                    Debug.LogError("stack empty while getting channels");
                data = l.Split(' ');
                s.Peek().channelCount = int.Parse(data[1]);
                string str = "";
                for (int k = 0; k < s.Peek().channelCount; k++)
                    str += data[k + 2][..1];
                s.Peek().channelOrder = str;
            }
            else if (l.StartsWith("{"))
            {
                /*                if (curJoint == null)
                                    Debug.LogError("current joint not assgined before \"{\"");*/
                s.Push(curJoint);
            }
            else if (l.StartsWith("}"))
            {
                s.Pop();
            }
            else if (l.StartsWith("ROOT"))
            {
                curJoint = new Joint(l.Split(' ')[1]);
                joints.Add(curJoint);
                jointCount++;
            }
        }
        //MOTION
        while (i < lines.Length)
        {
            l = lines[i].Trim();
            data = l.Split(' ');
            if (!l.StartsWith('F') && l != "")
            {
                int j = 0, k = 0;
                while (k < joints.Count)
                {
                    //Debug.Log($"j: {j}, k: {k} in line: {i}");
                    joints[k].BuildChannels(data[j..(j + joints[k].channelCount)]);
                    j += joints[k].channelCount;
                    k++;
                }
            }
            else if (l.StartsWith("Frames:"))
            {
                frameCount = int.Parse(data[1]);
            }
            else if (l.StartsWith("Frame Time:"))
            {
                frameTime = float.Parse(data[2]);
            }
            else
            {
                Debug.Log("Unexepected line: " + l + $" -in line {i} while dealing with motion.");
            }
            i++;
        }
        /*float[] rot = joints[5].rotation[1];
        Debug.Log($"{rot[0]}, {rot[1]}, {rot[2]}");
        Debug.Log(ZYXToQuaternion(rot));*/

        int randomCount = joints[3].rotation.Count;
        Debug.Assert(randomCount == frameCount, $"frame count not match: supposed to be {frameCount}, got {randomCount}");
    }

    public void ForwardKinematicsByFrame(int frame)
    {
        Debug.Assert(frame < frameCount, "Exceeding frame number.");
        Joint j = joints[0];
        //handedness issue, reverse the x axis
        j.location[frame] = new Vector3(-j.position[frame][0], j.position[frame][1], j.position[frame][2]) * scale;
        j.orientation[frame] = BVHEulerToUnityQuatenion(j.rotation[frame], j.channelOrder);
        for (int i = 1; i < joints.Count; i++)
        {
            j = joints[i];
            j.location[frame] = j.parent.orientation[frame] * new Vector3(-j.offset[0], j.offset[1], j.offset[2]) * scale + j.parent.location[frame];
            j.orientation[frame] = j.parent.orientation[frame] * BVHEulerToUnityQuatenion(j.rotation[frame], j.channelOrder);
        }


    }
}

public class Joint
{
    public string name;
    public float[] offset;
    public int channelCount = 0;
    public string channelOrder = null;
    public int childCount;
    public List<Joint> children;
    public Joint parent;
    public List<float[]> position;
    public List<float[]> rotation;
    public List<Vector3> location;
    public List<Quaternion> orientation;

    public Joint(string name, Joint parent = null)
    {
        this.name = name;
        this.parent = parent;
        childCount = 0;
        children = new List<Joint>();
        position = new List<float[]>();
        rotation = new List<float[]>();
        location = new List<Vector3>();
        orientation = new List<Quaternion>();
    }
    public void BuildChannels(string[] data)
    {
        Debug.Assert(channelOrder != null && channelCount != 0, $"No channels found while building joint-{name}");
        if (channelCount == 3)
        {
            position = null;
            rotation.Add(new float[] { float.Parse(data[channelOrder[0] - 'X']), float.Parse(data[channelOrder[1] - 'X']),
                        float.Parse(data[channelOrder[2] - 'X']) });
            location.Add(Vector3.zero);
            orientation.Add(Quaternion.identity);
        }
        else if (channelCount == 6)
        {
            position.Add(new float[] { float.Parse(data[channelOrder[0] - 'X']), float.Parse(data[channelOrder[1] - 'X']),
                        float.Parse(data[channelOrder[2] - 'X']) });
            rotation.Add(new float[] { float.Parse(data[channelOrder[3] - 'U']), float.Parse(data[channelOrder[4] - 'U']),
                        float.Parse(data[channelOrder[5] - 'U']) });
            location.Add(Vector3.zero);
            orientation.Add(Quaternion.identity);
        }
        else
        {
            Debug.Assert(false,
                $"Unexpected channel count while building joint-{name}: found {channelCount} channels, expect 3 or 6.");
        }
    }
}
