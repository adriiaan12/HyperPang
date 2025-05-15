using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum Surface { Lurra, Ezkerra, Eskubi, Zapaia }
    Surface currentSurface = Surface.Lurra;

    public float speed = 10f;
    public float climbSpeed = 8f;
    public float jumpForce = 18f;

    public float gravity = -30f;

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer sr;
    LifeManager lm;

    public GameObject shield;
    public bool blink;

    public GameObject arrowPrefab;
    public Transform shotSpawnPoint;

    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private bool onLeader = false;
    private bool isClimbing = false;
    private bool isGrounded = false;

    private Vector2 velocity;

    void Awake()
    {
        lm = FindObjectOfType<LifeManager>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (!GameManager.inGame) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Vector2 moveInput = Vector2.zero;

        if (onLeader && Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            isClimbing = true;
            velocity.y = Input.GetAxisRaw("Vertical") * climbSpeed;
        }
        else if (isClimbing && !onLeader)
        {
            isClimbing = false;
        }

        if (isClimbing)
        {
            velocity.x = 0;
            animator.SetFloat("velX", 0);
            animator.SetFloat("velY", velocity.y);
            rb.MovePosition(rb.position + new Vector2(0, velocity.y * Time.deltaTime));
            return;
        }

        switch (currentSurface)
        {
            case Surface.Lurra:
                moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
                break;
            case Surface.Zapaia:
                moveInput = new Vector2(-Input.GetAxisRaw("Horizontal"), 0);
                break;
            case Surface.Ezkerra:
                moveInput = new Vector2(0, -Input.GetAxisRaw("Horizontal"));
                break;
            case Surface.Eskubi:
                moveInput = new Vector2(0, Input.GetAxisRaw("Horizontal"));
                break;
        }

        velocity.x = moveInput.x * speed;

        if (Input.GetKeyDown(KeyCode.W) && isGrounded && currentSurface == Surface.Lurra && !isClimbing)
        {
            velocity.y = jumpForce;
            animator.SetTrigger("jump");
        }

        // Aplicar gravedad constante
        if (!isGrounded && !isClimbing)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        Vector2 nextPos = rb.position + velocity * Time.deltaTime;
        rb.MovePosition(nextPos);

        animator.SetFloat("velX", moveInput.x);
        animator.SetFloat("velY", moveInput.y);

        if (moveInput.x < 0) sr.flipX = true;
        else if (moveInput.x > 0) sr.flipX = false;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootArrow();
        }

        if (GameManager.gm.time < 0)
        {
            StartCoroutine(Lose());
        }
    }

    void ShootArrow()
    {
        if (arrowPrefab != null && shotSpawnPoint != null)
        {
            GameObject shot = Instantiate(arrowPrefab, shotSpawnPoint.position, Quaternion.identity);
            ShotArrow arrow = shot.GetComponent<ShotArrow>();
            arrow.SetSurface(currentSurface);
        }
    }

    void AlignToSurface(Surface surface)
    {
        switch (surface)
        {
            case Surface.Lurra:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Surface.Ezkerra:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case Surface.Eskubi:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case Surface.Zapaia:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.inGame && !FreezeManager.fm.freeze)
        {
            if (collision.CompareTag("ball") || collision.CompareTag("Hexagon"))
            {
                if (shield.activeInHierarchy)
                {
                    shield.SetActive(false);
                    StartCoroutine(Blinking());
                }
                else if (!blink)
                {
                    StartCoroutine(Lose());
                }
            }
        }

        if (collision.CompareTag("leader"))
        {
            onLeader = true;
        }

        if (collision.CompareTag("lurra"))
        {
            currentSurface = Surface.Lurra;
            AlignToSurface(currentSurface);
        }
        else if (collision.CompareTag("ezkerra"))
        {
            currentSurface = Surface.Ezkerra;
            AlignToSurface(currentSurface);
        }
        else if (collision.CompareTag("eskubi"))
        {
            currentSurface = Surface.Eskubi;
            AlignToSurface(currentSurface);
        }
        else if (collision.CompareTag("zapaia"))
        {
            currentSurface = Surface.Zapaia;
            AlignToSurface(currentSurface);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("leader"))
        {
            onLeader = false;
            isClimbing = false;
            velocity.y = 0;
        }
    }

    public void Win()
    {
        shield.SetActive(false);
        animator.SetBool("win", true);
    }

    private void OnBecameInvisible()
    {
        Invoke("ReloadLevel", 0.5f);
        if (lm.lifes <= 0)
        {
            SceneManager.LoadScene(0);
        }
    }

    void ReloadLevel()
    {
        lm.SubstractLifes();
        lm.RestartLifesDoll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public IEnumerator Blinking()
    {
        blink = true;
        for (int i = 0; i < 8; i++)
        {
            if (!GameManager.inGame) break;
            sr.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.2f);
            sr.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.2f);
        }
        blink = false;
    }

    public IEnumerator Lose()
    {
        GameManager.inGame = false;
        animator.SetBool("lose", true);
        BallManager.bm.LoseGame();
        HexagonManager.hm.LoseGame();
        lm.LifeLose();

        yield return new WaitForSeconds(1);
        rb.isKinematic = false;

        if (transform.position.x <= 0)
        {
            rb.AddForce(new Vector2(-10, 10), ForceMode2D.Impulse);
        }
        else
        {
            rb.AddForce(new Vector2(10, 10), ForceMode2D.Impulse);
        }
    }
}
