using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerClipSoundEffects : MonoBehaviour
{
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField, Range(0f, 1f)] private float walkVolume = .55f;
    [SerializeField, Range(0f, 1f)] private float runVolume = .68f;
    [SerializeField, Range(0f, 1f)] private float jumpVolume = .7f;

    private CharacterController controller;
    private Animator animator;
    private AudioSource source;
    private bool jumped;
    private static readonly int Speed = Animator.StringToHash("Speed");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        source = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    private void Update()
    {
        float animationSpeed = animator != null ? animator.GetFloat(Speed) : controller.velocity.magnitude;
        bool moving = controller.isGrounded && animationSpeed > .05f;
        bool running = animationSpeed > .75f;
        AudioClip movementClip = moving ? (running ? runClip : walkClip) : null;

        if (movementClip == null)
        {
            if (source.isPlaying) source.Stop();
            source.clip = null;
        }
        else if (source.clip != movementClip)
        {
            source.Stop();
            source.clip = movementClip;
            source.loop = true;
            source.volume = running ? runVolume : walkVolume;
            source.Play();
        }
        else if (!source.isPlaying)
        {
            source.loop = true;
            source.Play();
        }

        if (!controller.isGrounded && controller.velocity.y > 3f && !jumped)
        {
            if (jumpClip != null) AudioSource.PlayClipAtPoint(jumpClip, transform.position, jumpVolume);
            jumped = true;
        }
        if (controller.isGrounded) jumped = false;
    }
}

[RequireComponent(typeof(KeyPickup))]
public sealed class KeyClipSound : MonoBehaviour
{
    [SerializeField] private AudioClip keyClip;
    [SerializeField, Range(0f, 1f)] private float volume = .85f;

    private KeyPickup pickup;

    private void Awake() => pickup = GetComponent<KeyPickup>();

    private void OnDisable()
    {
        if (pickup != null && pickup.collected && keyClip != null)
            AudioSource.PlayClipAtPoint(keyClip, transform.position, volume);
    }
}