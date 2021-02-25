/* Look Interpreter: James Agerton 2020
 * 
 * Tracks camera state and provides look input.
 */

using UnityEngine;
using UnityEngine.InputSystem;

namespace ProceduralCharacter.Cameras
{
    public enum CamStates
    {
        Behind,
        Target,
        Free
    }

    public class LookInterpreter : MonoBehaviour, ThirdPersonControls.ICameraActions
    {
        #region Variables (private)
        private ThirdPersonControls cameraControls;
        private CamStates camState;
        private Vector2 lookStick;
        #endregion

        #region Properties
        public Vector2 LookStick => lookStick;
        public CamStates CamState => camState;
        #endregion

        #region Unity Methods
        private void Start()
        {
            if (cameraControls == null)
            {
                cameraControls = new ThirdPersonControls();
                //Tell action map to update this script during input.
                cameraControls.Camera.SetCallbacks(this);
                cameraControls.Enable();
            }
        }


        #endregion

        #region Methods
        public void OnExitFPV(InputAction.CallbackContext context)
        {
            if (camState != CamStates.Behind)
            {
                camState = CamStates.Behind;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                lookStick = context.ReadValue<Vector2>();
                
                if(camState != CamStates.Free)
                {
                    camState = CamStates.Free;
                }
            }
            else
            {
                lookStick = Vector2.zero;
            }
        }

        public void OnTargetView(InputAction.CallbackContext context)
        {
            if(context.performed && 
                camState != CamStates.Target)
            {
                camState = CamStates.Target;
            }
            else
            {
                camState = CamStates.Behind;
            }
        }
        #endregion
    }
}

