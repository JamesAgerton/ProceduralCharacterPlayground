using UnityEngine;

namespace ProceduralCharacter.Cameras
{
    [CreateAssetMenu(fileName = "New Camera Tools", menuName = "ScriptableObjects/Camera/Tools/Tools")]
    public class SO_CameraTools : ScriptableObject
    {
        [SerializeField]
        public SO_ResetCamera Resetter;
        [SerializeField]
        public SO_SmoothCamera Smoother;
        [SerializeField]
        public SO_FOVSmooth FOVController;
        [SerializeField]
        public SO_CompensateForWalls Compensate;

        public void Reset(ref Transform gameCam, float deltaTime)
        {
            if(Resetter == null)
            {
                Debug.LogError("No SO_ResetCamera placed in SO_CameraTools slot!", this);
                return;
            }
            Resetter.Reset(ref gameCam, deltaTime);
        }

        public void ResetPosition(ref Transform gameCam, float deltaTime)
        {
            if (Resetter == null)
            {
                Debug.LogError("No SO_ResetCamera placed in SO_CameraTools slot!", this);
                return;
            }
                Resetter.ResetPosition(ref gameCam, deltaTime);
        }

        public void ResetRotation(ref Transform gameCam, float deltaTime)
        {
            if (Resetter == null)
            {
                Debug.LogError("No SO_ResetCamera placed in SO_CameraTools slot!", this);
                return;
            }
            Resetter.ResetRotation(ref gameCam, deltaTime);
        }

        public void PositionSmooth(Transform item, Vector3 from, Vector3 to)
        {
            if(Smoother == null)
            {
                Debug.LogError("No SO_SmoothCamera placed in SO_CameraTools slot!", this);
                return;
            }
            Smoother.Position(item, from, to);
        }

        public void LocalPositionSmooth(Transform item, Vector3 from, Vector3 to)
        {
            if (Smoother == null)
            {
                Debug.LogError("No SO_SmoothCamera placed in SO_CameraTools slot!", this);
                return;
            }
            Smoother.LocalPosition(item, from, to);
        }

        public void FOVSmooth(float TargetFOV)
        {
            if (FOVController == null)
            {
                Debug.LogError("No SO_FOVSmooth placed in SO_CameraTools slot!", this);
                return;
            }
            FOVController.Change(TargetFOV);
        }

        public void FOVSmooth(float TargetFOV, float dampTime)
        {
            if (FOVController == null)
            {
                Debug.LogError("No SO_FOVSmooth placed in SO_CameraTools slot!", this);
                return;
            }
            FOVController.Change(TargetFOV, dampTime);
        }

        public void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
        {
            if (Compensate == null)
            {
                Debug.LogError("No SO_CompensateForWalls placed in SO_CameraTools slot!", this);
                return;
            }
            Compensate.CompensateForWalls(fromObject, ref toTarget);
        }

        public void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget, LayerMask mask)
        {
            if (Compensate == null)
            {
                Debug.LogError("No SO_CompensateForWalls placed in SO_CameraTools slot!", this);
                return;
            }
            Compensate.CompensateForWalls(fromObject, ref toTarget, mask);
        }
    }
}
