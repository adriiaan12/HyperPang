using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    enum Surface { Lurra, Ezkerra, Eskubi, Zapaia }
    Surface currentSurface = Surface.Lurra;

    public float speed = 10f;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer sr;
    LifeManager lm;

    public GameObject shield;
    public bool blink;

    void Awake()
    {
        lm = FindObjectOfType<LifeManager>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (GameManager.inGame)
        {
            Vector2 moveInput = Vector2.zero;

            // Movimiento según superficie actual
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
            animator.SetFloat("velY", moveInput.y);

            if (moveInput.x < 0)
                sr.flipX = true;
            else if (moveInput.x > 0)
                sr.flipX = false;

            rb.MovePosition(rb.position + moveInput * speed * Time.deltaTime);

            if (GameManager.gm.time < 0)
            {
                StartCoroutine(Lose());
            }
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
        // Solo el suelo tiene gravedad
        rb.gravityScale = (currentSurface == Surface.Lurra) ? 1f : 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.inGame && !FreezeManager.fm.freeze)
        {
            // Comprobamos que no sea la misma superficie
            if (collision.CompareTag("lurra") && currentSurface != Surface.Lurra)
            {
                currentSurface = Surface.Lurra;
                AlignToSurface(currentSurface);
                UpdateGravity();
            }
            else if (collision.CompareTag("ezkerra") && currentSurface != Surface.Ezkerra)
            {
                currentSurface = Surface.Ezkerra;
                AlignToSurface(currentSurface);
                UpdateGravity();
            }
            else if (collision.CompareTag("eskubi") && currentSurface != Surface.Eskubi)
            {
                currentSurface = Surface.Eskubi;
                AlignToSurface(currentSurface);
                UpdateGravity();
            }
            else if (collision.CompareTag("zapaia") && currentSurface != Surface.Zapaia)
            {
                currentSurface = Surface.Zapaia;
                AlignToSurface(currentSurface);
                UpdateGravity();
            }
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

        // Cambiar superficie
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

        // Rebote fuera de juego
        if (!GameManager.inGame && (collision.CompareTag("ezkerra") || collision.CompareTag("eskubi")))
        {
            sr.flipX = !sr.flipX;
            rb.linearVelocity = -rb.linearVelocity / 3;
            rb.AddForce(Vector3.up * 5, ForceMode2D.Impulse);
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