using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimController : MonoBehaviour {

    [SerializeField] Vector3 Offset = new Vector3(-20f, 74f, -210f);
    //[SerializeField] bool LoadDataFile = false;
    //[SerializeField] string FileName = "AnimDataFile.json";
    [SerializeField] List<Transform> Joints;

    private Dictionary<int, Quaternion> InitRotations = new Dictionary<int, Quaternion>();
    private AnimData frameData;
    
    void Start () {
        switch (Controller.Mode)
        {
            case PlayMode.Stream: InitStreaming(); break;
            case PlayMode.FileJson: InitLoadDataFile(); break;
            default: InitStreaming(); break;
        }
    }

    private void InitStreaming()
    {
        UDPReceiver.BeginReceiving();
        for (int i = 0; i < Joints.Count; i++)
        {
            if (Joints[i] == null) continue;
            InitRotations.Add(i, Joints[i].localRotation);
        }
    }

    private void InitLoadDataFile()
    {
        //DataFrameController.Init("");
    }

    private void UpdateModel()
    {
        Joints[0].position = -frameData.totalPosition + Offset;
        // TODO: global rotation REMIND GINES
        //Joints[0].localRotation = InitRotations[0];
        //Joints[0].Rotate(new Vector3(-data.jointAngles[0].x, data.jointAngles[0].y, data.jointAngles[0].z), Mathf.Rad2Deg);
        for (int i = 1; i < Joints.Count; i++)
        {
            //if (i < 20 || i > 41) continue;
            if (Joints[i] == null) continue;
            Joints[i].localRotation = InitRotations[i];// * data.jointAngleToRotation(i);
            Joints[i].Rotate(Vector3.right, frameData.jointAngles[i].x, Space.World);
            Joints[i].Rotate(Vector3.down, frameData.jointAngles[i].y, Space.World);
            Joints[i].Rotate(Vector3.back, frameData.jointAngles[i].z, Space.World);
        }
    }
	
	// Update is called once per frame
	void Update () {
        switch (Controller.Mode)
        {
            case PlayMode.Stream: frameData.Parse(UDPReceiver.ReceivedData); break;
            default: frameData.Parse(UDPReceiver.ReceivedData); break;
        }

        if (frameData.isValid) UpdateModel();
    }
}
