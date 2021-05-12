using System.Collections;
using System.Collections.Generic;
using ProceduralCharacter.Animation;
using UnityEngine;

namespace ProceduralCharacter.Movement
{
    public class Movement_StepUp : MonoBehaviour
    {
        #region Variables (PRIVATE)
        private MovementInterpreter _input;
        private Rigidbody _body;

        [SerializeField]
        float _stepUpHeight = 0.7f;
        [SerializeField]
        float _stepUpDist = 0.5f;

        List<ContactPoint> _allCPs;

        #endregion

        #region Properties (PUBLIC)

        #endregion

        #region Unity Methods
        // Start is called before the first frame update
        private void Start()
        {
            _input = GetComponent<MovementInterpreter>();
            _body = GetComponent<Rigidbody>();

            _allCPs = new List<ContactPoint>();
        }

        private void FixedUpdate()
        {
            Vector3 velocity = _body.velocity;

            ContactPoint groundCP = default(ContactPoint);
            bool grounded = FindGround(out groundCP, _allCPs);

            Vector3 stepUpOffset = default(Vector3);
            bool stepUp = false;
            if (grounded)
            {
                stepUp = FindStep(out stepUpOffset, _allCPs, groundCP);
            }

            if (stepUp)
            {
                //Step up is so FUCKED
                _body.MovePosition(_body.position + stepUpOffset + _input.MoveDirection * _stepUpDist);
            }

            _allCPs.Clear();
        }


        private void OnCollisionEnter(Collision collision)
        {
            _allCPs.AddRange(collision.contacts);
        }

        private void OnCollisionStay(Collision collision)
        {
            _allCPs.AddRange(collision.contacts);
        }
        #endregion

        #region Methods
        bool FindGround(out ContactPoint groundCP, List<ContactPoint> allCPs)
        {
            groundCP = default(ContactPoint);
            bool found = false;
            foreach (ContactPoint cp in allCPs)
            {
                //Pointing with some up direction
                if (cp.normal.y > 0.0001f && (found == false || cp.normal.y > groundCP.normal.y))
                {
                    groundCP = cp;
                    found = true;
                }
            }

            return found;
        }

        bool FindStep(out Vector3 stepUpOffset, List<ContactPoint> allCPs, ContactPoint groundCP)
        {
            stepUpOffset = default(Vector3);

            if(_input.MoveStick.magnitude < 0.0001f)
            {
                return false;
            }

            foreach(ContactPoint cp in _allCPs)
            {
                bool test = ResolveStepUp(out stepUpOffset, cp, groundCP);
                if (test)
                {
                    return test;
                }
            }

            return false;
        }

        bool ResolveStepUp(out Vector3 stepUpOffset, ContactPoint stepTestCP, ContactPoint groundCP)
        {
            stepUpOffset = default(Vector3);
            Collider stepCol = stepTestCP.otherCollider;

            //( 1 ) if contact point normal matches that of the stair (y close to 0)
            //if (Mathf.Abs(stepTestCP.normal.y) > 0.1f)  
            //{
            //    Debug.Log("Contact point normal larger than 0.1f");
            //    Debug.DrawRay(stepTestCP.point, stepTestCP.normal, Color.red);
            //    return false;
            //}

            //( 2 ) Make sure contact point is low enough to step up to
            if (!(stepTestCP.point.y - groundCP.point.y < (_body.position.y + _stepUpHeight)))    
            {
                Debug.Log("Contact point too high: " + stepTestCP.point.y);
                return false;
            }

            // ( 3 ) check to see if there's actually a place to step onto
            RaycastHit hitInfo;
            float stepHeight = groundCP.point.y + _stepUpHeight + 0.0001f;
            Vector3 stepTestInvDir = new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;
            Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) + (stepTestInvDir * 0.001f);
            Vector3 direction = Vector3.down;
            if (!(stepCol.Raycast(new Ray(origin, direction), out hitInfo, _stepUpHeight)))
            {
                return false;
            }

            Vector3 stepUpPoint = new Vector3(stepTestCP.point.x, hitInfo.point.y + 0.001f, stepTestCP.point.z) + (stepTestInvDir * 0.001f);
            Vector3 stepUpPointOffset = stepUpPoint - new Vector3(stepTestCP.point.x, groundCP.point.y, stepTestCP.point.z);

            stepUpOffset = stepUpPointOffset;
            return true;
        }
        #endregion
    }

}