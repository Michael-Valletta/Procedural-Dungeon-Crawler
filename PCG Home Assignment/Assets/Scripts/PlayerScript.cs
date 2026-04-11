using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float attackRange = 1.2f;
    public int attackDamage = 10;
    public LayerMask enemyLayers;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.x > 0) sprite.flipX = false;
        else if (moveInput.x < 0) sprite.flipX = true;

        float moveSpeedForAnim = moveInput.magnitude;
        anim.SetFloat("Speed", moveSpeedForAnim);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    void Attack()
    {
        anim.SetTrigger("Attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("We hit " + enemy.name);
            EnemyScript health = enemy.GetComponent<EnemyScript>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}