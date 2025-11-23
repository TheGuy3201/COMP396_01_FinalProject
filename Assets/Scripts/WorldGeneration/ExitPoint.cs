using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class ExitPoint : MonoBehaviour
{
    [Tooltip("Name of the scene to load when the player reaches this exit point.")]
    public string nextLevelName;

    // Called for non-trigger physics collisions
    public void OnTriggerEnter(Collider other) // Detect collision with player to load next level
    {
        Debug.Log("ExitPoint: OnTriggerEnter with " + other.gameObject.name);
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player reached exit point (collision). Loading next level: " + nextLevelName);
            //AudioManager.Play("Teleport");
            SceneManager.LoadScene(nextLevelName);
        }
    }
}
