using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum Surface { Suelo, Izquierda, Derecha, Techo }
    public Surface currentSurface = Surface.Suelo;

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
            animator.SetBool("climb", true);
        }
        else if (!onLeader)
        {
            isClimbing = false;
            rb.gravityScale = 1f;
            animator.SetBool("climb", false);
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
            case Surface.Suelo:
                moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
                break;
            case Surface.Techo:
                moveInput = new Vector2(-Input.GetAxisRaw("Horizontal"), 0);
                break;
            case Surface.Izquierda:
                moveInput = new Vector2(0, -Input.GetAxisRaw("Horizontal"));
                break;
            case Surface.Derecha:
                moveInput = new Vector2(0, Input.GetAxisRaw("Horizontal"));
                break;
        }

        float animatorVel = 0f;

        switch (currentSurface)
        {
            case Surface.Suelo:
                animatorVel = moveInput.x;
                break;
            case Surface.Techo:
                animatorVel = -moveInput.x; // invertido para techo
                break;
            case Surface.Izquierda:
                animatorVel = -moveInput.y; // movimiento hacia arriba = derecha visualmente
                break;
            case Surface.Derecha:
                animatorVel = moveInput.y;
                break;
        }

        animator.SetFloat("velX", animatorVel);
        animator.SetInteger("velX", (int)animatorVel);

        animator.SetFloat("velY", moveInput.y);

        switch (currentSurface)
        {
            case Surface.Suelo: // Suelo normal
                sr.flipX = moveInput.x < 0;
                break;

            case Surface.Techo: // Techo - invertir la lógica
                sr.flipX = moveInput.x > 0;
                break;

            case Surface.Izquierda: // Pared izquierda
                sr.flipX = moveInput.y > 0;
                break;

            case Surface.Derecha: // Pared derecha
                sr.flipX = moveInput.y < 0;
                break;
        }


        rb.MovePosition(rb.position + moveInput * speed * Time.deltaTime);

        // Salto (solo en Suelo, si estamos en suelo y no escalando)
        if (Input.GetKeyDown(KeyCode.W) && isGrounded && currentSurface == Surface.Suelo && !isClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("jump");
        }



        // Si se acaba el tiempo
        if (GameManager.gm.time < 0)
        {
            StartCoroutine(Lose());
        }
    }


    void AlignToSurface(Surface surface)
    {
        switch (surface)
        {
            case Surface.Suelo:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Surface.Izquierda:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case Surface.Derecha:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case Surface.Techo:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
        }
    }

    void UpdateGravity()
    {
        rb.gravityScale = (currentSurface == Surface.Suelo) ? 1f : 0f;
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

        if (collision.CompareTag("Suelo"))
        {
            currentSurface = Surface.Suelo;
            AlignToSurface(currentSurface);
            UpdateGravity();
        }
        else if (collision.CompareTag("Izquierda"))
        {
            currentSurface = Surface.Izquierda;
            AlignToSurface(currentSurface);
            UpdateGravity();
        }
        else if (collision.CompareTag("Derecha"))
        {
            currentSurface = Surface.Derecha;
            AlignToSurface(currentSurface);
            UpdateGravity();
        }
        else if (collision.CompareTag("Techo"))
        {
            currentSurface = Surface.Techo;
            AlignToSurface(currentSurface);
            UpdateGravity();
        }

        if (!GameManager.inGame && (collision.CompareTag("Izquierda") || collision.CompareTag("Derecha")))
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