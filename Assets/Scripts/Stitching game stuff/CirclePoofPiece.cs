using UnityEngine;
using UnityEngine.UI;

public class CirclePoofPiece : MonoBehaviour
{
    [SerializeField] private Graphic graphic;
    [SerializeField] private float lifetime = 0.6f;
    [SerializeField] private float gravity = 700f;          // UI units/sec^2
    [SerializeField] private float spinDamping = 6f;        // higher = faster decay

    private Vector2 velocity;
    private float angularVelocity;
    private float age;
    private RectTransform rect;

    public void Init(Vector2 startVelocity, float startAngularVelocity, float life)
    {
        rect = (RectTransform)transform;
        velocity = startVelocity;
        angularVelocity = startAngularVelocity;
        lifetime = Mathf.Max(0.05f, life);
        age = 0f;

        if (graphic == null)
            graphic = GetComponent<Graphic>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        age += dt;

        // motion
        velocity.y -= gravity * dt;
        rect.anchoredPosition += velocity * dt;

        // spin with decay
        rect.localEulerAngles += new Vector3(0f, 0f, angularVelocity * dt);
        angularVelocity = Mathf.Lerp(angularVelocity, 0f, spinDamping * dt);

        // fade out
        float t = age / lifetime;
        if (graphic != null)
        {
            Color c = graphic.color;
            c.a = 1f - Mathf.Clamp01(t);
            graphic.color = c;
        }

        if (age >= lifetime)
            Destroy(gameObject);
    }
}