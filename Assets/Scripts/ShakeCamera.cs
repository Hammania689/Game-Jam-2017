using UnityEngine;
using System.Collections;

public class ShakeCamera : MonoBehaviour
{
    public static ShakeCamera InstanceSM1;
    private float _amplitude = 0.1f;

    public Vector3 initialPosition;
    private bool isShaking = false;

    void Start()
    {
        InstanceSM1 = this;
        initialPosition = transform.localPosition;
    }

    public void ShakeSM1(float amplitude, float duration)
    {
        _amplitude = amplitude;
        isShaking = true;
        CancelInvoke();
        Invoke("StopShaking", duration);
    }

    public void StopShaking()
    {
        isShaking = false;
    }

    void Update()
    {
        if (isShaking)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * _amplitude;
        }
        else if (!isShaking)
        {
            transform.localPosition = initialPosition;
        }
    }
}
