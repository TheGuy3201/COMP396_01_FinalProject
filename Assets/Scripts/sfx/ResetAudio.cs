using UnityEngine;

public class ResetAudio : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        AudioManager.StopAllSounds();
    }

}