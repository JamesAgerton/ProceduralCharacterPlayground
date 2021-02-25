using UnityEngine;

namespace ProceduralCharacter.Cameras
{
    [CreateAssetMenu(fileName = "New FOV Smooth", menuName = "ScriptableObjects/Camera/Tools/FOV Smooth")]
    public class SO_FOVSmooth : ScriptableObject
    {
        //[SerializeField, Tooltip("The Camera to be controlled.")]
        //private Camera cam;
        [SerializeField, Tooltip("Damp time of the FOV change.")]
        private float FOVDampTime = 3f;

        public void Change(float TargetFOV)
        {
            Camera cam = Camera.main;
            cam.fieldOfView = Mathf.Lerp(
                cam.fieldOfView,
                TargetFOV,
                FOVDampTime * Time.deltaTime);
        }

        public void Change(float TargetFOV, float FOVDampTime)
        {
            Camera cam = Camera.main;
            cam.fieldOfView = Mathf.Lerp(
                cam.fieldOfView,
                TargetFOV,
                FOVDampTime * Time.deltaTime);
        }
    }
}
