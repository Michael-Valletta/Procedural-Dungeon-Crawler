using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public bool isHealthPotion;
    public bool isSpeedBall;
    public int healthAmount = 20;
    public float speedBoost = 4f;
    public float boostDuration = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerScript player = collision.GetComponent<PlayerScript>();

            if (isHealthPotion)
            {
                if (player.currentHealth >= player.maxHealth) return;

                player.currentHealth = Mathf.Min(player.currentHealth + healthAmount, player.maxHealth);
                Destroy(gameObject);
            }
            else if (isSpeedBall)
            {
                player.StartCoroutine(player.ApplySpeedBoost(speedBoost, boostDuration));
                Destroy(gameObject);
            }
        }
    }
}