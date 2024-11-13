using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [Header("General Setup Settings")]
    [SerializeField] InputAction movement;
    [SerializeField] InputAction fire;
    [Tooltip("How fast ship moves up and down based upon player input")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float acceleration = 5f;
    [SerializeField] float deceleration = 5f;
    [SerializeField] float maxSpeed = 15f;
    [Tooltip("How fast player moves horizontally")] [SerializeField] float xRange = 5f;
    [Tooltip("How fast player moves vertically")][SerializeField] float yRange = 3.5f;

    [Header("Laser gun array")]
    [Tooltip("Add all lasers here")]
    [SerializeField] GameObject[] lasers;


    [Header("Screen position based tuning")]
    [SerializeField] float controlPitchFactor = -10f;
    [SerializeField] float positionPitchFactor = -2f;
    [SerializeField] float controlRollFactor = 30f;

    [SerializeField] ParticleSystem laserRight;
    [SerializeField] ParticleSystem laserLeft;

    private Vector2 currentSpeed = Vector2.zero;
    private Vector2 inputVector = Vector2.zero;

    void OnEnable()
    {
        movement.Enable();
        fire.Enable(); // Enable the fire action
    }

    private void OnDisable()
    {
        movement.Disable();
        fire.Disable(); // Disable the fire action
    }

    void Update()
    {
        inputVector = movement.ReadValue<Vector2>();
        ProcessTranslation();
        ProcessRotation();
        ProcessFiring();
    }

    void ProcessRotation()
    {
        float pitchDueToPosition = transform.localPosition.y * positionPitchFactor;
        float pitchDueToControlThrow = inputVector.y * controlPitchFactor;
        float pitch = pitchDueToPosition + pitchDueToControlThrow;

        float roll = -inputVector.x * controlRollFactor;

        transform.localRotation = Quaternion.Euler(pitch, 0, roll);
    }

    void ProcessTranslation()
    {
        currentSpeed.x = Mathf.MoveTowards(currentSpeed.x, inputVector.x * maxSpeed, acceleration * Time.deltaTime);
        currentSpeed.y = Mathf.MoveTowards(currentSpeed.y, inputVector.y * maxSpeed, acceleration * Time.deltaTime);

        if (inputVector.x == 0)
        {
            currentSpeed.x = Mathf.MoveTowards(currentSpeed.x, 0, deceleration * Time.deltaTime);
        }

        if (inputVector.y == 0)
        {
            currentSpeed.y = Mathf.MoveTowards(currentSpeed.y, 0, deceleration * Time.deltaTime);
        }

        float xOffset = currentSpeed.x * Time.deltaTime;
        float yOffset = currentSpeed.y * Time.deltaTime;

        float rawXPos = transform.localPosition.x + xOffset;
        float clampedXPos = Mathf.Clamp(rawXPos, -xRange, xRange);
        float rawYPos = transform.localPosition.y + yOffset;
        float clampedYPos = Mathf.Clamp(rawYPos, -yRange, yRange);

        transform.localPosition = new Vector3(clampedXPos, clampedYPos, transform.localPosition.z);
    }

    void ProcessFiring()
    {
        if (fire.ReadValue<float>() > 0.5f)
        {
            Debug.Log("I'm Shooting");
            if (!laserRight.isPlaying)
            {
                ActiveLasers();
                laserRight.Stop();
                laserRight.Play();
            }
            if (!laserLeft.isPlaying)
            {
                ActiveLasers();
                laserLeft.Stop();
                laserLeft.Play();
            }
        }
        else
        {
            Debug.Log("I'm not shooting");
            if (laserRight.isPlaying)
            {
               DeactivateLasers();
               laserRight.Stop();
            }
            if (laserLeft.isPlaying)
            {
                DeactivateLasers();
               laserLeft.Stop();
            }
        }
    }

    void ActiveLasers()
    {
        foreach (GameObject laser in lasers)
        {
           var emissionModule = laser.GetComponent<ParticleSystem>().emission;
           emissionModule.enabled = true;
            laser.SetActive(true);
        }
    }

    void DeactivateLasers()
    {
        foreach(GameObject laser in lasers)
        {
           var emissionModule = laser.GetComponent<ParticleSystem>().emission;
           emissionModule.enabled = false;
            laser.SetActive(false);
        }
    }
}
