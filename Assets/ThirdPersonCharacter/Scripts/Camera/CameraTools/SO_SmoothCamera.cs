using UnityEngine;

namespace ProceduralCharacter.Cameras
{

    [CreateAssetMenu(fileName = "New Smooth Camera", menuName = "ScriptableObjects/Camera/Tools/Smooth Camera")]
    public class SO_SmoothCamera : ScriptableObject
    {
        [SerializeField, Tooltip("Damp time for smoothDamp function.")]
        private float CamSmoothDampTime = 0.1f;

        private Vector3 PositionSmooth = Vector3.zero;
        private Vector3 LocalSmooth = Vector3.zero;

        public void Position(Transform item, Vector3 from, Vector3 to)
        {
            item.position = Vector3.SmoothDamp(from, to, ref PositionSmooth, CamSmoothDampTime * Time.deltaTime);
        }

        public void LocalPosition(Transform item, Vector3 from, Vector3 to)
        {
            item.localPosition = Vector3.SmoothDamp(from, to, ref LocalSmooth, CamSmoothDampTime * Time.deltaTime);
        }
    }
}
