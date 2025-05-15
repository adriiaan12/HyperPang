using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum Surface { Lurra, Ezkerra, Eskubi, Zapaia }
    Surface currentSurface = Surface.Lurra;

    public float speed = 10f;
    public float climbSpeed = 5f;
    public float jumpForce = 12f;

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

    void Awake()
    {
        lm = FindObjectOfType<LifeManager>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!GameManager.inGame) return;

        // Comprobar si estamos en el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Escalar si estamos en leader y pulsamos W o S
        if (onLeader && Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            isClimbing = true;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero; // Para que no se acumule velocidad al comenzar a escalar
            animator.SetBool("climb",true);
        }
        else if (!onLeader)
        {
            isClimbing = false;
            rb.gravityScale = 1f;
            animator.SetBool("climb",false);
        }

        if (isClimbing)
        {
            float vertical = Input.GetAxisRaw("Vertical");
            rb.linearVelocity = new Vector2(0, vertical * climbSpeed);

            animator.SetFloat("velX", 0);
            animator.SetFloat("velY", vertical);

            return; // No hacemos nada más mientras escalamos
        }

        // Movimiento horizontal y vertical según superficie
        Vector2 moveInput = Vector2.zero;
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

        animator.SetFloat("velX", moveInput.x);
        int velXint = (int)moveInput.x;
        animator.SetInteger("velX", velXint);

        animator.SetFloat("velY", moveInput.y);

        if (moveInput.x < 0) sr.flipX = true;
        else if (moveInput.x > 0) sr.flipX = false;

        rb.MovePosition(rb.position + moveInput * speed * Time.deltaTime);

        // Salto (solo en Lurra, si estamos en suelo y no escalando)
        if (Input.GetKeyDown(KeyCode.W) && isGrounded && currentSurface == Surface.Lurra && !isClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("jump");
        }

        // Disparo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootArrow();
        }

        // Si se acaba el tiempo
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

    void UpdateGravity()
    {
        rb.gravityScale = (currentSurface == Surface.Lurra) ? 1f : 0f;
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
            UpdateGravity();
        }
        else if (collision.CompareTag("ezkerra"))
        {
            currentSurface = Surface.Ezkerra;
            AlignToSurface(currentSurface);
            UpdateGravity();
        }
        else if (collision.CompareTag("eskubi"))
        {
            currentSurface = Surface.Eskubi;
            AlignToSurface(currentSurface);
            UpdateGravity();
        }
        else if (collision.CompareTag("zapaia"))
        {
            currentSurface = Surface.Zapaia;
            AlignToSurface(currentSurface);
            UpdateGravity();
        }

        if (!GameManager.inGame && (collision.CompareTag("ezkerra") || collision.CompareTag("eskubi")))
        {
            sr.flipX = !sr.flipX;
            rb.linearVelocity = -rb.linearVelocity / 3;
            rb.AddForce(Vector3.up * 5, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("leader"))
        {
            onLeader = false;
            isClimbing = false;
            rb.gravityScale = 1f;
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

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}