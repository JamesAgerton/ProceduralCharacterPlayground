using UnityEngine;

namespace ProceduralCharacter.Cameras
{

    [CreateAssetMenu(fileName = "New Camera Reset", menuName = "ScriptableObjects/Camera/Tools/Reset Camera")]
    public class SO_ResetCamera : ScriptableObject
    {
        [SerializeField, Tooltip("Lerp speed of camera reset.")]
        private float LerpSpeed = 1.5f;

        public void Reset(
            ref Transform CameraXForm,
            float deltaTime)
        {
            CameraXForm.localPosition = Vector3.Lerp(
                CameraXForm.localPosition,
                Vector3.zero,
                deltaTime * LerpSpeed);
            CameraXForm.localRotation = Quaternion.Lerp(
                CameraXForm.localRotation,
                Quaternion.identity,
                deltaTime * LerpSpeed);
        }

        public void ResetRotation(
            ref Transform CameraXForm,
            float deltaTime)
        {
            CameraXForm.localRotation = Quaternion.Lerp(
                CameraXForm.localRotation,
                Quaternion.identity,
                deltaTime * LerpSpeed);
        }

        public void ResetPosition(
            ref Transform CameraXForm, 
            float deltaTime)
        {
            CameraXForm.localPosition = Vector3.Lerp(
                CameraXForm.localPosition,
                Vector3.zero,
                deltaTime * LerpSpeed);
        }
    }

}
