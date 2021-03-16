using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimpleMovingPlatform)), CanEditMultipleObjects]
public class SimpleMovingPlatformEditor : Editor
{
    protected virtual void OnSceneGUI()
    {
        SimpleMovingPlatform platform = (SimpleMovingPlatform)target;

        for(int i = 0; i < platform.waypoints.Count; i++)
        {
            //float size = HandleUtility.GetHandleSize(platform.waypoints[i]) * 0.5f;
            //Vector3 snap = Vector3.one * 0.5f;

            EditorGUI.BeginChangeCheck();
            Vector3 newTargetPosition = Handles.PositionHandle(platform.waypoints[i], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(platform, "Change waypoint position.");
                platform.waypoints[i] = newTargetPosition;
            }
        }
    }
}
