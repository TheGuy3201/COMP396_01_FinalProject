using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class Sound
{
    public string name;

    [UnityEngine.Range(0,1)]
    public float volume;

    [UnityEngine.Range(0.1f,3)]
    public float pitch;

    public bool isLoop;

    public AudioClip clip;
    public AudioMixerGroup mixer;
}

public static class AudioManager
{
    static List<AudioSource> sources;
    static Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();
    static int numOfSources = 50;
    static AudioManager()
    {
        sources = new List<AudioSource>();
        for (int i = 0; i < numOfSources; i++)
        {
            GameObject go = new GameObject();
            AudioSource src = go.AddComponent<AudioSource>();
            MonoBehaviour.DontDestroyOnLoad(src);
            sources.Add(src);
            
        }
    }

    public static void Play(string name)
    {
        //get the audiosource
        AudioSource src = GetAudioSource();

        //set properties
        CustomizeAudioSource(src, sounds[name]);

        //Play audio
        src.Play();
    }

    private static void CustomizeAudioSource(AudioSource src, Sound info)
    {
        src.clip = info.clip;
        src.volume = info.volume;
        src.pitch = info.pitch;
        src.loop = info.isLoop;
        src.outputAudioMixerGroup = info.mixer;
    }

    public static void AddSound(Sound s)
    {
        sounds[s.name] = s;
    }

    private static AudioSource GetAudioSource()
    {
        foreach (AudioSource source in sources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        Debug.Log($"Error: No AudioSources available, maybe increase {numOfSources}");
        return null;
    }

    internal static void StopAllSounds()
    {
        foreach (AudioSource source in sources)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }
}