using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SmoothBouncingRandomBall : MonoBehaviour
{
    [Header("Area")]
    public float areaSize = 1f;
    public float moveSpeed = 0.5f;

    [Header("Turning")]
    public float turnSpeed = 4f;   // Higher = faster turning, lower = smoother turning

    [Header("Bounce")]
    public float wallPush = 0.02f;
    public float directionChangeChance = 0.15f;

    private Rigidbody rb;
    private Vector3 startPosition;

    private Vector3 currentDirection;
    private Vector3 targetDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        Vector2 random2D = Random.insideUnitCircle.normalized;
        currentDirection = new Vector3(random2D.x, 0f, random2D.y).normalized;
        targetDirection = currentDirection;
    }

    void FixedUpdate()
    {
        KeepInsideArea();

        currentDirection = Vector3.Slerp(
            currentDirection,
            targetDirection,
            turnSpeed * Time.fixedDeltaTime
        ).normalized;

        MoveBall();
    }

    void MoveBall()
    {
        Vector3 velocity = currentDirection * moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    void KeepInsideArea()
    {
        float half = areaSize * 0.5f;
        Vector3 offset = rb.position - startPosition;

        Vector3 newTarget = targetDirection;
        bool changed = false;

        if (offset.x > half && targetDirection.x > 0f)
        {
            newTarget.x *= -1f;
            changed = true;
        }
        else if (offset.x < -half && targetDirection.x < 0f)
        {
            newTarget.x *= -1f;
            changed = true;
        }

        if (offset.z > half && targetDirection.z > 0f)
        {
            newTarget.z *= -1f;
            changed = true;
        }
        else if (offset.z < -half && targetDirection.z < 0f)
        {
            newTarget.z *= -1f;
            changed = true;
        }

        if (changed)
        {
            newTarget.y = 0f;
            targetDirection = newTarget.normalized;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length == 0) return;

        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;
        normal.y = 0f;

        if (normal.sqrMagnitude > 0.001f)
        {
            normal.Normalize();
            targetDirection = Vector3.Reflect(targetDirection, normal).normalized;

            Vector3 push = normal * wallPush;
            rb.position += new Vector3(push.x, 0f, push.z);
        }

        if (Random.value < directionChangeChance)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 0.35f;
            targetDirection = new Vector3(
                targetDirection.x + randomOffset.x,
                0f,
                targetDirection.z + randomOffset.y
            ).normalized;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireCube(center, new Vector3(areaSize, 0.05f, areaSize));
    }
}