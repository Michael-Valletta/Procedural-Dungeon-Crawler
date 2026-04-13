using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public enum ItemType { Health, Speed }
    public ItemType type;
    public float amount = 20f;
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerScript player = collision.GetComponent<PlayerScript>();
            if (type == ItemType.Health)
            {
                player.currentHealth = Mathf.Min(player.maxHealth, player.currentHealth + (int)amount);
                // Update your UI health text here
            }
            else if (type == ItemType.Speed)
            {
                StartCoroutine(player.ApplySpeedBoost(amount, duration));
            }
            Destroy(gameObject);
        }
    }
}
