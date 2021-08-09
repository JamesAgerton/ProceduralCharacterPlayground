using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralCharacter.Animation
{
    public class CharacterAnimationIK : MonoBehaviour
    {
        #region Variables (PRIVATE)
        Animator _anim;
        
        [SerializeField]
        bool _enableFeetIK = true;
        [SerializeField, Range(0, 2)]
        float _raycastStartHeight = 1.14f;
        [SerializeField, Range(0, 2)]
        float _raycastDownDistance = 1.5f;
        [SerializeField]
        LayerMask _ground;
        [SerializeField]
        float _pelvisOffset = 0f;
        [SerializeField, Range(0, 1)]
        float _pelvisUpDownSpeed = 0.28f;
        [SerializeField, Range(0, 1)]
        float _feetToIKPositionSpeed = 0.5f;

        public string LeftFootAnimVariableName = "LeftFootCurve";
        public string RightFootAnimVariableName = "RightFootCurve";

        public bool UseProIKFeature = false;
        public bool ShowSolverDebug = true;

        Vector3 _rightFootPosition, _leftFootPosition, _rightFootIKPosition, _leftFootIKPosition;
        Quaternion _rightFootIKRotation, _leftFootIKRotation;
        float _lastPelvisPositionY, _lastRightFootPositionY, _lastLeftFootPositionY;

        #endregion

        #region Properties (PUBLIC)

        #endregion

        #region Unity Methods

        private void Start()
        {
            _anim = GetComponent<Animator>();
            if(_anim == null)
            {
                Debug.LogError("CharacterAnimationIK must be on the same gameobject as the Animator component!", this);
            }
        }

        /// <summary>
        /// Updating the AdjustFeetTarget method and also finding the position of each foot inside the Solver Position.
        /// </summary>
        private void FixedUpdate()
        {
            if(!_enableFeetIK) { return; }
            if(_anim == null) { return; }

            AdjustFeetTarget(ref _rightFootPosition, HumanBodyBones.RightFoot);
            AdjustFeetTarget(ref _leftFootPosition, HumanBodyBones.LeftFoot);

            //find and raycast to the ground to find positions
            FeetPositionSolver(_rightFootPosition, ref _rightFootIKPosition, ref _rightFootIKRotation); // handle solver for right foot
            FeetPositionSolver(_leftFootPosition, ref _leftFootIKPosition, ref _leftFootIKRotation);    // handle solver for left foot

        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_enableFeetIK) { return; }
            if(_anim == null) { return; }

            MovePelvisHeight();

            //right foot IK position and rotation -- utilize the pro features here
            _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            if (UseProIKFeature)
            {
                _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, _anim.GetFloat(RightFootAnimVariableName));
            }

            MoveFeetToIKPoint(AvatarIKGoal.RightFoot, _rightFootIKPosition, _rightFootIKRotation, ref _lastRightFootPositionY);


            //left foot IK position and rotation -- utilize the pro features here
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            if (UseProIKFeature)
            {
                _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _anim.GetFloat(LeftFootAnimVariableName));
            }

            MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, _leftFootIKPosition, _leftFootIKRotation, ref _lastLeftFootPositionY);
        }

        private void OnDrawGizmos()
        {
            
        }
        #endregion

        #region Methods
        /// <summary>
        /// Moves feet to IK point.
        /// </summary>
        /// <param name="foot"></param>
        /// <param name="positionIKHolder"></param>
        /// <param name="rotationIKHolder"></param>
        /// <param name="lastFootPositionY"></param>
        void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
        {
            Vector3 targetIKPosition = _anim.GetIKPosition(foot);

            if(positionIKHolder != Vector3.zero)
            {
                targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
                positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

                float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, _feetToIKPositionSpeed);
                targetIKPosition.y += yVariable;

                lastFootPositionY = yVariable;

                targetIKPosition = transform.TransformPoint(targetIKPosition);

                _anim.SetIKRotation(foot, rotationIKHolder);
            }

            _anim.SetIKPosition(foot, targetIKPosition);
        }

        /// <summary>
        /// Moves the height of the pelvis.
        /// </summary>
        void MovePelvisHeight()
        {
            if (_rightFootIKPosition == Vector3.zero || _leftFootIKPosition == Vector3.zero || _lastPelvisPositionY == 0f)
            {
                _lastPelvisPositionY = _anim.bodyPosition.y;
                return;
            }

            float lOffsetPosition = _leftFootIKPosition.y - transform.position.y;
            float rOffsetPosition = _rightFootIKPosition.y - transform.position.y;

            float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

            Vector3 newPelvisPosition = _anim.bodyPosition + Vector3.up * totalOffset;

            newPelvisPosition.y = Mathf.Lerp(_lastPelvisPositionY, newPelvisPosition.y, _pelvisUpDownSpeed);

            _anim.bodyPosition = newPelvisPosition;

            _lastPelvisPositionY = _anim.bodyPosition.y;
        }

        /// <summary>
        /// Locating the feet position via raycast and then solving.
        /// </summary>
        /// <param name="fromSkyPosition">Raycast origin.</param>
        /// <param name="feetIKPositions"></param>
        /// <param name="feetIKRotations"></param>
        void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations)
        {
            //raycast handling section
            RaycastHit feetOutHit;

            if (ShowSolverDebug)
            {
                Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (_raycastDownDistance + _raycastStartHeight), Color.yellow);
            }

            if(Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, _raycastDownDistance + _raycastStartHeight, _ground))
            {
                //Find feet ik positions from sky position
                feetIKPositions = fromSkyPosition;
                feetIKPositions.y = feetOutHit.point.y + _pelvisOffset;
                feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

                return;
            }

            feetIKPositions = Vector3.zero; //it didn't work :(
        }

        /// <summary>
        /// Adjusts the feet target.
        /// </summary>
        /// <param name="feetPositions"></param>
        /// <param name="foot"></param>
        void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
        {
            feetPositions = _anim.GetBoneTransform(foot).position;
            feetPositions.y = transform.position.y + _raycastStartHeight;

        }
        #endregion
    }
}
