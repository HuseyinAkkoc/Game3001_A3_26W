using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool rotateTowardMovement = true;
    [SerializeField] private Transform playerStartPos;

    private Rigidbody rb;
    private Vector2 moveInput;

    private void Awake()
    {
        transform.position = playerStartPos.position;
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (moveAction != null)
            moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.action.Disable();
    }

    private void Update()
    {
        if (moveAction == null) return;

        moveInput = moveAction.action.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        if (rotateTowardMovement && moveInput.sqrMagnitude > 0.001f)
        {
            Vector3 lookDir = new Vector3(moveInput.x, 0f, moveInput.y);
            transform.forward = lookDir.normalized;
        }
    }
}