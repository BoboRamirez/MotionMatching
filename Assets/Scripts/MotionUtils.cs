using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static MathUtils;

public class MotionUtils : MonoBehaviour
{
    public TextAsset SourceBVH;
    public GameObject EGCude;
    [SerializeField]int frame = 0;
    [SerializeField]int frameEnd = int.MaxValue;
    private Skeleton bvhSk;
    private List<GameObject> cubes = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        string[] lines = SourceBVH.text.Split('\n');
        //Debug.Log(lines.Length);
        //ResetRot(transform);
        //Debug.Log("Hello");

        bvhSk = new Skeleton(lines);
        Application.targetFrameRate = (int) (1.0f / bvhSk.frameTime);
        for (int i = 0; i < bvhSk.jointCount; i++)
        {
            cubes.Add(Instantiate(EGCude, transform));
            //Debug.Log(i.ToString() + ": " + bvhSk.joints[i].name);
        }
        SetRot(transform);
    }
    private void Update()
    {
        if (frame >= bvhSk.frameCount || frame >= frameEnd)
            return;
        bvhSk.ForwardKinematicsByFrame(frame);
        for (int i = 0; i < bvhSk.jointCount; i++)
        {
            cubes[i].transform.SetPositionAndRotation(bvhSk.joints[i].location[frame], bvhSk.joints[i].orientation[frame]);
        }
        frame++;

    }
    private void SetRot(Transform t)
    {
        //Debug.Log("name: " + t.name);
        t.rotation = Quaternion.identity;
        for (int i = 0; i < t.childCount; i++)
        {
            SetRot(t.GetChild(i));
        }
    }
    private void CreateTestSkeleton(Skeleton sk, int frame = 0)
    {
        foreach(Joint j in sk.joints)
        {
            Instantiate(EGCude, j.location[frame], j.orientation[frame], transform);
        }
        
    }
}

