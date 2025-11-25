using System.Collections;
using UnityEngine;

public class CoinsManager : MonoBehaviour
{
    private TMPro.TextMeshProUGUI coinsText;
    private int totalCoins = 0;

    void Start()
    {
        StartCoroutine(GetCoinsTextAfterDelay(0.5f));
        Debug.Log("CoinsManager: Start");
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        coinsText.text = totalCoins.ToString();
        Debug.Log($"Coins collected: {totalCoins}");
    }

    public int GetTotalCoins()
    {
        return totalCoins;
    }

    IEnumerator GetCoinsTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        coinsText = GameObject.FindWithTag("CoinsText").GetComponent<TMPro.TextMeshProUGUI>();
        coinsText.text = totalCoins.ToString();
    }
}