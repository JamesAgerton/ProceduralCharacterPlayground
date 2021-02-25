using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralCharacter.Cameras
{
    [CreateAssetMenu(fileName = "New Dark Souls Camera Mode", menuName = "ScriptableObjects/Camera/Camera Modes/Dark Souls")]
    public class SO_DarkSoulsCameraMode : CameraMode
    {
        #region Variables (private)
        [SerializeField]
        public SO_CameraTools Tools;
        [SerializeField]
        private Vector2 distance = new Vector2(3f, 1.5f);
        [SerializeField]
        private float RotationDegreePerSecond = 60f;
        [SerializeField]
        private Vector2 AngleMaxMin = new Vector2(50f, -80f);

        private Vector3 targetPosition = Vector3.zero;
        #endregion

        #region Properties
        public override Vector3 DesiredPos => targetPosition;
        #endregion

        public override void Calculate(ref Transform camXForm, GameObject TargetObject, float deltaTime, LookInterpreter _input)
        {
            //throw new System.NotImplementedException();

            //Plan: Put rig on characterOffset, rotate based on controls, offset camera along -forward vector of rig
            Vector3 characterOffset = TargetObject.transform.position + Vector3.up;
            targetPosition = new Vector3(0f, distance.y, -distance.x);

            if(_input.LookStick.magnitude > 0f)
            {
                camXForm.parent.Rotate(_input.LookStick.y * RotationDegreePerSecond * Time.deltaTime,
                _input.LookStick.x * RotationDegreePerSecond * Time.deltaTime, 0f, Space.Self);
            }
            if(camXForm.parent.rotation.eulerAngles.x > AngleMaxMin.x &&
                camXForm.parent.rotation.eulerAngles.x < 360f + AngleMaxMin.y)
            {
                if (_input.LookStick.y < 0f)
                {
                    //Debug.Log("Looking down too far.");
                    camXForm.parent.rotation = Quaternion.Euler(new Vector3(AngleMaxMin.y, camXForm.parent.rotation.eulerAngles.y, 0f));
                }
                if (_input.LookStick.y > 0f)
                {
                    //Debug.Log("Looking up too far.");
                    camXForm.parent.rotation = Quaternion.Euler(new Vector3(AngleMaxMin.x, camXForm.parent.rotation.eulerAngles.y, 0f));
                }
            }

            camXForm.parent.rotation = Quaternion.Euler(new Vector3(camXForm.parent.rotation.eulerAngles.x, camXForm.parent.rotation.eulerAngles.y, 0f));

            Vector3 realTarget = camXForm.parent.TransformPoint(targetPosition);

            Tools.CompensateForWalls(characterOffset, ref realTarget);
            Tools.PositionSmooth(camXForm.parent, camXForm.parent.position, characterOffset);
            Tools.LocalPositionSmooth(camXForm, camXForm.localPosition, camXForm.parent.InverseTransformPoint(realTarget));
            camXForm.LookAt(characterOffset);
        }

        #region Methods

        #endregion
    }
}
