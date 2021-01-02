using Cinemachine;
using RPG.Saving;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class CameraControl : MonoBehaviour, ISaveable
    {
        [SerializeField] private float scrollSpeed = 10f;
        [SerializeField] private float zoomSpeed = 5f;

        private Quaternion startingRotation;
        private CinemachineVirtualCamera mainCamera;

        private float startingPositionY;

        private void Awake()
        {
            mainCamera = GetComponent<CinemachineVirtualCamera>();
        }
        private void Start()
        {
           startingRotation = transform.rotation;
           startingPositionY = transform.position.y;
        }

        private void LateUpdate()
        {
            CameraMove();
        }

        private void CameraMove()
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputZ = Input.GetAxis("Vertical");
         //   transform.position.y = startingPositionY;

            if (inputZ != 0 || inputX != 0)
            {
                transform.position += Vector3.forward * inputZ * scrollSpeed * Time.deltaTime;
                transform.position += Vector3.right * inputX * scrollSpeed * Time.deltaTime;
                //transform.position += transform.up * inputZ * scrollSpeed * Time.deltaTime;
             //   transform.position += transform.right * inputX * scrollSpeed * Time.deltaTime;

            }

            float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            mainCamera.m_Lens.FieldOfView += -mouseScrollWheel * zoomSpeed * Time.deltaTime;
            mainCamera.m_Lens.FieldOfView = Mathf.Clamp(mainCamera.m_Lens.FieldOfView, 20, 100);
            //transform.position += transform.forward * mouseScrollWheel * zoomSpeed;

        /*    if (Input.GetMouseButton(1))
            {
                
                float mouseSensitivity = 30f;

     //           float mouseXstart = transform.position.x;
        //        float mouseYstart = transform.position.y;

                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                //         Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                //              Debug.Log(mouseRay.direction);

                //   transform.LookAt(mouseRay.origin + mouseRay.direction;
                //                transform.Rotate((mouseRay.origin + mouseRay.direction )* 0.01f);


                //  transform.LookAt(new Vector3(mouseY, mouseX, 0));

                //  transform.LookAt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                //  transform.Rotate(-mouseY, mouseX, 0);
                // transform.eulerAngles = new Vector3(mouseY, mouseX, 0);

                Quaternion currentRotation = transform.rotation;
                float rotationSpeed = 20f;
                if (Input.GetKeyDown(KeyCode.R))
                {
                    currentRotation = startingRotation;
                }

                if (Input.GetKey(KeyCode.D)) currentRotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);
                if (Input.GetKey(KeyCode.A)) currentRotation *= Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, Vector3.up);

             //  if (Input.GetKey(KeyCode.U)) currentRotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.forward);
             //  if (Input.GetKey(KeyCode.O)) currentRotation *= Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, Vector3.forward);

                if (Input.GetKey(KeyCode.S)) currentRotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.right);
                if (Input.GetKey(KeyCode.W)) currentRotation *= Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, Vector3.right);

                transform.rotation = currentRotation;
            }
*/
        }
        // Saving System //

        [System.Serializable]
        struct CameraControlSaveData
        {
            public SerializableVector3 position;
        }

        public object CaptureState()
        {
            CameraControlSaveData data = new CameraControlSaveData();
            data.position = new SerializableVector3(transform.position);

            return data;
        }

        public void RestoreState(object state)
        {
            CameraControlSaveData data = (CameraControlSaveData)state;
            transform.position = data.position.ToVector();
        }
    }
}
