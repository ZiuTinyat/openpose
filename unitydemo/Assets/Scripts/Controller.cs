using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    public GameObject Item;
    public Vector3 Offset;

    private AnimData data = new AnimData();

	// Use this for initialization
	void Start () {
        UDPReceiver.BeginReceiving();
    }
	
	// Update is called once per frame
	void Update () {
        data.Parse(UDPReceiver.ReceivedData);
        Item.transform.position = data.totalPosition + Offset;
        Item.transform.rotation = data.jointAngleToRotation(0);
	}
}
