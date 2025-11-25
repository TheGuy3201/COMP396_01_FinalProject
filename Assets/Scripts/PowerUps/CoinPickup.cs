using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] int coinValue = 1;

    private CoinsManager coinsManager;

    void Start()
    {
        coinsManager = GameObject.FindWithTag("CoinsManager").GetComponent<CoinsManager>();
        Debug.Log("CoinPickup: Start");
    }
    public void OnTriggerEnter(Collider other) // Detect collision with player to apply speed boost
    {
        Debug.Log("CoinPickup: OnTriggerEnter with " + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            if (coinsManager != null)
            {
                coinsManager.AddCoins(coinValue);
                Destroy(gameObject);
            }
        }
    }
}