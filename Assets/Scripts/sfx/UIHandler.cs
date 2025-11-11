using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class UIHandler : MonoBehaviour
{
    [SerializeField] MixerManager mixerMngr;
    [SerializeField] Slider masterVolSLDR;
    [SerializeField] Slider musicVolSLDR;
    [SerializeField] Slider sfxVolSLDR;

    private float ogMasterVol;
    private float ogMusicVol;
    private float ogSfxVol;

    private void Start()
    {
        //Read original values
        ogMasterVol = mixerMngr.GetMasterVolume();
        ogMusicVol = mixerMngr.GetMusicVolume();
        ogSfxVol = mixerMngr.GetSFXVolume();

        //Set ui to show these values
        masterVolSLDR.value = ogMasterVol;
        musicVolSLDR.value = ogMusicVol;
        sfxVolSLDR.value = ogSfxVol;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void UpdateAudioSettings()
    {
        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }

    private void SetMasterVolume()
    {
        mixerMngr.SetMasterVolume(masterVolSLDR.value);
        ogMasterVol = masterVolSLDR.value;
        mixerMngr.SetMixer_Master();
    }

    private void SetSFXVolume()
    {
        mixerMngr.SetSFXVolume(sfxVolSLDR.value);
        ogSfxVol = sfxVolSLDR.value;
        mixerMngr.SetMixer_SFX();
    }
    private void SetMusicVolume()
    {
        mixerMngr.SetMusicVolume(musicVolSLDR.value);
        ogMusicVol = musicVolSLDR.value;
        mixerMngr.SetMixer_Music();
    }
}
