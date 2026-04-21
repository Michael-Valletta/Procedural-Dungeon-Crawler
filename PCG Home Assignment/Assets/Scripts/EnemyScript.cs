using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyScript : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Combat Settings")]
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float attackRate = 1f;
    private float nextAttackTime = 0f;
    public int damage = 5;

    [Header("Animation Names")]
    public string attackAnimationName = "Enemy 1 Attacking";
    public string dieAnimationName = "Enemy 1 Dying";

    private Transform player;
    private Animator anim;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterEnemy();
        }
        else
        {
            Debug.LogError("Enemy spawned but LevelManager Instance is null");
        }
    }

    void FixedUpdate() 
    {
        if (player == null || isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.SetFloat("Speed", 0);

            if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - (Vector3)rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        if (sprite != null) sprite.flipX = direction.x < 0;
        if (anim != null) anim.SetFloat("Speed", moveSpeed);

        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
    }

    void AttackPlayer()
    {
        if (anim != null) anim.Play(attackAnimationName);
        PlayerScript pStats = player.GetComponent<PlayerScript>();
        if (pStats != null) pStats.TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.Play(dieAnimationName);

        if (LevelManager.Instance != null) LevelManager.Instance.EnemyDied();

        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1.5f);
    }
}