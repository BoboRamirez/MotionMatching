using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MathUtils
{
    /// <summary>
    /// convert from euler ZYX (in degree) to quaternion. Only that this method did not consider that unity is using a left-handed coordinate system.
    /// No use.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Quaternion ZYXToQuaternion(float x, float y, float z)
    {
        float cx = math.cos(x * 0.5f / 180 * math.PI);
        float sx = math.sin(x * 0.5f / 180 * math.PI);
        float cy = math.cos(y * 0.5f / 180 * math.PI);
        float sy = math.sin(y * 0.5f / 180 * math.PI);
        float cz = math.cos(z * 0.5f / 180 * math.PI);
        float sz = math.sin(z * 0.5f / 180 * math.PI);
        float qw = cx * cy * cz + sx * sy * sz;
        float qx = sx * cy * cz - cx * sy * sz;
        float qy = cx * sy * cz + sx * cy * sz;
        float qz = cx * cy * sz - sx * sy * cz;
        Quaternion q = new(qx, qy, qz, qw);
        return q;
    }
    public static Quaternion ZYXToQuaternion(float[] rot)
    {
        Debug.Assert(rot.Length == 3);
        return ZYXToQuaternion(rot[0], rot[1], rot[2]);
    }
    /// <summary>
    /// Take 6 Euler orders and handedness into consideration.
    /// </summary>
    /// <param name="eulerAngles">bvh eulerAngles, in the order of XYZ, as an input order</param>
    /// <param name="order">eg: "XYZ", "ZYX", etc. No duplicate order like "XXZ". </param>
    /// <returns>the Unity quaternion, which is embeded in a left-handed system</returns>
    public static Quaternion BVHEulerToUnityQuatenion(Vector3 eulerAngles, string order)
    {
        Quaternion rotX = Quaternion.AngleAxis(-eulerAngles.x, Vector3.left);
        Quaternion rotY = Quaternion.AngleAxis(-eulerAngles.y, Vector3.up);
        Quaternion rotZ = Quaternion.AngleAxis(-eulerAngles.z, Vector3.forward);
        Quaternion q = Quaternion.identity;
        /*if (order.Length == 3)
        {
            foreach (char c in order)
            {
                if (c == 'X')       q = q * rotX;
                else if (c == 'Y')  q = q * rotY;
                else if (c == 'Z')  q = q * rotZ;
            }
            return q;
        }
        else if (order.Length == 6)
        {
            foreach (char c in order[3..])
            {
                if (c == 'X')       q = rotX * q;
                else if (c == 'Y')  q = rotY * q;
                else if (c == 'Z')  q = rotZ * q;
            }
            return q;
        }
        else
            return q;*/
        //Debug.Log(order[^3..]);
        foreach (char c in order[^3..])
        {
            //Debug.Log(c);
            if (c == 'X') q = q * rotX;
            else if (c == 'Y') q = q * rotY;
            else if (c == 'Z') q = q * rotZ;
        }
        return q;
    }
    public static Quaternion BVHEulerToUnityQuatenion(float[] eulerAngles, string order)
    {
        return BVHEulerToUnityQuatenion(new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]), order);
    }
}
