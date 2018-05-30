using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class CameraController : MonoBehaviour
    {
        // Singleton
        private static CameraController instance;

        //public float speed = 1f;
        //[SerializeField] KeyCode ForwardKey = KeyCode.W;
        //[SerializeField] KeyCode BackwardKey = KeyCode.S;
        //[SerializeField] KeyCode LeftKey = KeyCode.A;
        //[SerializeField] KeyCode RightKey = KeyCode.D;

        [SerializeField] Transform MyCamera;

        public Transform HumanCenter;
        public float FollowRotatingCoeff = 0.5f;
        public float FollowTranslatingCoeff = 0.3f;
        public float RotatingStiff = 0.5f;
        public float TranslatingStiff = 0.5f;
        private float InitDistance;
        private Vector3 InitPos;
        private Quaternion InitRotation;
        private Vector3 GoalWatch;
        private Vector3 GoalPos;

        public static Transform FocusCenter { set { instance.HumanCenter = value; } get { return instance.HumanCenter; } }
        public static Vector3 RotationCenter { set { instance.transform.position = value; } get { return instance.transform.position; } }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            InitDistance = Vector3.Dot(GoalPos - MyCamera.position, MyCamera.forward);
            InitPos = MyCamera.position;
            InitRotation = MyCamera.rotation;
        }

        private void SetGoalUpdate()
        {
            GoalWatch = HumanCenter.position;
            GoalPos = HumanCenter.position;
        }

        private void MoveToGoalUpdate()
        {
            // Rotation
            float angleDiff;
            Vector3 axisRotate;
            Quaternion.FromToRotation(transform.forward, GoalWatch - transform.position).ToAngleAxis(out angleDiff, out axisRotate);
            //float deltaAngle = FollowRotatingCoeff * Mathf.Sqrt(angleDiff) * Time.deltaTime; // index 1/2
            float deltaAngle = FollowRotatingCoeff * angleDiff * Mathf.Abs(angleDiff) * Time.deltaTime; // index 2
            transform.Rotate(axisRotate, deltaAngle, Space.World);

            // Translation
            float distanceDiff = Vector3.Dot(GoalPos - MyCamera.position, MyCamera.forward) - InitDistance;
            //float deltaMove = FollowTranslatingCoeff * distanceDiff * Time.deltaTime; // index 1
            float deltaMove = FollowTranslatingCoeff * distanceDiff * Mathf.Abs(distanceDiff) * Time.deltaTime; // index 2
            MyCamera.localPosition += new Vector3(0, 0, deltaMove);
            if (MyCamera.localPosition.z < 0f) MyCamera.localPosition = new Vector3(0f, 0f, 0f);
        }

        /*private void KeyboardControlUpdate()
        {
            Vector3 move = new Vector3();
            if (Input.GetKey(ForwardKey)) move += Time.deltaTime * speed * Vector3.back;
            if (Input.GetKey(BackwardKey)) move += Time.deltaTime * speed * Vector3.forward;
            if (Input.GetKey(LeftKey)) move += Time.deltaTime * speed * Vector3.right;
            if (Input.GetKey(RightKey)) move += Time.deltaTime * speed * Vector3.left;

            transform.Translate(move, Space.World);
        }*/

        void Update()
        {
            SetGoalUpdate();
            MoveToGoalUpdate();
        }
    }
}
