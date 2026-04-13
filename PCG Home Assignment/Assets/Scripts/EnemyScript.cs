using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Combat Settings")]
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public float attackRate = 1f;
    private float nextAttackTime = 0f;
    public int damage = 5;

    private Transform player;
    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (LevelManager.Instance != null) LevelManager.Instance.RegisterEnemy();
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void AttackPlayer()
    {
        anim.Play("Enemy 1 Attacking");
        PlayerScript playerStats = player.GetComponent<PlayerScript>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took damage! HP: " + currentHealth);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.Play("Enemy 1 Dying");

        if (LevelManager.Instance != null) LevelManager.Instance.EnemyDied();
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>() != null) GetComponent<Rigidbody2D>().simulated = false;
        Destroy(gameObject, 2.0f);
    }
}