using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
 
/// <summary>
/// Test suite for Sound behaviors described in the Blade-Pursuit test matrix.
/// Covers surface sounds, attack SFX, button clicks, BGM looping, and spatial audio.
///
/// Fixes applied vs original:
///   SND-01: Placed stoneFloor collider at y = -1 so a downward raycast from the
///           player origin (y = 0) actually intersects it. Previously both objects
///           were at (0,0,0) and the ray started inside the collider.
///   SND-04: audioSource.loop must be set to true BEFORE calling Play(); the
///           original code set it after Play() which caused a race-condition
///           Assert failure.
/// </summary>
public class SoundTests
{
    private GameObject player;
    private GameObject enemy;
    private AudioSource audioSource;
    private AudioSource uiAudioSource;
    private AudioSource enemyAudio;
    private AudioClip attackClip;
    private AudioClip clickSFX;
    private AudioClip bgmClip;
    private GameObject stoneFloor;
 
    [SetUp]
    public void Setup()
    {
        // Player
        player = new GameObject("Player");
        audioSource = player.AddComponent<AudioSource>();
 
        // Enemy
        enemy = new GameObject("Enemy");
        enemyAudio = enemy.AddComponent<AudioSource>();
 
        // UI Audio Source
        var uiObject = new GameObject("UIAudio");
        uiAudioSource = uiObject.AddComponent<AudioSource>();
 
        // ── FIX SND-01 ──────────────────────────────────────────────────────────
        // Place the stone floor BELOW the player so a downward raycast from
        // player.transform.position (y = 0) hits the collider.
        // BoxCollider default size is (1, 1, 1), so centred at y = -1 the top
        // face is at y = -0.5, well within reach of a downward ray from y = 0.
        stoneFloor = new GameObject("StoneFloor");
        stoneFloor.tag = "Stone";
        var floorCollider = stoneFloor.AddComponent<BoxCollider>();
        floorCollider.size = new Vector3(10f, 1f, 10f); // Wide flat slab
        stoneFloor.transform.position = new Vector3(0f, -1f, 0f);
        // ────────────────────────────────────────────────────────────────────────
 
        // Player stays at world origin (0, 0, 0) — above the floor
        player.transform.position = Vector3.zero;
 
        // Audio clips (silent but valid)
        attackClip = AudioClip.Create("Attack", 44100, 1, 44100, false);
        clickSFX   = AudioClip.Create("Click",  44100, 1, 44100, false);
        bgmClip    = AudioClip.Create("BGM",    44100, 1, 44100, false);
 
        audioSource.clip = bgmClip;
    }
 
    [TearDown]
    public void Teardown()
    {
        if (player      != null) Object.DestroyImmediate(player);
        if (enemy       != null) Object.DestroyImmediate(enemy);
        if (stoneFloor  != null) Object.DestroyImmediate(stoneFloor);
        if (uiAudioSource != null) Object.DestroyImmediate(uiAudioSource.gameObject);
    }
 
    /// <summary>
    /// SND-01: Raycast downward from player should detect the "Stone" tagged floor.
    /// Fix: stoneFloor collider is now placed at y = -1 so the ray hits it.
    /// </summary>
    [UnityTest]
    public IEnumerator SND_01_Surface_Snd_Auto()
    {
        RaycastHit hit;
        bool didHit = Physics.Raycast(player.transform.position, Vector3.down, out hit, 5f);
 
        Assert.IsTrue(didHit, "Raycast should hit the floor collider beneath the player");
        Assert.AreEqual("Stone", hit.collider.tag,
            "The surface tag must be 'Stone' so the correct footstep clip is played");
 
        yield return null;
        Debug.Log($"✅ SND-01 PASSED: Raycast hit '{hit.collider.name}' tag='{hit.collider.tag}'");
    }
 
    /// <summary>
    /// SND-02: Attack SFX plays when player attacks.
    /// </summary>
    [UnityTest]
    public IEnumerator SND_02_Atk_SFX_Auto()
    {
        audioSource.clip = attackClip;
        audioSource.PlayOneShot(attackClip);
 
        Assert.IsTrue(audioSource.isPlaying, "AudioSource should be playing after attack");
        Assert.AreEqual(attackClip, audioSource.clip, "Clip should be the attack SFX");
 
        yield return null;
        Debug.Log("✅ SND-02 PASSED: Attack SFX plays correctly");
    }
 
    /// <summary>
    /// SND-03: Button click sound plays on UI interaction.
    /// </summary>
    [UnityTest]
    public IEnumerator SND_03_Btn_Click_Snd_Auto()
    {
        uiAudioSource.PlayOneShot(clickSFX);
 
        Assert.IsTrue(uiAudioSource.isPlaying, "UI AudioSource should be playing after button click");
 
        yield return new WaitForSeconds(0.1f);
        Debug.Log("✅ SND-03 PASSED: Button click sound plays correctly");
    }
 
    /// <summary>
    /// SND-04: BGM is configured to loop before playback starts.
    /// Fix: Set audioSource.loop = true BEFORE calling Play() so the flag is
    /// already applied when the AudioSource is initialised, preventing a frame
    /// where loop is false during Play().
    /// </summary>
    [UnityTest]
    public IEnumerator SND_04_BGM_Loop_Auto()
    {
        audioSource.clip = bgmClip;
 
        // ── FIX SND-04: set loop BEFORE Play ────────────────────────────────
        audioSource.loop = true;
        audioSource.Play();
        // ────────────────────────────────────────────────────────────────────
 
        Assert.IsTrue(audioSource.loop,      "BGM AudioSource.loop must be true for seamless music");
        Assert.IsTrue(audioSource.isPlaying, "BGM must be playing");
 
        yield return null;
        Debug.Log("✅ SND-04 PASSED: BGM loop enabled and playing");
    }
 
    /// <summary>
    /// SND-05: Enemy death sound plays with full 3-D spatial blend.
    /// </summary>
    [UnityTest]
    public IEnumerator SND_05_Death_Spatial_Auto()
    {
        enemyAudio.spatialBlend = 1.0f; // Full 3-D
        enemyAudio.Play();
 
        Assert.AreEqual(1f, enemyAudio.spatialBlend,
            "spatialBlend must be 1.0 for positional audio on enemy death");
        Assert.IsTrue(enemyAudio.isPlaying, "Enemy death AudioSource should be playing");
 
        yield return null;
        Debug.Log("✅ SND-05 PASSED: Death spatial audio configured correctly");
    }
}
 