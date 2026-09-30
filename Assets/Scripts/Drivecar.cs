using UnityEngine;

public class DriveCar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D frontTire;
    [SerializeField] private Rigidbody2D backTire;
    [SerializeField] private Rigidbody2D carBody;
    [SerializeField] private EnvironmentGenerator road;

    [Header("Driving")]
    [SerializeField] private float motorTorque = 3f;
    [SerializeField] private float maxForwardSpeed = 18f;
    [SerializeField] private float maxReverseSpeed = 7f;
    [SerializeField] private float brakeDeceleration = 14f;
    [SerializeField] private float coastDeceleration = 1.5f;

    private Rigidbody2D[] vehicleBodies;

    private CircleCollider2D frontCollider;
    private CircleCollider2D backCollider;

    private readonly ContactPoint2D[] contacts =
        new ContactPoint2D[16];

    private float moveInput;

    private void Awake()
    {
        if (carBody == null)
            carBody = GetComponent<Rigidbody2D>();

        if (frontTire == null ||
            backTire == null ||
            carBody == null ||
            road == null)
        {
            Debug.LogError(
                "DriveCar: Assign both tyres, VEHICLE as Car Body, " +
                "and Ground as Road.",
                this
            );

            enabled = false;
            return;
        }

        frontCollider =
            frontTire.GetComponent<CircleCollider2D>();

        backCollider =
            backTire.GetComponent<CircleCollider2D>();

        if (frontCollider == null || backCollider == null)
        {
            Debug.LogError(
                "DriveCar: Both tyres need a Circle Collider 2D.",
                this
            );

            enabled = false;
            return;
        }

        vehicleBodies =
            carBody.GetComponentsInChildren<Rigidbody2D>();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    private void OnDisable()
    {
        moveInput = 0f;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            moveInput = 0f;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        bool grounded =
            TouchingRoad(frontCollider) ||
            TouchingRoad(backCollider);

        float currentSpeed = carBody.linearVelocity.x;

        bool braking = moveInput * currentSpeed < -0.15f;
        bool coasting = Mathf.Abs(moveInput) < 0.01f;

        float targetSpeed = currentSpeed;

        if (grounded)
        {
            if (braking || coasting)
            {
                float deceleration = braking
                    ? brakeDeceleration
                    : coastDeceleration;

                targetSpeed = Mathf.MoveTowards(
                    currentSpeed,
                    0f,
                    deceleration * Time.fixedDeltaTime
                );
            }

            // Gradually correct speed above the allowed limit.
            targetSpeed = Mathf.MoveTowards(
                targetSpeed,
                Mathf.Clamp(
                    targetSpeed,
                    -maxReverseSpeed,
                    maxForwardSpeed
                ),
                brakeDeceleration * Time.fixedDeltaTime
            );

            float speedChange = targetSpeed - currentSpeed;

            // Apply the same horizontal velocity change to all
            // vehicle parts so the joints are not pulled apart.
            foreach (Rigidbody2D body in vehicleBodies)
            {
                if (body == null ||
                    body.bodyType != RigidbodyType2D.Dynamic ||
                    !body.simulated)
                {
                    continue;
                }

                body.AddForce(
                    Vector2.right * speedChange * body.mass,
                    ForceMode2D.Impulse
                );
            }
        }

        if (braking || coasting)
        {
            if (grounded)
            {
                MatchRollingSpeed(
                    frontTire,
                    frontCollider,
                    targetSpeed
                );

                MatchRollingSpeed(
                    backTire,
                    backCollider,
                    targetSpeed
                );
            }

            return;
        }

        float speedLimit = moveInput > 0f
            ? maxForwardSpeed
            : maxReverseSpeed;

        if (moveInput * currentSpeed < speedLimit)
        {
            DriveWheel(frontTire, frontCollider, speedLimit);
            DriveWheel(backTire, backCollider, speedLimit);
        }
    }

    private bool TouchingRoad(Collider2D wheel)
    {
        int count = wheel.GetContacts(contacts);

        for (int i = 0; i < count; i++)
        {
            Collider2D first = contacts[i].collider;
            Collider2D second = contacts[i].otherCollider;

            bool firstIsRoad =
                first != null &&
                first.GetComponentInParent<EnvironmentGenerator>()
                    == road;

            bool secondIsRoad =
                second != null &&
                second.GetComponentInParent<EnvironmentGenerator>()
                    == road;

            if (firstIsRoad || secondIsRoad)
                return true;
        }

        return false;
    }

    private float GetRadius(CircleCollider2D wheel)
    {
        Vector3 scale = wheel.transform.lossyScale;

        float largestScale = Mathf.Max(
            Mathf.Abs(scale.x),
            Mathf.Abs(scale.y)
        );

        return Mathf.Max(0.05f, wheel.radius * largestScale);
    }

    private void MatchRollingSpeed(
        Rigidbody2D wheel,
        CircleCollider2D wheelCollider,
        float horizontalSpeed)
    {
        float targetAngularSpeed =
            -horizontalSpeed /
            GetRadius(wheelCollider) *
            Mathf.Rad2Deg;

        float nextAngularSpeed = Mathf.MoveTowards(
            wheel.angularVelocity,
            targetAngularSpeed,
            6000f * Time.fixedDeltaTime
        );

        float impulse =
            (nextAngularSpeed - wheel.angularVelocity) *
            Mathf.Deg2Rad *
            wheel.inertia;

        wheel.AddTorque(impulse, ForceMode2D.Impulse);
    }

    private void DriveWheel(
        Rigidbody2D wheel,
        CircleCollider2D wheelCollider,
        float speedLimit)
    {
        float targetAngularSpeed =
            -moveInput *
            speedLimit /
            GetRadius(wheelCollider) *
            Mathf.Rad2Deg;

        float angularChange =
            targetAngularSpeed - wheel.angularVelocity;

        float requestedImpulse =
            angularChange * Mathf.Deg2Rad * wheel.inertia;

        float maximumImpulse =
            motorTorque * Time.fixedDeltaTime;

        wheel.AddTorque(
            Mathf.Clamp(
                requestedImpulse,
                -maximumImpulse,
                maximumImpulse
            ),
            ForceMode2D.Impulse
        );
    }
}