using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float speed = 1f;
    [SerializeField] KeyCode ForwardKey = KeyCode.W;
    [SerializeField] KeyCode BackwardKey = KeyCode.S;
    [SerializeField] KeyCode LeftKey = KeyCode.A;
    [SerializeField] KeyCode RightKey = KeyCode.D;

    // Update is called once per frame
    void Update () {
        Vector3 move = new Vector3();
        if (Input.GetKey(ForwardKey)) move += Time.deltaTime * speed * Vector3.back;
        if (Input.GetKey(BackwardKey)) move += Time.deltaTime * speed * Vector3.forward;
        if (Input.GetKey(LeftKey)) move += Time.deltaTime * speed * Vector3.right;
        if (Input.GetKey(RightKey)) move += Time.deltaTime * speed * Vector3.left;

        transform.Translate(move, Space.World);
    }
}
