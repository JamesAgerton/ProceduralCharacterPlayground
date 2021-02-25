/* CameraController2: James Agerton 2020
 * 
 * The controller is placed on the camera, which should have an empty parent object (rig).
 * The camera object also requires a LookInterpreter script.
 * This controller passes values to scriptable objects which control camera behaviour.
 * It selects from several slots which mode to use, and manages which mode should be active.
 */

using UnityEngine;

namespace ProceduralCharacter.Cameras
{
    [RequireComponent(typeof(LookInterpreter))]
    public class CameraController2 : MonoBehaviour
    {
        #region Variables (Private)
        public LookInterpreter LookInterpreter;

        [SerializeField, Tooltip("The object the camera should follow and focus on.")]
        public GameObject Target;

        [SerializeField]
        public CameraMode DefaultCam;
        //[SerializeField]
        //private CameraMode FreeCam;

        public Transform cameraXForm;
        public Transform rigXForm;
        #endregion

        #region Properties
        /// <summary>
        /// CamState is the enum describing which mode the camera uses for behavior
        /// </summary>
        public CamStates CamState => LookInterpreter.CamState;
        /// <summary>
        /// LookStick is the Vector2 describing the position of the camera controlling stick.
        /// </summary>
        public Vector2 LookStick => LookInterpreter.LookStick;
        /// <summary>
        /// Transform used to control rotation of the camera
        /// </summary>
        public Transform CameraXForm => cameraXForm;
        /// <summary>
        /// Transform used to control camera position (parent to CameraXForm).
        /// </summary>
        public Transform RigXForm => rigXForm;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            LookInterpreter = GetComponent<LookInterpreter>();
            cameraXForm = this.transform;
            rigXForm = this.transform.parent;

            //Make sure camera has parent
            if(rigXForm == null)
            {
                //throw missing parent error
                Debug.LogError("Camera has no parent for rig control.", this);
            }

            //Check that target is available
            if(Target == null)
            {
                GameObject[] gos;
                gos = GameObject.FindGameObjectsWithTag("Player");
                if(gos.Length == 0)
                {
                    //throw no target warning
                    Debug.LogError("Camera has no target and there are no objects tagged \"Player\"!", this);
                }
                else
                {
                    Target = gos[0];
                }
            }
        }

        //Some Debug Drawings
        private void OnDrawGizmos()
        {
            //Draw TargetPosition
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(DefaultCam.DesiredPos, 0.2f);
        }

        private void Update()
        {
            switch (CamState)
            {
                //case CamStates.Behind:
                default:
                    DefaultCam.Calculate(ref cameraXForm, Target, Time.deltaTime, LookInterpreter);
                    break;
            }
        }
        #endregion

        #region Methods

        #endregion
    }
}
