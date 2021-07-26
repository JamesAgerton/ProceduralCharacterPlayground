using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralCharacter.Movement
{
    [RequireComponent(typeof(MovementController), typeof(MovementInterpreter), typeof(MovementFloatRide))]
    public class MovementCrouch : MonoBehaviour
    {
        #region Variables (PRIVATE)
        MovementController _MC;
        MovementFloatRide _MFR;
        MovementInterpreter _Input;

        [SerializeField, Min(0)]
        float _crouchRideHeightMultiplier = 0.5f;
        [SerializeField, Min(0)]
        float _crouchSpeedMultiplier = 0.5f;
        #endregion

        #region Properties (PUBLIC)
        public float CrouchRideMultiplier => _crouchRideHeightMultiplier;
        public float CrouchSpeedMultiploer => _crouchSpeedMultiplier;
        #endregion

        #region Unity Methods
        // Start is called before the first frame update
        private void Start()
        {
            _MC = GetComponent<MovementController>();
            _MFR = GetComponent<MovementFloatRide>();
            _Input = GetComponent<MovementInterpreter>();
        }

        private void Update()
        {
            if (_Input.Crouch)
            {
                _MFR.RideHeightMultiplier = _crouchRideHeightMultiplier;
                _MC.DefaultSpeedMultiplier = _crouchSpeedMultiplier;
            }
            else
            {
                _MFR.RideHeightMultiplier = 1f;
                _MC.DefaultSpeedMultiplier = 1f;
            }
        }
        #endregion

        #region Methods

        #endregion
    }

}