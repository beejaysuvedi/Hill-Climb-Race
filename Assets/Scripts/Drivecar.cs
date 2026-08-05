using UnityEngine;

public class DriveCar : MonoBehaviour
{
    [SerializeField] Rigidbody2D frontTire;
    [SerializeField] Rigidbody2D backTire;

    [SerializeField] float speed = 150f;

    float moveInput;

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        frontTire.AddTorque(-moveInput * speed * Time.fixedDeltaTime);
        backTire.AddTorque(-moveInput * speed * Time.fixedDeltaTime);
    }
}