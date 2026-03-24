using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    public AudioClip backgroundMusicClip;
    public float volume = 0.3f;

    void Awake()
    {
        // keep music playing between scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
            
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = backgroundMusicClip;
            source.loop = true;
            source.volume = volume;
            source.playOnAwake = true;
            source.Play();
        }
        else
        {
            Destroy(gameObject); // kill duplicate
        }
    }
}
