using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AnimData {
    public AnimData(string text)
    {
        isValid = false;
        totalPosition = new Vector3();
        jointAngles = new List<Vector3>();
        if (text.StartsWith("AnimData:"))
        {
            text = text.Substring(9);
            try
            {
                this = JsonUtility.FromJson<AnimData>(text);
                isValid = true;
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
                Debug.Log(text);
            }
        }
    }
    public bool isValid;
    public Vector3 totalPosition;
    public List<Vector3> jointAngles;
    public Quaternion jointAngleToRotation(int index) // deprecated
    {
        if (jointAngles == null)
        {
            Debug.Log("Data not initialized yet");
            return Quaternion.identity;
        }
        if (index >= jointAngles.Count)
        {
            Debug.Log("Joint index " + index + " larger than expected " + jointAngles.Count);
            return Quaternion.identity;
        }
        return ToRotation(jointAngles[index]);
    }
    public void Parse(string text)
    {
        this = new AnimData(text);
    }
    public static Quaternion ToRotation(Vector3 angle) // deprecated
    {
        return Quaternion.AngleAxis(angle.x, Vector3.left)
            * Quaternion.AngleAxis(angle.y, Vector3.down)
            * Quaternion.AngleAxis(angle.z, Vector3.back);
    }	
}

public struct AnimDataSet
{
    public AnimDataSet(string text)
    {
        isValid = false;
        dataList = new List<AnimData>();
        try
        {
            this = JsonUtility.FromJson<AnimDataSet>(text);
            isValid = true;
        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
            Debug.Log(text);
        }
    }
    public bool isValid;
    public List<AnimData> dataList;
}
