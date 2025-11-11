using System;
using System.Runtime.Serialization;
using UnityEngine;

public class EmittorController : MonoBehaviour
{
    AudioSource emittor;

    private void Start()
    {
        emittor = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StopAudio();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            StartAudio();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ResumeAudio();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            PauseAudio();
        }
    }

    private void PauseAudio()
    {
        if (emittor.isPlaying)
        {
            emittor.Pause();
        }
    }

    private void ResumeAudio()
    {
        if (!emittor.isPlaying)
        {
            emittor.UnPause();
        }
    }

    private void StartAudio()
    {
        if (!emittor.isPlaying)
        {
            emittor.Play();
        }
    }

    private void StopAudio()
    {
        if (emittor.isPlaying)
        {
            emittor.Stop();
        }
    }
}
