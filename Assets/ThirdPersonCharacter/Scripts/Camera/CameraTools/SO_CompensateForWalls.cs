using UnityEngine;

namespace ProceduralCharacter.Cameras
{
    [CreateAssetMenu(fileName = "New Compensate For Walls", menuName = "ScriptableObjects/Camera/Tools/Compensate For Walls")]
    public class SO_CompensateForWalls : ScriptableObject
    {
        [SerializeField]
        public LayerMask Comp;

        //TODO: calculate normal offset from hit to prevent most near clipping?

        public void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
        {
            RaycastHit wallHit = new RaycastHit();
            if(Physics.Linecast(fromObject, toTarget, out wallHit, Comp))
            {
                toTarget = HandleCompensation(wallHit, fromObject);
            }
        }

        public void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget, LayerMask mask)
        {
            RaycastHit wallHit = new RaycastHit();
            if (Physics.Linecast(fromObject, toTarget, out wallHit, mask))
            {
                toTarget = HandleCompensation(wallHit, fromObject);
            }
        }

        private Vector3 HandleCompensation(RaycastHit wallHit, Vector3 fromObject)
        {
            Debug.DrawLine(wallHit.point, fromObject, Color.red);
            return new Vector3(wallHit.point.x, wallHit.point.y, wallHit.point.z);
        }
    }
}
