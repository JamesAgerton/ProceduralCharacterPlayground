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
        CharacterAnimationController _CAC;

        [SerializeField]
        bool _IKWeightGizmo = false;

        [SerializeField]
        LayerMask _ground;

        [SerializeField, Range(0f, 1f), Tooltip("The difference between the bottom of the model's foot and the IK position.")]
        float _ankleHeight;

        [SerializeField, Tooltip("Curve used to evaluate foot weight based on CharacterAnimationController:StrideFraction.")]
        AnimationCurve _IKWeightCurve;
        [SerializeField, Tooltip("Offset of weight curve from CharacterAnimationController StrideWeightCurve.")]
        float _offset = 0f;
        float _leftFootIKWeight = 1f;
        float _rightFootIKWeight = 1f;
        #endregion

        #region Properties (PUBLIC)

        #endregion

        #region Unity Methods
        // Start is called before the first frame update
        private void Start()
        {
            _anim = GetComponent<Animator>();
            if (_anim == null)
            {
                Debug.LogError("CharacterAnimationIK must be a component on the same object as the Animator component!", this);
            }

            if (_CAC == null)
            {
                Debug.LogError("CharacterAnimatorIK needs a reference to a CharacterAnimationController component!", this);
            }
        }

        private void Update()
        {
            GetFootWeights();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (_anim != null && _CAC != null)
            {
                _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, _leftFootIKWeight);
                _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _leftFootIKWeight);

                _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, _rightFootIKWeight);
                _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, _rightFootIKWeight);

                // Left Foot
                RaycastHit hitInfo;
                Ray ray = new Ray(_anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
                //Debug.DrawRay(ray.origin, ray.direction);
                if (Physics.Raycast(ray, out hitInfo, _ankleHeight + 1.2f, _ground))
                {
                    Vector3 footPosition = hitInfo.point;
                    footPosition.y += _ankleHeight;
                    _anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    _anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hitInfo.normal));
                }

                // Right Foot
                ray = new Ray(_anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
                //Debug.DrawRay(ray.origin, ray.direction);
                if (Physics.Raycast(ray, out hitInfo, _ankleHeight + 1.2f, _ground))
                {
                    Vector3 footPosition = hitInfo.point;
                    footPosition.y += _ankleHeight;
                    _anim.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                    _anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hitInfo.normal));
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_IKWeightGizmo && Application.isPlaying)
            {
                Vector3 Rfoot = _anim.GetIKPosition(AvatarIKGoal.RightFoot);
                Vector3 Lfoot = _anim.GetIKPosition(AvatarIKGoal.LeftFoot);

                Color color = Color.Lerp(Color.red, Color.green, _rightFootIKWeight);
                Gizmos.color = color;
                Gizmos.DrawLine(Rfoot, Rfoot + Vector3.up * _rightFootIKWeight * 0.5f);

                color = Color.Lerp(Color.red, Color.green, _leftFootIKWeight);
                Gizmos.color = color;
                Gizmos.DrawLine(Lfoot, Lfoot + Vector3.up * _leftFootIKWeight * 0.5f);
            }
        }
        #endregion

        #region Methods
        void GetFootWeights()
        {
            if(_CAC != null)
            {
                if (_CAC.IsMoving)
                {
                    _rightFootIKWeight = _IKWeightCurve.Evaluate((_CAC.StrideFraction + _offset) % 0.5f);
                    _leftFootIKWeight = _IKWeightCurve.Evaluate((_CAC.StrideFraction + 0.25f + _offset) % 0.5f);
                }
                else
                {
                    _rightFootIKWeight = 1f;
                    _leftFootIKWeight = 1f;
                }
            }
        }
        #endregion
    }
}
