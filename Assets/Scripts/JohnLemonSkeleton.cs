using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JohnLemonSkeleton : MonoBehaviour
{
    public TextAsset SourceBVH;

    public int jointCount = 0;
    public List<Vector3> offsets;
    public List<Quaternion> offsetAngles;
    public List<Quaternion> orientations;
    private Dictionary<int, int> JLToLafan;
    private Dictionary<int, int> jointChild;
    private Skeleton lafan;
    public int frame = 0, endFrame = 7135;
    private int jointIndex;
    [SerializeField] private List<float> twistAngles;
    // Start is called before the first frame update
    void Start()
    {
        string[] lines = SourceBVH.text.Split('\n');
        lafan = new Skeleton(lines);
        Application.targetFrameRate = (int)(1.0f / lafan.frameTime);
        JLToLafan = new Dictionary<int, int>()
        {
            {0, 0},{1, 1}, {2, 2}, {3, 3}, {4, 4 }, {5, 5}, {6, 6}, {7, 7},
            {8, 8}, {9, 9}, {10, 11}, {11, 14}, {12, 15}, {13, 16}, {14, 17}, 
            {15, 12}, {16, 13}, {17, 18}, {18, 19}, {19, 20}, {20, 21}
        };
        jointChild = new Dictionary<int, int>()
        {
            {0, 9}, {1, 2}, {2, 3}, {3, 4}, {4, -1}, {5, 6}, {6, 7}, 
            {7, 8}, {8, -1}, {9, 10}, {10, 15 }, {11, 12}, {12, 13}, 
            {13, 14}, {14, -1 }, {15, 16}, {16, -1}, {17, 18}, {18, 19},
            {19, 20}, {20, -1 }
        };
        jointIndex = 0;
        ResetRot(transform);
        twistAngles = new List<float> { -90, -90, -90, -90, 0, -90, -90,
            -90, -90, -90, -90, 90, 90, 90, 90, -90, -90, -90, 90, 90, 90 };
        offsets = new List<Vector3>();
        offsetAngles = new List<Quaternion>();
        orientations = new List<Quaternion>();
        BuildSkeleton(transform.GetChild(0).gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (frame >= endFrame || frame >= lafan.frameCount) return;
        ActivateJLByFrame(frame);
        frame++;
    }

    private void BuildSkeleton(GameObject bone)
    {
        Debug.Log(jointCount.ToString() + ": " + bone.name);
        offsets.Add(bone.transform.parent.rotation * bone.transform.localPosition);
        jointCount++;
        if (bone.name.EndsWith("End") || bone.name.EndsWith("Head") || bone.name.EndsWith("Hand"))
            return;
        for (int i = 0; i < bone.transform.childCount; i++)
        {
            BuildSkeleton(bone.transform.GetChild(i).gameObject);
        }
    }
    private void ResetRot(Transform t)
    {
        //Debug.Log("name: " + t.name);
        t.localRotation = Quaternion.identity;
        for (int i = 0; i < t.childCount; i++)
        {
            ResetRot(t.GetChild(i));
        }
    }
    private void SetRot(Transform t)
    {
        //Debug.Log($"name: {t.name}, index: {jointIndex}, rotation: {orientations[jointIndex].eulerAngles}");
        t.rotation = orientations[jointIndex];
        jointIndex++;
        if (t.name.EndsWith("End") || t.name.EndsWith("Head") || t.name.EndsWith("Hand"))
            return;
        for (int i = 0; i < t.childCount; i++)
        {
            SetRot(t.GetChild(i));
        }
    }
    private void ActivateJLByFrame(int frame)
    {
        int l;
        Vector3 lafanOffset;
        lafan.ForwardKinematicsByFrame(frame);
        orientations.Clear();
        offsetAngles.Clear();
        Quaternion qSwing, qTwist, q = new();
        for (int i = 0; i < jointCount; i++)
        {
            l = JLToLafan[i];
            if (l != 9)
                lafanOffset = new Vector3(-lafan.joints[l].offset[0], lafan.joints[l].offset[1], lafan.joints[l].offset[2]);
            else
                lafanOffset = new Vector3(-lafan.joints[9].offset[0], lafan.joints[9].offset[1], lafan.joints[9].offset[2])
                            + new Vector3(-lafan.joints[10].offset[0], lafan.joints[10].offset[1], lafan.joints[10].offset[2]);
            /*Debug.Log($"joint {i}:");
            Debug.Log(offsets[i].normalized);
            Debug.Log(lafanOffset.normalized);*/
            q.SetFromToRotation(offsets[i].normalized, lafanOffset.normalized);
            offsetAngles.Add(q);
            //Debug.Log(q * offsets[i].normalized - lafanOffset.normalized);
            //transform.rotation = lafan.joints[l].orientation[frame] * q;
        }
        for (int i = 0; i < jointCount; i++)
        {
            qSwing = jointChild[i] == -1 ? offsetAngles[i] : offsetAngles[jointChild[i]];
            qTwist = Quaternion.AngleAxis(twistAngles[i], Vector3.right);
            orientations.Add(lafan.joints[JLToLafan[i]].orientation[frame] * qTwist * qSwing);
        }

        jointIndex = 0;
        SetRot(transform.GetChild(0));
    }
}
