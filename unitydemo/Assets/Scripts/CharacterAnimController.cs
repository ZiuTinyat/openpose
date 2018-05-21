using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimController : MonoBehaviour {

    public Vector3 Offset;

    [SerializeField] List<Transform> Joints = new List<Transform>();

    private List<Quaternion> InitRotations = new List<Quaternion>();
    private AnimData data = new AnimData();

    // Use this for initialization
    void Start () {
        UDPReceiver.BeginReceiving();
        for (int i = 0; i < Joints.Count; i++)
        {
            InitRotations.Add(Joints[i].localRotation);
        }
    }
	
	// Update is called once per frame
	void Update () {
        data.Parse(UDPReceiver.ReceivedData);
        //Joints[0].position = data.totalPosition + Offset;
        for (int i = 0; i < Joints.Count; i++)
        {
            Joints[i].localRotation = InitRotations[i] * data.jointAngleToRotation(i);
        }
    }
}
