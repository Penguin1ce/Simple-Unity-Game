using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    private Rigidbody _rb;
    private Vector3 _moveInput;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        gameObject.tag = "Player";
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        _moveInput = new Vector3(horizontal, 0, vertical);
        if (_moveInput.magnitude > 1f) _moveInput.Normalize();
    }

    private void FixedUpdate()
    {
        if (_moveInput.magnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(_moveInput, Vector3.up);
            _rb.rotation = Quaternion.Slerp(_rb.rotation, target, rotationSpeed * Time.fixedDeltaTime);
        }
        Vector3 velocity = _moveInput * moveSpeed;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;
    }
}
