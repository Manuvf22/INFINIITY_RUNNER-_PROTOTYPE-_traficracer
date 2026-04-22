using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSourceEngine;
    [SerializeField] private AudioSource audioSourceSFX;
    [SerializeField] private AudioSource audioSourceAbility3;

    [Header("Audio Clips — SFX")]
    [SerializeField] private AudioClip crashClip;
    [SerializeField] private AudioClip destroyClip;
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip ability3MusicClip;

    [Header("Audio Clips — Motor (3 estados)")]
    [SerializeField] private AudioClip engineCruiseClip;       // sin input = crucero
    [SerializeField] private AudioClip engineAccelerateClip;   // presionando arriba/W
    [SerializeField] private AudioClip engineBrakeClip;        // presionando abajo/S

    [Header("Engine Pitch")]
    [SerializeField] private float minEnginePitch = 0.8f;
    [SerializeField] private float maxEnginePitch = 2.0f;

    [Header("Engine Crossfade")]
    [SerializeField] private float crossfadeSpeed = 4f;

    private enum EngineState { Cruise, Accelerate, Brake }
    private EngineState currentEngineState = EngineState.Cruise;

    private AudioSource audioSourceEngine2;
    private bool usingSource1 = true;
    private float crossfadeProgress = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        audioSourceEngine2 = gameObject.AddComponent<AudioSource>();
        audioSourceEngine2.loop = true;
        audioSourceEngine2.playOnAwake = false;
        audioSourceEngine2.volume = 0f;

        if (audioSourceEngine != null)
        {
            audioSourceEngine.loop = true;
            audioSourceEngine.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (PlayerController.Instance == null) return;
        if (audioSourceEngine == null) return;
        if (!audioSourceEngine.isPlaying && !audioSourceEngine2.isPlaying) return;

        UpdateEngineState();
        UpdateEnginePitch();
        UpdateCrossfade();
    }

    private void UpdateEngineState()
    {
        bool accel = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        bool brake = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);

        EngineState newState;
        if (accel) newState = EngineState.Accelerate;
        else if (brake) newState = EngineState.Brake;
        else newState = EngineState.Cruise;

        if (newState != currentEngineState)
        {
            currentEngineState = newState;
            AudioClip nextClip = ClipForState(newState);
            if (nextClip != null)
                StartCrossfade(nextClip);
        }
    }

    private AudioClip ClipForState(EngineState state)
    {
        return state switch
        {
            EngineState.Accelerate => engineAccelerateClip ?? engineCruiseClip,
            EngineState.Brake => engineBrakeClip ?? engineCruiseClip,
            _ => engineCruiseClip,
        };
    }

    private void StartCrossfade(AudioClip newClip)
    {
        AudioSource incoming = usingSource1 ? audioSourceEngine2 : audioSourceEngine;
        AudioSource outgoing = usingSource1 ? audioSourceEngine : audioSourceEngine2;

        if (incoming.isPlaying && incoming.clip == newClip) return;

        incoming.clip = newClip;
        incoming.pitch = outgoing.pitch;
        incoming.volume = 0f;
        incoming.Play();

        usingSource1 = !usingSource1;
        crossfadeProgress = 0f;
    }

    private void UpdateCrossfade()
    {
        if (crossfadeProgress >= 1f) return;

        crossfadeProgress = Mathf.MoveTowards(crossfadeProgress, 1f, crossfadeSpeed * Time.deltaTime);

        AudioSource active = usingSource1 ? audioSourceEngine : audioSourceEngine2;
        AudioSource inactive = usingSource1 ? audioSourceEngine2 : audioSourceEngine;

        active.volume = crossfadeProgress;
        inactive.volume = 1f - crossfadeProgress;

        if (crossfadeProgress >= 1f)
        {
            inactive.Stop();
            inactive.volume = 0f;
        }
    }

    private void UpdateEnginePitch()
    {
        float t = Mathf.InverseLerp(
            PlayerController.Instance.minSpeed,
            PlayerController.Instance.maxSpeed,
            PlayerController.Instance.currentSpeed);
        float pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, t);

        audioSourceEngine.pitch = pitch;
        audioSourceEngine2.pitch = pitch;
    }

    // ── API pública ──────────────────────────────────────────────

    public void StartEngine()
    {
        if (audioSourceEngine == null) return;
        AudioClip startClip = engineCruiseClip;
        if (startClip == null) return;

        currentEngineState = EngineState.Cruise;
        usingSource1 = true;
        crossfadeProgress = 1f;

        audioSourceEngine.clip = startClip;
        audioSourceEngine.loop = true;
        audioSourceEngine.volume = 1f;
        audioSourceEngine.Play();

        audioSourceEngine2.Stop();
        audioSourceEngine2.volume = 0f;
    }

    public void StopEngine()
    {
        if (audioSourceEngine != null) audioSourceEngine.Stop();
        if (audioSourceEngine2 != null) audioSourceEngine2.Stop();
    }

    public void PlayCrash()
    {
        if (audioSourceSFX != null && crashClip != null)
            audioSourceSFX.PlayOneShot(crashClip);
    }

    public void PlayObstacleDestroy()
    {
        if (audioSourceSFX != null && destroyClip != null)
            audioSourceSFX.PlayOneShot(destroyClip);
    }

    public void PlayCoinCollect()
    {
        if (audioSourceSFX != null && coinClip != null)
            audioSourceSFX.PlayOneShot(coinClip);
    }

    public void PlayAbility3Music()
    {
        if (audioSourceAbility3 != null && ability3MusicClip != null)
        {
            audioSourceAbility3.clip = ability3MusicClip;
            audioSourceAbility3.loop = true;
            audioSourceAbility3.Play();
        }
    }

    public void StopAbility3Music()
    {
        if (audioSourceAbility3 != null)
            audioSourceAbility3.Stop();
    }
}