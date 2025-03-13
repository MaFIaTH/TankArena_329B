using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    [Header("References")] 
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ParticleSystem dustCloud;

    [Header("Settings")] 
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float turningRate = 30f;
    [SerializeField] private float particleEmissionValue = 10f;
    private ParticleSystem.EmissionModule _emissionModule;
    
    private Vector2 _previousMovementInput;
    private Vector3 _previousPos;

    private const float ParticleStopThreshold = 0.005f;


    private void Awake()
    {
        _emissionModule = dustCloud.emission;   
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.MoveEvent += HandleMove;
    }
    
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.MoveEvent -= HandleMove;
    }

    private void Update()
    {
        if (!IsOwner) return;
        float zRotation = _previousMovementInput.x * -turningRate * Time.deltaTime;
        bodyTransform.Rotate(0, 0, zRotation);
    }
    
    private void FixedUpdate()
    {
        if ((transform.position - _previousPos).sqrMagnitude > ParticleStopThreshold)
        {
            _emissionModule.rateOverTime = particleEmissionValue;
        }
        else
        {
            _emissionModule.rateOverTime = 0;
        }

        _previousPos = transform.position;
        if (!IsOwner) return;
        rb.linearVelocity = bodyTransform.up * (movementSpeed * _previousMovementInput.y);
    }
    
    private void HandleMove(Vector2 movement)
    {
        _previousMovementInput = movement;
    }
}
