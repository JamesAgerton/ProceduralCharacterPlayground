using UnityEngine;
using ProceduralCharacter.Movement;

namespace ProceduralCharacter.Cameras
{
    [CreateAssetMenu(fileName = "New Behind Camera Mode (Procedural)", menuName = "ScriptableObjects/Camera/Camera Modes/Behind(P)")]
    public class SO_BehindCameraMode_P : CameraMode
    {
        #region Variables(Private)
        [SerializeField, Tooltip("Offset distance and height.")]
        private Vector2 distance = new Vector2(3f, 1.5f);
        [SerializeField, Tooltip("Damp time for camera rotation.")]
        private float lookDirDampTime = 0.1f;
        [SerializeField]
        public SO_CameraTools Tools;

        private Vector3 lookDir = new Vector3(0f, 0f, 1f);
        private Vector3 curLookDir = new Vector3(0f, 0f, 1f);
        public Vector3 targetPosition = Vector3.zero;
        private Vector3 velocityLookDir = Vector3.zero;
        #endregion

        #region Properties
        /// <summary>
        /// Distance is the set position in x and y from the Target Object.
        /// </summary>
        public Vector2 Distance => distance;
        /// <summary>
        /// DesiredPos is the calculated position of the camera relative to the TargetObject.
        /// </summary>
        public override Vector3 DesiredPos => targetPosition;
        /// <summary>
        /// LookDirection is also calculated based on the position of the TargetObject and 
        /// its velocity (if a rigidbody is present).
        /// </summary>
        public Vector3 LookDirection => lookDir;
        #endregion

        #region Methods
        public override void Calculate(ref Transform camXForm, GameObject TargetObject, float deltaTime, LookInterpreter _input)
        {
            Tools.Reset(ref camXForm, deltaTime);
            Vector3 characterOffset = TargetObject.transform.position +
                (distance.y * TargetObject.transform.up);

            //If in locomotion (need to check velocity I guess)
            //This part determines where the camera needs to look while its following you.
            MovementInterpreter TargetInput = TargetObject.GetComponent<MovementInterpreter>();
            if (TargetInput != null && !TargetInput.IsPivot(80, 10) && TargetInput.MoveStick.magnitude > 0)
            {
                lookDir = Vector3.Lerp(
                    TargetObject.transform.right * (TargetInput.MoveStick.x < 0 ? 1f : -1f),
                    TargetObject.transform.forward * (TargetInput.MoveStick.y < 0 ? -1f : 1f),
                    Mathf.Abs(Vector3.Dot(camXForm.forward, TargetObject.transform.forward)));

                curLookDir = Vector3.Normalize(characterOffset - camXForm.position);
                curLookDir.y = 0;

                curLookDir = Vector3.SmoothDamp(
                    curLookDir,
                    lookDir,
                    ref velocityLookDir,
                    lookDirDampTime);
            }
            else if (TargetInput == null)
            {
                curLookDir = TargetObject.transform.forward;
            }

            //curLookDir = TargetObject.transform.forward;

            targetPosition = characterOffset +
                TargetObject.transform.up * distance.y -
                Vector3.Normalize(curLookDir) * distance.x;

            //Tools.CompensateForWalls(characterOffset, ref targetPosition);

            if (camXForm.parent != null)
            {
                //Tools.PositionSmooth(camXForm.parent, camXForm.parent.position, targetPosition);
                camXForm.parent.position = targetPosition;
            }

            camXForm.LookAt(characterOffset);
        }
        #endregion
    }

}