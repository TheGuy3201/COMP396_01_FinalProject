using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class ExitPoint : MonoBehaviour
{
    [Tooltip("Name of the scene to load when the player reaches this exit point.")]
    public string nextLevelName;

    // Called for non-trigger physics collisions (requires Rigidbody on either object)
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("ExitPoint: OnCollisionEnter with " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player reached exit point (collision). Loading next level: " + nextLevelName);
            //AudioManager.Play("Teleport");
            SceneManager.LoadScene(nextLevelName);
        }
    }

    // Called when this collider is set as 'isTrigger' and another collider enters it
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("ExitPoint: OnTriggerEnter with " + other.gameObject.name);
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player reached exit point (trigger). Loading next level: " + nextLevelName);
            //AudioManager.Play("Teleport");
            SceneManager.LoadScene(nextLevelName);
        }
    }
}
