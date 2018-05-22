using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimController : MonoBehaviour {

    [SerializeField] Vector3 Offset;

    [SerializeField] List<Transform> Joints;

    private Dictionary<int, Quaternion> InitRotations = new Dictionary<int, Quaternion>();

    private AnimData data = new AnimData();

    private void Awake()
    {
    }
    
    void Start () {
        UDPReceiver.BeginReceiving();
        for (int i = 0; i < Joints.Count; i++)
        {
            if (Joints[i] == null) continue;
            InitRotations.Add(i, Joints[i].localRotation);
        }
    }
	
	// Update is called once per frame
	void Update () {
        data.Parse(UDPReceiver.ReceivedData);
        if(data.isValid)
        {
            //Joints[0].position = -data.totalPosition + Offset;
            // TODO: global rotation REMIND GINES
            //Joints[0].localRotation = InitRotations[0];
            //Joints[0].Rotate(new Vector3(-data.jointAngles[0].x, data.jointAngles[0].y, data.jointAngles[0].z), Mathf.Rad2Deg);
            for (int i = 1; i < Joints.Count; i++)
            {
                if (i < 20 || i > 41) continue;
                if (Joints[i] == null) continue;
                Joints[i].localRotation = InitRotations[i];// * data.jointAngleToRotation(i);
                Joints[i].Rotate(Vector3.right, data.jointAngles[i].x, Space.World);
                Joints[i].Rotate(Vector3.down, data.jointAngles[i].y, Space.World);
                Joints[i].Rotate(Vector3.back, data.jointAngles[i].z, Space.World);
            }
        }
    }
}
