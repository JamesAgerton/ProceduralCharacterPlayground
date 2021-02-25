using UnityEngine;

namespace ProceduralCharacter.Cameras
{
    public abstract class CameraMode : ScriptableObject
    {
        public abstract Vector3 DesiredPos { get; }
        public abstract void Calculate(ref Transform camXForm, GameObject TargetObject, float deltaTime, LookInterpreter _input);
    }
}