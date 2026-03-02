using UnityEngine;

public class PlayerAudioSystem : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private AudioSource movementAudioSource;
    [SerializeField] private AudioSource damageAudioSource;
    [SerializeField] private AudioSource deathAudioSource;

    [Header("Settings")]
    [SerializeField] private bool useRandomPitch = true;
    [SerializeField] private float pitchVariation = 0.1f;

    private ThirdPersonController movementController;
    private AttackComboController attackController;
    private PlayerHealth playerHealth;
    private Vector3 previousPosition;
    private bool isMoving;

    void Start()
    {
        // Get references
        movementController = GetComponent<ThirdPersonController>();
        attackController = GetComponent<AttackComboController>();
        playerHealth = GetComponent<PlayerHealth>();

        // Create audio sources if not assigned
        if (attackAudioSource == null)
            attackAudioSource = gameObject.AddComponent<AudioSource>();
        if (movementAudioSource == null)
            movementAudioSource = gameObject.AddComponent<AudioSource>();
        if (damageAudioSource == null)
            damageAudioSource = gameObject.AddComponent<AudioSource>();
        if (deathAudioSource == null)
            deathAudioSource = gameObject.AddComponent<AudioSource>();

        // Configure audio sources
        ConfigureAudioSource(attackAudioSource, spatialBlend: 0.5f);
        ConfigureAudioSource(movementAudioSource, loop: false, spatialBlend: 0.3f);
        ConfigureAudioSource(damageAudioSource, spatialBlend: 0.5f);
        ConfigureAudioSource(deathAudioSource, spatialBlend: 0.5f);

        // Subscribe to events
        if (attackController != null)
        {
            attackController.OnAttackStart += PlayAttackSound;
        }

        // Diagnostic info to help locate why player audio might be silent
        if (AudioManager.Instance == null)
            Debug.LogWarning("🔇 AudioManager.Instance is null - ensure AudioManager GameObject exists in the scene.");
        else
        {
            Debug.Log($"🔊 AudioManager found. Player attack sounds count: {AudioManager.Instance.playerAttackSounds.Count}, movement: {AudioManager.Instance.playerMovementSounds.Count}, damage: {AudioManager.Instance.playerDamageSounds.Count}, death: {AudioManager.Instance.playerDeathSounds.Count}");
        }

        if (attackController == null)
            Debug.LogWarning("⚠️ AttackComboController not found on Player — attack audio will not trigger via events.");

        previousPosition = transform.position;
    }

    void Update()
    {
        CheckMovement();
    }

    void OnDestroy()
    {
        if (attackController != null)
        {
            attackController.OnAttackStart -= PlayAttackSound;
        }
    }

    private void ConfigureAudioSource(AudioSource source, bool loop = false, float spatialBlend = 1f)
    {
        source.spatialBlend = spatialBlend;
        source.loop = loop;
        source.playOnAwake = false;
    }

    private void CheckMovement()
    {
        // Calculate movement
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);
        bool currentlyMoving = distanceMoved > 0.01f;

        // Play movement sound when starting to move
        if (currentlyMoving && !isMoving)
        {
            PlayMovementSound();
        }

        // Stop movement sound when stopping
        if (!currentlyMoving && isMoving && movementAudioSource.isPlaying)
        {
            movementAudioSource.Stop();
        }

        isMoving = currentlyMoving;
        previousPosition = transform.position;
    }

    public void PlayAttackSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("🔇 Cannot play player attack sound: AudioManager.Instance is null");
            return;
        }

        var soundList = AudioManager.Instance.playerAttackSounds;
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("🔇 No player attack sounds configured in AudioManager");
            return;
        }

        // Stop movement so attack is audible
        if (movementAudioSource != null && movementAudioSource.isPlaying)
            movementAudioSource.Stop();

        // Ensure attack source is not currently playing
        if (attackAudioSource != null && attackAudioSource.isPlaying)
            attackAudioSource.Stop();

        string categoryKey = "Player_attack";
        bool played = AudioManager.Instance.PlayRandomSoundOnSourceLimited(attackAudioSource, soundList, categoryKey, maxConcurrent: 6);
        if (!played)
            AudioManager.Instance.PlayRandomSoundOnSource(attackAudioSource, soundList);

        if (useRandomPitch)
        {
            attackAudioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }

        Debug.Log("🔊 Player Attack Sound");
    }

    public void PlayMovementSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("🔇 Cannot play player movement sound: AudioManager.Instance is null");
            return;
        }

        var soundList = AudioManager.Instance.playerMovementSounds;
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("🔇 No player movement sounds configured in AudioManager");
            return;
        }

        // Don't restart movement sound if already playing
        if (movementAudioSource != null && movementAudioSource.isPlaying)
            return;

        string categoryKey = "Player_movement";
        bool played = AudioManager.Instance.PlayRandomSoundOnSourceLimited(movementAudioSource, soundList, categoryKey, maxConcurrent: 2);
        if (!played)
        {
            // fallback to normal play
            AudioManager.Instance.PlayRandomSoundOnSource(movementAudioSource, soundList);
        }

        Debug.Log("🔊 Player Movement Sound");
    }

    public void PlayDamageSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("🔇 Cannot play player damage sound: AudioManager.Instance is null");
            return;
        }

        var soundList = AudioManager.Instance.playerDamageSounds;
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("🔇 No player damage sounds configured in AudioManager");
            return;
        }

        // Stop movement to prioritize damage sound
        if (movementAudioSource != null && movementAudioSource.isPlaying)
            movementAudioSource.Stop();

        string categoryKey = "Player_damage";
        bool played = AudioManager.Instance.PlayRandomSoundOnSourceLimited(damageAudioSource, soundList, categoryKey, maxConcurrent: 3);
        if (!played)
            AudioManager.Instance.PlayRandomSoundOnSource(damageAudioSource, soundList);

        Debug.Log("🔊 Player Damage Sound");
    }

    public void PlayDeathSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("🔇 Cannot play player death sound: AudioManager.Instance is null");
            return;
        }

        var soundList = AudioManager.Instance.playerDeathSounds;
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("🔇 No player death sounds configured in AudioManager");
            return;
        }

        // Stop movement and attack to prioritize death sound
        if (movementAudioSource != null && movementAudioSource.isPlaying)
            movementAudioSource.Stop();
        if (attackAudioSource != null && attackAudioSource.isPlaying)
            attackAudioSource.Stop();

        string categoryKey = "Player_death";
        bool played = AudioManager.Instance.PlayRandomSoundOnSourceLimited(deathAudioSource, soundList, categoryKey, maxConcurrent: 2);
        if (!played)
            AudioManager.Instance.PlayRandomSoundOnSource(deathAudioSource, soundList);

        Debug.Log("🔊 Player Death Sound");
    }

    public AudioSource GetAttackSource() => attackAudioSource;
    public AudioSource GetMovementSource() => movementAudioSource;
    public AudioSource GetDamageSource() => damageAudioSource;
    public AudioSource GetDeathSource() => deathAudioSource;
}
