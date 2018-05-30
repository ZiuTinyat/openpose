using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class CharacterAnimController : MonoBehaviour
    {
        //[SerializeField] bool VisualizeJoint;
        //[SerializeField] GameObject JointObject;
        [SerializeField] Vector3 Offset = new Vector3(0f, 1f, 0f);
        [SerializeField] bool AllowFacialAnim = false;
        [SerializeField] List<Transform> Joints;
        [SerializeField] List<Transform> FacialJoints;

        private Dictionary<int, Quaternion> InitRotations = new Dictionary<int, Quaternion>();
        private Dictionary<int, Quaternion> UpdatedRotations = new Dictionary<int, Quaternion>();
        private AnimData frameData;

        private Vector3 InitRootPosition;

        void Start()
        {
            if (Joints.Count == 0)
            {
                Debug.Log("No Joints attached");
                return;
            }
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                InitRotations.Add(i, Joints[i].rotation);
                UpdatedRotations.Add(i, Joints[i].localRotation);
                //if (VisualizeJoint) Instantiate(JointObject, Joints[i], false);
            }
            InitRootPosition = Joints[0].position;
        }
        
        private void InitSkeleton(List<Vector3> posList)
        {
            Debug.Log(posList.Count);
            for (int i = 0; i < Mathf.Min(Joints.Count, posList.Count); i++)
            {
                if (Joints[i] == null) continue;
                Joints[i].position = new Vector3(-posList[i].x, posList[i].y, posList[i].z) / 100f;
            }
        }

        public Transform GetCenter()
        {
            return Joints[0];
        }

        private void UpdateModel()
        {
            Joints[0].position = (-frameData.totalPosition / 100f) + Offset;
            // TODO: global rotation REMIND GINES
            //Vector3 axisAngle = new Vector3(frameData.jointAngles[0].y, -frameData.jointAngles[0].z, -frameData.jointAngles[0].x);
            //Joints[0].rotation = InitRotations[0];
            //Joints[0].Rotate(axisAngle, -2 * axisAngle.magnitude * Mathf.Rad2Deg);
            //UpdatedRotations[0] = Joints[0].localRotation;
            //Joints[0].rotation = InitRotations[0];
            for (int i = 1; i < Joints.Count; i++)
            {
                //if (i < 20 || i > 61) continue;
                if (Joints[i] == null) continue;
                Joints[i].rotation = InitRotations[i];// * frameData.jointAngleToRotation(i);
                Joints[i].Rotate(Vector3.right, frameData.jointAngles[i].x, Space.World);
                Joints[i].Rotate(Vector3.down, frameData.jointAngles[i].y, Space.World);
                Joints[i].Rotate(Vector3.back, frameData.jointAngles[i].z, Space.World);
                UpdatedRotations[i] = Joints[i].localRotation;
                Joints[i].rotation = InitRotations[i];
            }
            for (int i = 1; i < Joints.Count; i++)
            {
                //if (i < 20 || i > 61) continue;
                if (Joints[i] == null) continue;
                Joints[i].localRotation = UpdatedRotations[i];
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Offset -= Joints[0].position - InitRootPosition;
            }
            switch (Controller.Mode)
            {
                case PlayMode.Stream: frameData.Parse(UDPReceiver.ReceivedData); break;
                case PlayMode.FileJson: frameData = DataFrameController.GetCurrentFrame(); break;
            }

            if (frameData.isValid) UpdateModel();
        }
    }
}
