using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private UnityEngine.UI.Slider hlthSlider;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        hlthSlider.value = currentHealth;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        hlthSlider.value = currentHealth;
        Debug.Log($"Player healed {amount}. Current health: {currentHealth}/{maxHealth}");
    }
    
    void Die()
    {
        Debug.Log("Player died!");
        // Add death logic here (respawn, game over screen, etc.)
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
