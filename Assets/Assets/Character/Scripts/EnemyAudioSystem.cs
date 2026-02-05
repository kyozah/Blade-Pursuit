using UnityEngine;

public class EnemyAudioSystem : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private AudioSource movementAudioSource;
    [SerializeField] private AudioSource damageAudioSource;
    [SerializeField] private AudioSource deathAudioSource;

    [Header("Enemy Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Skeleton;

    [Header("Settings")]
    [SerializeField] private bool useRandomPitch = true;
    [SerializeField] private float pitchVariation = 0.15f;
    [SerializeField] private float movementSoundInterval = 0.5f;

    private Enemy enemyScript;
    private float lastMovementSoundTime;
    private Vector3 previousPosition;

    public enum EnemyType { Skeleton, Fly, Tank }

    void Start()
    {
        enemyScript = GetComponent<Enemy>();

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
        ConfigureAudioSource(attackAudioSource, spatialBlend: 0.7f);
        ConfigureAudioSource(movementAudioSource, spatialBlend: 0.5f);
        ConfigureAudioSource(damageAudioSource, spatialBlend: 0.7f);
        ConfigureAudioSource(deathAudioSource, spatialBlend: 0.7f);

        lastMovementSoundTime = -movementSoundInterval;
        previousPosition = transform.position;

        Debug.Log($"🔊 {enemyType} Audio System initialized");
    }

    void Update()
    {
        CheckMovement();
    }

    private void ConfigureAudioSource(AudioSource source, bool loop = false, float spatialBlend = 1f)
    {
        source.spatialBlend = spatialBlend;
        source.loop = loop;
        source.playOnAwake = false;
    }

    private void CheckMovement()
    {
        if (enemyScript != null && enemyScript.IsDead)
            return;

        // Calculate movement
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);

        // Play movement sound periodically while moving
        if (distanceMoved > 0.01f && Time.time - lastMovementSoundTime >= movementSoundInterval)
        {
            PlayMovementSound();
            lastMovementSoundTime = Time.time;
        }

        previousPosition = transform.position;
    }

    public void PlayAttackSound()
    {
        if (AudioManager.Instance == null) return;

        System.Collections.Generic.List<AudioManager.SoundEffect> soundList = GetAttackSoundList();
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning($"No attack sounds configured for {enemyType}");
            return;
        }

        // If movement sound is playing on this enemy, stop it so attack sound is not drowned.
        if (movementAudioSource != null && movementAudioSource.isPlaying)
        {
            movementAudioSource.Stop();
        }

        // Ensure attack source is not currently playing (avoid overwriting unintentionally)
        if (attackAudioSource != null && attackAudioSource.isPlaying)
        {
            attackAudioSource.Stop();
        }

        // Play attack sound. Use limited global plays for attack category but ensure attacks always play locally.
        string categoryKey = $"{enemyType}_attack";
        bool played = AudioManager.Instance.PlayRandomSoundOnSourceLimited(attackAudioSource, soundList, categoryKey, maxConcurrent: 8);
        if (!played)
        {
            // If limiter prevented global play, still play locally to preserve attack feedback
            AudioManager.Instance.PlayRandomSoundOnSource(attackAudioSource, soundList);
        }

        if (useRandomPitch)
        {
            attackAudioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }

        Debug.Log($"🔊 {enemyType} Attack Sound");
    }

    private void PlayMovementSound()
    {
        if (AudioManager.Instance == null) return;

        System.Collections.Generic.List<AudioManager.SoundEffect> soundList = GetMovementSoundList();
        if (soundList == null || soundList.Count == 0)
            return;

        // Prevent the same enemy from starting the same movement sound if its source is already playing
        if (movementAudioSource.isPlaying)
            return;

        // Use a global limiter to avoid too many overlapping movement sounds of the same category
        string categoryKey = $"{enemyType}_movement";
        bool played = AudioManager.Instance.PlayRandomSoundOnSourceLimited(movementAudioSource, soundList, categoryKey, maxConcurrent: 4);

        if (played && useRandomPitch)
        {
            movementAudioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }
    }

    public void PlayDamageSound()
    {
        if (AudioManager.Instance == null) return;

        System.Collections.Generic.List<AudioManager.SoundEffect> soundList = GetDamageSoundList();
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning($"No damage sounds configured for {enemyType}");
            return;
        }

        // Stop movement to prioritize damage sound
        if (movementAudioSource != null && movementAudioSource.isPlaying)
            movementAudioSource.Stop();

        // Play damage sound, prefer limited global plays but fallback to local play
        string categoryKey = $"{enemyType}_damage";
        bool played = AudioManager.Instance.PlayRandomSoundOnSourceLimited(damageAudioSource, soundList, categoryKey, maxConcurrent: 6);
        if (!played)
            AudioManager.Instance.PlayRandomSoundOnSource(damageAudioSource, soundList);

        if (useRandomPitch)
        {
            damageAudioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }

        Debug.Log($"🔊 {enemyType} Damage Sound");
    }

    public void PlayDeathSound()
    {
        if (AudioManager.Instance == null) return;

        System.Collections.Generic.List<AudioManager.SoundEffect> soundList = GetDeathSoundList();
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning($"No death sounds configured for {enemyType}");
            return;
        }

        // Stop movement and attack to ensure death sound is audible
        if (movementAudioSource != null && movementAudioSource.isPlaying)
            movementAudioSource.Stop();
        if (attackAudioSource != null && attackAudioSource.isPlaying)
            attackAudioSource.Stop();

        string categoryKey = $"{enemyType}_death";
        bool played = AudioManager.Instance.PlayRandomSoundOnSourceLimited(deathAudioSource, soundList, categoryKey, maxConcurrent: 4);
        if (!played)
            AudioManager.Instance.PlayRandomSoundOnSource(deathAudioSource, soundList);

        if (useRandomPitch)
        {
            deathAudioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }

        Debug.Log($"🔊 {enemyType} Death Sound");
    }

    private System.Collections.Generic.List<AudioManager.SoundEffect> GetAttackSoundList()
    {
        return enemyType switch
        {
            EnemyType.Skeleton => AudioManager.Instance.skeletonAttackSounds,
            EnemyType.Fly => AudioManager.Instance.flyAttackSounds,
            EnemyType.Tank => AudioManager.Instance.tankAttackSounds,
            _ => null
        };
    }

    private System.Collections.Generic.List<AudioManager.SoundEffect> GetMovementSoundList()
    {
        return enemyType switch
        {
            EnemyType.Skeleton => AudioManager.Instance.skeletonMovementSounds,
            EnemyType.Fly => AudioManager.Instance.flyMovementSounds,
            EnemyType.Tank => AudioManager.Instance.tankMovementSounds,
            _ => null
        };
    }

    private System.Collections.Generic.List<AudioManager.SoundEffect> GetDamageSoundList()
    {
        return enemyType switch
        {
            EnemyType.Skeleton => AudioManager.Instance.skeletonDamageSounds,
            EnemyType.Fly => AudioManager.Instance.flyDamageSounds,
            EnemyType.Tank => AudioManager.Instance.tankDamageSounds,
            _ => null
        };
    }

    private System.Collections.Generic.List<AudioManager.SoundEffect> GetDeathSoundList()
    {
        return enemyType switch
        {
            EnemyType.Skeleton => AudioManager.Instance.skeletonDeathSounds,
            EnemyType.Fly => AudioManager.Instance.flyDeathSounds,
            EnemyType.Tank => AudioManager.Instance.tankDeathSounds,
            _ => null
        };
    }
}
