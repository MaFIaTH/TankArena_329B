using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    [Header("References")] 
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")] 
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float turningRate = 30f;
    
    private Vector2 _previousMovementInput;
    
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
        if (!IsOwner) return;
        rb.linearVelocity = bodyTransform.up * (movementSpeed * _previousMovementInput.y);
    }
    
    private void HandleMove(Vector2 movement)
    {
        _previousMovementInput = movement;
    }
}
