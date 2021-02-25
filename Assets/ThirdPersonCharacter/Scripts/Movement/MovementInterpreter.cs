﻿/* Movement Interpreter: James Agerton 2020
 * 
 * Must be used with ThirdPersonControls (Unity Input System (new)).
 * Takes in entered direction from the "stick" (input) and translates that to worldspace direction
 * based on a transform (presumably, the camera). The math used to make the transform requires a 
 * correction of a few degrees to avoid the value flipping into the negative. Effectively, this
 * freezes rotation between those angles, but it is preferable to the flipping behaviour.
 */

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

namespace ProceduralCharacter.Movement
{
    public class MovementInterpreter : MonoBehaviour, ThirdPersonControls.IMovementActions
    {
        #region Variables (private)
        // Take inputs and make modified results available for other components
        [SerializeField, Tooltip("The transform of the main camera.")]
        private Transform _cameraTransform;
        [SerializeField, Tooltip("The z value of a direction vector, where the script corrects for angle calculation error.")]
        [Range(-1f, -0.8f)]
        private float _angleCorrection = -0.88f;

        private ThirdPersonControls _moveControls;

        private Vector2 _moveStick = Vector2.zero;
        private Vector3 _moveDirection = Vector3.zero;
        private bool _jump = false;
        private bool _sprint = false;
        private bool _crouch = false;
        private bool _roll = false;
        private float _angle = 0f;
        #endregion

        #region Properties
        public Vector2 MoveStick => _moveStick;
        public Vector3 MoveDirection => _moveDirection;
        public bool Jump => _jump;
        public bool Sprint => _sprint;
        public bool Crouch => _crouch;
        public bool Roll => _roll;
        public float Angle => _angle;
        #endregion

        #region Unity Methods
        private void Start()
        {
            if (_moveControls == null)
            {
                _moveControls = new ThirdPersonControls();
                _moveControls.Movement.SetCallbacks(this);
                _moveControls.Enable();
            }

            _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            StickToWorldSpace(_moveStick, _cameraTransform);
        }

        //Some Debug Drawings
        private void OnDrawGizmos()
        {
            float GizmoRadius = 1f;
            Vector3 stick = new Vector3(_moveStick.x, 0f, _moveStick.y) * GizmoRadius;
            Vector3 stickPos = stick + transform.position;
            float stickAngle = Vector3.Angle(Vector3.forward, stick.normalized) *
                (Vector3.Cross(stick.normalized, Vector3.forward).y >= 0f ? -1f : 1f);
            float worldAngle = Vector3.Angle(transform.forward, _moveDirection.normalized) *
                (Vector3.Cross(_moveDirection.normalized, transform.forward).y >= 0f ? -1f : 1f);
            Vector3 camDir = _cameraTransform.forward;
            camDir.y = 0f;
            Vector3 correctionDir = new Vector3(1 + _angleCorrection, 0f, _angleCorrection).normalized;
            Vector3 antiCorrectionDir = new Vector3((1 + _angleCorrection) * -1f, 0f, _angleCorrection).normalized;

            //Draw moveStick
            Handles.color = Color.grey;
            Handles.DrawWireDisc(transform.position, transform.up, GizmoRadius);
            Handles.DrawWireArc(transform.position, transform.up, Vector3.forward, stickAngle, stick.magnitude);
            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(stickPos, 0.1f);
            Gizmos.DrawLine(transform.position, stickPos);
            Gizmos.DrawLine(transform.position, Vector3.forward + transform.position);

            //Draw moveDirection
            Handles.color = Color.cyan;
            Handles.DrawSolidArc(transform.position, transform.up, transform.forward, worldAngle, stick.magnitude / 2f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + (_moveDirection * GizmoRadius), 0.11f);
            Gizmos.DrawLine(transform.position, transform.position + (_moveDirection * GizmoRadius));

            //Draw root
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * stick.magnitude / 2f);

            //Draw CameraDirection
            Handles.color = Color.red;
            Handles.DrawWireArc(transform.position, transform.up, camDir, -7f, GizmoRadius);
            Handles.DrawWireArc(transform.position, transform.up, camDir, 7f, GizmoRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + camDir * (GizmoRadius - 0.2f), transform.position + camDir * (GizmoRadius + 0.2f));

            //Draw Correction range
            Gizmos.DrawLine(transform.position + (correctionDir * (GizmoRadius - 0.2f)), transform.position + (correctionDir * (GizmoRadius + 0.2f)));
            Gizmos.DrawLine(transform.position + (antiCorrectionDir * (GizmoRadius - 0.2f)), transform.position + (antiCorrectionDir * (GizmoRadius + 0.2f)));
        }
        #endregion

        #region Methods
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _jump = true;
            }
            else
            {
                _jump = false;
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _moveStick = context.ReadValue<Vector2>();
            }
            else
            {
                _moveStick = Vector2.zero;
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _sprint = true;
            }
            else
            {
                _sprint = false;
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _crouch = true;
            }
            else
            {
                _crouch = false;
            }
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _roll = true;
            }
            else
            {
                _roll = false;
            }
        }

        public void StickToWorldSpace(
            Vector2 stick,
            Transform RelativeCamera)
        {
            Vector3 stickDirection = new Vector3(stick.x, 0f, stick.y);

            //Get camera rotation
            Vector3 CameraDirection = RelativeCamera.forward;
            CameraDirection.y = 0f;

            Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, CameraDirection);
            if (CameraDirection.z <= _angleCorrection)
            {
                Debug.Log("Correction");
                float mult = stick.x > 0 ? -1f : 1f;
                referentialShift = Quaternion.FromToRotation(Vector3.forward, new Vector3((1 + _angleCorrection) * mult, 0f, _angleCorrection).normalized);
            }

            //convert stick to worldspace
            _moveDirection = referentialShift * stickDirection;

            //find angles
            Vector3 axisSign = Vector3.Cross(_moveDirection, transform.forward);
            _angle = Vector3.Angle(transform.forward, _moveDirection) * (axisSign.y >= 0 ? -1f : 1f);
        }

        /// <summary>
        /// If |min| < |Angle| < |max|, returns true. Otherwise returns false.
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public bool IsPivot(float max, float min)
        {
            if (Mathf.Abs(_angle) < Mathf.Abs(max) && Mathf.Abs(Angle) > Mathf.Abs(min))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
