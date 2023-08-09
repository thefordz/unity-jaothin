using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraShaking : MonoBehaviour
{
    // Transform of the camera to shake. Grabs the gameObject's transform
    // if null.
    public Transform camTransform;

    // How long the object should shake for.
    private float _shakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    private float _shakeAmount = 0.7f;
    private float _decreaseFactor = 1.0f;

    Vector3 originalPos;

    void Awake()
    {
        if (camTransform == null)
        {
            camTransform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    public void ShakeTheScreen(float shakeDuration, float shakeAmount)
    {
        originalPos = camTransform.localPosition;
        this._shakeDuration = shakeDuration;
        this._shakeAmount = shakeAmount;
        StartCoroutine(ScreenIsShakingIEnu());
    }

    IEnumerator ScreenIsShakingIEnu()
    {
        while (_shakeDuration > 0)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * _shakeAmount;
            _shakeDuration -= Time.deltaTime * _decreaseFactor;
            yield return new WaitForSeconds(0.001f);
        }
        _shakeDuration = 0f;
        camTransform.localPosition = originalPos;
    }
    /*
    void Update()
    {
        if (shakeDuration > 0)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeDuration = 0f;
            camTransform.localPosition = originalPos;
        }
     }
    */
}