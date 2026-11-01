using UnityEngine;

public class PlaneScript : MonoBehaviour
{
    public float speed = 18f;
    public float acceleration = 12f;
    public Vector3 localForward = Vector3.forward;

    public HoldButton leftButton;
    public HoldButton rightButton;
    public float sideSpeed = 10f;
    public Vector3 localRight = Vector3.right;

    public Transform model;
    public Vector3 tiltAxis = Vector3.forward;
    public float maxTilt = 30f;
    public float tiltSmooth = 6f;

    public float bounceDistance = 6f;
    public float bounceSpeed = 70f;
    public float minBounceSpeed = 50f;

    public float pauseTime = 0.1f;

    public AudioClip passClip;
    [Range(0f, 1f)] public float passVolume = 0.8f;

    public AudioClip hitClip;
    [Range(0f, 1f)] public float hitVolume = 1f;

    public float loopEndX = -330f;
    public float loopStartX = 300f;

    enum State { Forward, Bounce, Pause }
    State state = State.Forward;

    float currentSpeed;
    float retreated;
    float pauseTimer;
    float currentTilt;
    Quaternion modelStartRot;
    AudioSource audioSource;

    void Start()
    {
        currentSpeed = speed;
        audioSource = GetComponent<AudioSource>();
        if (model != null) modelStartRot = model.localRotation;
    }

    void Update()
    {
        Vector3 dir = transform.TransformDirection(localForward.normalized);
        float dt = Time.deltaTime;
        float h = 0f;

        switch (state)
        {
            case State.Forward:
                currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * dt);
                transform.position += dir * currentSpeed * dt;

                h = (rightButton != null && rightButton.isHeld ? 1f : 0f)
                  - (leftButton != null && leftButton.isHeld ? 1f : 0f);
                Vector3 side = transform.TransformDirection(localRight.normalized);
                transform.position += side * h * sideSpeed * dt;
                break;

            case State.Bounce:
                float t = Mathf.Clamp01(retreated / bounceDistance);
                float v = Mathf.Max(bounceSpeed * (1f - t), minBounceSpeed);
                float step = Mathf.Min(v * dt, bounceDistance - retreated);
                transform.position -= dir * step;
                retreated += step;
                if (retreated >= bounceDistance - 0.01f)
                {
                    state = State.Pause;
                    pauseTimer = pauseTime;
                }
                break;

            case State.Pause:
                pauseTimer -= dt;
                if (pauseTimer <= 0f)
                {
                    currentSpeed = 0f;
                    state = State.Forward;
                }
                break;
        }

        if (model != null)
        {
            float targetTilt = -h * maxTilt;
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSmooth * dt);
            model.localRotation = Quaternion.AngleAxis(currentTilt, tiltAxis.normalized) * modelStartRot;
        }

        if (transform.position.x <= loopEndX)
        {
            Vector3 p = transform.position;
            p.x = loopStartX;
            transform.position = p;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PassZone") && audioSource != null && passClip != null)
            audioSource.PlayOneShot(passClip, passVolume);

        if (state == State.Forward && other.CompareTag("Buildings"))
        {
            state = State.Bounce;
            retreated = 0f;
            if (audioSource != null && hitClip != null)
                audioSource.PlayOneShot(hitClip, hitVolume);
        }
    }
}