/* Movement Interpreter: James Agerton 2020
 * 
 * Description: 
 *      Movement Interpreter takes in raw input values and transforms them into worldspace values for other
 *          scripts to use.
 * 
 * Dependencies: 
 *      Unity.InputSystem (a unity package for the input system)
 *               
 * Variables:   
 *      _cameraTransform: Reference viewpoint, required for transform of input from screenspace to worldspace
 *      _angleCorrection: Due to rounding error in transform from screenspace to worldspace, script locks 
 *                          rotation for a small window. Too large a value leaves a noticeable pause when 
 *                          rotating continuously.
 *              
 * Properties:  
 *      MoveStick (Vector2):        Raw stick value from input system.
 *      MoveDirection (Vector3):    Transformed stick value in the world space relative to the reference camera 
 *                                      direction.
 *      Jump (bool):                Bool indicating if the jump button is pressed or not.
 *      Sprint (bool):              Bool indicating if the sprint button is pressed or not.
 *      Crouch (bool):              Bool indicating if the crouch button is pressed or not.
 *      Roll (bool):                Bool indicating if the roll button is pressed or not.
 *      Angle (float):              Angle between the Camera direction and the MoveDirection vector.
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
            Vector3 camDir = _cameraTransform.forward;
            camDir.y = 0f;
            camDir = camDir.normalized;
            Vector3 correctionDir = new Vector3(1 + _angleCorrection, 0f, _angleCorrection).normalized;
            Vector3 antiCorrectionDir = new Vector3((1 + _angleCorrection) * -1f, 0f, _angleCorrection).normalized;

            //Draw a Circle
            Gizmos.color = Color.grey;
            for (int i = 0; i < 18; i++)
            {
                Vector3 start = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (i * 20)) * GizmoRadius, 0, Mathf.Sin(Mathf.Deg2Rad * (i * 20)) * GizmoRadius);
                Vector3 end = new Vector3(Mathf.Cos(Mathf.Deg2Rad * ((i + 1) * 20)) * GizmoRadius, 0, Mathf.Sin(Mathf.Deg2Rad * ((i + 1) * 20)) * GizmoRadius);

                Gizmos.DrawLine(start + transform.position, end + transform.position);
            }

            //Draw moveStick
            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(stickPos, 0.1f);
            Gizmos.DrawLine(transform.position, stickPos);
            Gizmos.DrawLine(transform.position, Vector3.forward + transform.position);

            //Draw moveDirection
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + (_moveDirection * GizmoRadius), 0.11f);
            Gizmos.DrawLine(transform.position, transform.position + (_moveDirection * GizmoRadius));

            //Draw CameraDirection
            Gizmos.color = Color.red;
            Vector3 interior = transform.position + camDir * (GizmoRadius - 0.2f);
            Vector3 exterior = transform.position + camDir * (GizmoRadius + 0.2f);
            interior.y = transform.position.y;
            exterior.y = transform.position.y;
            Gizmos.DrawLine(interior + _cameraTransform.right * 0.05f, exterior + _cameraTransform.right * 0.1f);
            Gizmos.DrawLine(interior + _cameraTransform.right * -0.05f, exterior + _cameraTransform.right * -0.1f);
            Gizmos.DrawLine(interior + _cameraTransform.right * 0.05f, interior + _cameraTransform.right * -0.05f);
            Gizmos.DrawLine(exterior + _cameraTransform.right * 0.1f, exterior + _cameraTransform.right * -0.1f);

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
                //Debug.Log("Correction");
                float mult = stick.x > 0 ? -1f : 1f;
                referentialShift = Quaternion.FromToRotation(Vector3.forward, new Vector3((1 + _angleCorrection) * mult, 0f, _angleCorrection).normalized);
            }

            //convert stick to worldspace
            _moveDirection = referentialShift * stickDirection;

            //find angles
            Vector3 axisSign = Vector3.Cross(_moveDirection, CameraDirection);
            _angle = Vector3.Angle(CameraDirection, _moveDirection) * (axisSign.y >= 0 ? -1f : 1f);
        }

        /// <summary>
        /// If |min| < |Angle| < |max|, returns true. Otherwise returns false.
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public bool IsPivot(float max, float min)
        {
            if (Mathf.Abs(_angle) < Mathf.Abs(max) && Mathf.Abs(_angle) > Mathf.Abs(min))
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
