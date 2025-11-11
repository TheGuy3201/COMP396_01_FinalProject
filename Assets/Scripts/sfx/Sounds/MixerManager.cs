using UnityEngine;
using UnityEngine.Audio;

public class MixerManager : MonoBehaviour
{
    public static MixerManager instance;
    float masterVolume = 5f;
    float musicVolume = 5.5f;
    private float sfxVolume = 5f;

    [SerializeField] AudioMixer mixer;

    public float GetMasterVolume() => masterVolume;
    public float SetMasterVolume(float vol) => masterVolume = vol;

    public float GetMusicVolume() => musicVolume;
    public float SetMusicVolume(float vol) => musicVolume = vol;

    public float GetSFXVolume() => sfxVolume;
    public float SetSFXVolume(float vol) => sfxVolume = vol;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMixer_Master()
    {
        mixer.SetFloat("MasterVol", masterVolume);
    }

    public void SetMixer_Music()
    {
        mixer.SetFloat("BackgroundMusicVol", musicVolume);
    }

    public void SetMixer_SFX()
    {
        mixer.SetFloat("SFXVol", sfxVolume);
    }
}
