using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float attackRange = 1.2f;
    public int attackDamage = 10;
    public LayerMask enemyLayers;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private Vector2 moveInput;
    private bool isDead = false;

    public Tilemap floorTilemap;
    public TileBase lavaTile;
    public TileBase waterTile;

    private float lavaDamageCooldown = 2.0f;
    private float lastLavaDamageTime;

    void Awake() { currentHealth = maxHealth; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead) { rb.linearVelocity = Vector2.zero; return; }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.x > 0) sprite.flipX = false;
        else if (moveInput.x < 0) sprite.flipX = true;

        anim.SetFloat("Speed", moveInput.magnitude);

        if (Input.GetMouseButtonDown(0)) Attack();
    }

    void FixedUpdate()
    {
        if (isDead || floorTilemap == null) return;

        Vector3Int cellPos = floorTilemap.WorldToCell(transform.position);
        TileBase currentTile = floorTilemap.GetTile(cellPos);
        float currentSpeed = moveSpeed;

        if (currentTile == waterTile) currentSpeed *= 0.5f;

        if (currentTile == lavaTile)
        {
            if (Time.time >= lastLavaDamageTime + lavaDamageCooldown)
            {
                TakeDamage(2);
                lastLavaDamageTime = Time.time;
            }
        }
        rb.linearVelocity = moveInput.normalized * currentSpeed;
    }

    void Attack()
    {
        anim.SetTrigger("Attack");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyScript e = enemy.GetComponent<EnemyScript>();
            if (e != null) e.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        Invoke("GoToGameOver", 2f); 
    }

    void GoToGameOver() { SceneManager.LoadScene("GameOverScene"); }

    public System.Collections.IEnumerator ApplySpeedBoost(float extraSpeed, float time)
    {
        moveSpeed += extraSpeed;
        yield return new WaitForSeconds(time);
        moveSpeed -= extraSpeed;
    }
}