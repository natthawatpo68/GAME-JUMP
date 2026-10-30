using UnityEngine;

public static class GameSoundEffects
{
    private const int SampleRate = 44100;
    private static AudioClip walk, run, jump, key, door;

    private static AudioClip Tone(string name, float seconds, float frequency, float volume, float decay, float noise = 0f)
    {
        int length = Mathf.CeilToInt(seconds * SampleRate);
        var samples = new float[length];
        for (int i = 0; i < length; i++)
        {
            float time = i / (float)SampleRate;
            float envelope = Mathf.Exp(-decay * time) * Mathf.Clamp01(time * 80f);
            float sine = Mathf.Sin(2f * Mathf.PI * frequency * time);
            float random = (Mathf.PerlinNoise(i * 0.137f, frequency) * 2f - 1f) * noise;
            samples[i] = (sine * (1f - noise) + random) * volume * envelope;
        }
        var clip = AudioClip.Create(name, length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip Sweep(string name, float seconds, float lowFrequency, float highFrequency, float volume, float decay)
    {
        int length = Mathf.CeilToInt(seconds * SampleRate);
        var samples = new float[length];
        float phase = 0f;
        for (int i = 0; i < length; i++)
        {
            float time = i / (float)SampleRate;
            float t = time / seconds;
            float frequency = Mathf.Lerp(lowFrequency, highFrequency, t);
            phase += 2f * Mathf.PI * frequency / SampleRate;
            samples[i] = Mathf.Sin(phase) * volume * Mathf.Exp(-decay * time);
        }
        var clip = AudioClip.Create(name, length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public static void Walk(Vector3 position) => Play(ref walk, () => Tone("Footstep", .09f, 105f, .32f, 23f, .70f), position, .55f);
    public static void Run(Vector3 position) => Play(ref run, () => Tone("RunStep", .07f, 82f, .38f, 27f, .76f), position, .68f);
    public static void Jump(Vector3 position) => Play(ref jump, () => Sweep("Jump", .20f, 210f, 520f, .24f, 7f), position, .7f);
    public static void Key(Vector3 position) => Play(ref key, () => Sweep("KeyPickup", .32f, 720f, 1600f, .30f, 5f), position, .85f);
    public static void Door(Vector3 position) => Play(ref door, () => Tone("DoorOpen", .72f, 86f, .43f, 2.6f, .35f), position, 1f);

    private static void Play(ref AudioClip clip, System.Func<AudioClip> factory, Vector3 position, float volume)
    {
        if (clip == null) clip = factory();
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerSoundEffects : MonoBehaviour
{
    private CharacterController controller;
    private bool jumped;
    private float nextStep;

    private void Awake() => controller = GetComponent<CharacterController>();

    private void Update()
    {
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;
        float speed = horizontalVelocity.magnitude;
        bool moving = controller.isGrounded && speed > .15f;

        if (moving && Time.time >= nextStep)
        {
            bool running = speed > 5.4f;
            if (running) GameSoundEffects.Run(transform.position);
            else GameSoundEffects.Walk(transform.position);
            nextStep = Time.time + (running ? .28f : .43f);
        }

        if (!controller.isGrounded && controller.velocity.y > 3f && !jumped)
        {
            GameSoundEffects.Jump(transform.position);
            jumped = true;
        }
        if (controller.isGrounded) jumped = false;
    }
}

[RequireComponent(typeof(KeyPickup))]
public sealed class KeyPickupSound : MonoBehaviour
{
    private KeyPickup pickup;

    private void Awake() => pickup = GetComponent<KeyPickup>();

    private void OnDisable()
    {
        if (pickup != null && pickup.collected) GameSoundEffects.Key(transform.position);
    }
}

[RequireComponent(typeof(KeyExitManager))]
public sealed class DoorOpenSound : MonoBehaviour
{
    private KeyExitManager manager;
    private bool played;

    private void Awake() => manager = GetComponent<KeyExitManager>();

    private void Update()
    {
        if (!played && manager != null && manager.keysCollected >= manager.keysRequired)
        {
            GameSoundEffects.Door(transform.position);
            played = true;
        }
    }
}