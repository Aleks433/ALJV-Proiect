using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int health;
    public int maxHealth;
    public Transform healthBarForeground;
    void Start()
    {
        health = maxHealth;
    }
    public void TakeDamage(int damage)
    {
        health -= damage; 
        if(health <= 0)
        {
            GetComponent<PlayerMovement>().enabled = false;
        }
    }
    void Update()
    {
        float healthPercentage = ((float)health / maxHealth);
        healthBarForeground.localScale = new Vector3(healthPercentage, 1, 1);
    }
}
