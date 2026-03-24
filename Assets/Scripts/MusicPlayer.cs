using UnityEngine;

/// <summary>
/// A simple persistent music player. Attach this to an empty GameObject in your Main Menu scene.
/// It will keep playing continuously even when the game scene restarts or transitions.
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    [Header("Audio Settings")]
    public AudioClip backgroundMusicClip;
    [Range(0f, 1f)]
    public float volume = 0.3f;

    void Awake()
    {
        // Singleton pattern to ensure music doesn't overlap when returning to the Main Menu
        // or reloading the game scene.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep alive across all scenes
            
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = backgroundMusicClip;
            source.loop = true;
            source.volume = volume;
            source.playOnAwake = true;
            source.Play();
        }
        else
        {
            // If one already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }
}
