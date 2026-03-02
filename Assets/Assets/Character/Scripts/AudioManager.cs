using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
        public bool loop = false;
    }

    private static AudioManager instance;
    private Dictionary<string, AudioSource> activeSources = new Dictionary<string, AudioSource>();
    // Track concurrent plays per category (e.g., "Skeleton_Movement") to avoid too many overlapping sounds
    private Dictionary<string, int> concurrentPlays = new Dictionary<string, int>();
    
    public List<SoundEffect> playerAttackSounds = new List<SoundEffect>();
    public List<SoundEffect> playerMovementSounds = new List<SoundEffect>();
    public List<SoundEffect> playerDamageSounds = new List<SoundEffect>();
    public List<SoundEffect> playerDeathSounds = new List<SoundEffect>();
    
    public List<SoundEffect> skeletonAttackSounds = new List<SoundEffect>();
    public List<SoundEffect> skeletonMovementSounds = new List<SoundEffect>();
    public List<SoundEffect> skeletonDamageSounds = new List<SoundEffect>();
    public List<SoundEffect> skeletonDeathSounds = new List<SoundEffect>();
    
    public List<SoundEffect> flyAttackSounds = new List<SoundEffect>();
    public List<SoundEffect> flyMovementSounds = new List<SoundEffect>();
    public List<SoundEffect> flyDamageSounds = new List<SoundEffect>();
    public List<SoundEffect> flyDeathSounds = new List<SoundEffect>();
    
    public List<SoundEffect> tankAttackSounds = new List<SoundEffect>();
    public List<SoundEffect> tankMovementSounds = new List<SoundEffect>();
    public List<SoundEffect> tankDamageSounds = new List<SoundEffect>();
    public List<SoundEffect> tankDeathSounds = new List<SoundEffect>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Play a sound at a specific world position with spatial audio
    /// </summary>
    public AudioSource PlaySoundAtPosition(string soundName, Vector3 position, List<SoundEffect> soundList, float spatialBlend = 1f)
    {
        SoundEffect sound = GetSound(soundName, soundList);
        if (sound == null || sound.clip == null)
        {
            Debug.LogWarning($"❌ Sound '{soundName}' not found!");
            return null;
        }

        GameObject audioObj = new GameObject($"Audio_{soundName}");
        audioObj.transform.position = position;
        
        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.spatialBlend = spatialBlend; // 1 = fully 3D spatial
        source.Play();

        // Clean up after sound finishes
        Destroy(audioObj, sound.clip.length / sound.pitch);
        
        return source;
    }

    /// <summary>
    /// Play a sound on a specific AudioSource (for characters with their own audio source)
    /// </summary>
    public void PlaySoundOnSource(AudioSource source, string soundName, List<SoundEffect> soundList)
    {
        if (source == null) return;

        SoundEffect sound = GetSound(soundName, soundList);
        if (sound == null || sound.clip == null)
        {
            Debug.LogWarning($"❌ Sound '{soundName}' not found!");
            return;
        }

        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.loop = sound.loop;
        source.Play();
    }

    /// <summary>
    /// Play a random sound from the list
    /// </summary>
    public AudioSource PlayRandomSoundAtPosition(Vector3 position, List<SoundEffect> soundList, float spatialBlend = 1f)
    {
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("❌ Sound list is empty!");
            return null;
        }

        SoundEffect sound = soundList[Random.Range(0, soundList.Count)];
        return PlaySoundAtPosition(sound.name, position, soundList, spatialBlend);
    }

    /// <summary>
    /// Play a random sound on specific source
    /// </summary>
    public void PlayRandomSoundOnSource(AudioSource source, List<SoundEffect> soundList)
    {
        if (source == null || soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("❌ Invalid source or empty sound list!");
            return;
        }

        SoundEffect sound = soundList[Random.Range(0, soundList.Count)];
        PlaySoundOnSource(source, sound.name, soundList);
    }

    /// <summary>
    /// Play a random sound on specific source but limit concurrent plays for a given category key.
    /// Returns true if the sound was played, false if skipped due to limit.
    /// </summary>
    public bool PlayRandomSoundOnSourceLimited(AudioSource source, List<SoundEffect> soundList, string categoryKey, int maxConcurrent = 3)
    {
        if (source == null || soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("❌ Invalid source or empty sound list!");
            return false;
        }

        if (!concurrentPlays.ContainsKey(categoryKey))
            concurrentPlays[categoryKey] = 0;

        if (concurrentPlays[categoryKey] >= maxConcurrent)
        {
            // Too many similar sounds playing right now
            return false;
        }

        SoundEffect sound = soundList[Random.Range(0, soundList.Count)];
        PlaySoundOnSource(source, sound.name, soundList);

        // Increase counter and schedule decrement after clip finishes (conservative based on clip length / pitch)
        concurrentPlays[categoryKey]++;
        float delay = 0f;
        if (sound.clip != null && sound.pitch != 0f)
            delay = sound.clip.length / Mathf.Abs(sound.pitch);
        else if (sound.clip != null)
            delay = sound.clip.length;

        StartCoroutine(DecrementConcurrentAfterDelay(categoryKey, delay));
        return true;
    }

    private IEnumerator DecrementConcurrentAfterDelay(string categoryKey, float delay)
    {
        if (delay <= 0f) yield break;
        yield return new WaitForSeconds(delay);
        if (concurrentPlays.ContainsKey(categoryKey))
        {
            concurrentPlays[categoryKey] = Mathf.Max(0, concurrentPlays[categoryKey] - 1);
        }
    }

    /// <summary>
    /// Stop a sound by name (for looping sounds)
    /// </summary>
    public void StopSound(string soundName)
    {
        if (activeSources.ContainsKey(soundName))
        {
            Destroy(activeSources[soundName].gameObject);
            activeSources.Remove(soundName);
        }
    }

    private SoundEffect GetSound(string soundName, List<SoundEffect> soundList)
    {
        foreach (SoundEffect sound in soundList)
        {
            if (sound.name == soundName)
                return sound;
        }
        return null;
    }

    public static AudioManager Instance
    {
        get { return instance; }
    }
}
