// Taken from: http://luminaryapps.com/blog/use-a-pid-loop-to-control-unity-game-objects/
using UnityEngine;

[System.Serializable]
public class PIDController
{
    #region Variables (PRIVATE)
    [Tooltip("Proportional constant (counters current error)")]
    public float Kp = 0.2f;

    [Tooltip("Integral constant (counters cumulated error)")]
    public float Ki = 0.05f;

    [Tooltip("Derivative constant (fights oscillation)")]
    public float Kd = 1f;

    [Tooltip("Current control value")]
    public float value = 0;

    private float _lastError;
    private float _integral;
    #endregion

    #region Properties (PUBLIC)

    #endregion

    #region Unity Methods
    /// <summary>
    /// Update our value, based on the given error. We assume here that the
    /// last update was Time.deltaTime seconds ago.
    /// 
    /// </summary>
    /// <param name="error"></param> Difference between current and desired outcome.
    /// <returns> Updated control value </returns>
    public float Update(float error)
    {
        return Update(error, Time.deltaTime);
    }

    /// <summary>
    /// Update our value, based on the given error, which was last updated
    /// dt seconds ago.
    /// 
    /// </summary>
    /// <param name="error"></param> Difference between current and desired outcome.
    /// <param name="dt"></param> Time step.
    /// <returns> Updated control value </returns>
    public float Update(float error, float dt)
    {
        float derivative = (error - _lastError) / dt;
        _integral += error * dt;
        _lastError = error;

        value = Mathf.Clamp01(Kp * error + Ki * _integral + Kd * derivative);
        return value;
    }
    #endregion

    #region Methods

    #endregion
}
