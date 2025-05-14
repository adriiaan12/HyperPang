using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player; // Para usar Player.Surface

public class ShotArrow : MonoBehaviour
{
    float speed = 4f;

    public GameObject chainGFX;

    Vector2 startPos;
    Vector2 direction = Vector2.up;

    void Start()
    {
        startPos = transform.position;

        if (chainGFX != null)
        {
            GameObject chain = Instantiate(chainGFX, transform.position, Quaternion.identity);
            chain.transform.parent = transform;
        }
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, startPos) >= 0.2f)
        {
            if (chainGFX != null)
            {
                GameObject chain = Instantiate(chainGFX, transform.position, Quaternion.identity);
                chain.transform.parent = transform;
            }

            startPos = transform.position;
        }
    }

    public void SetSurface(Surface surface)
    {
        switch (surface)
        {
            case Surface.Lurra:
                direction = Vector2.up;
                break;
            case Surface.Zapaia:
                direction = Vector2.down;
                break;
            case Surface.Ezkerra:
                direction = Vector2.right;
                break;
            case Surface.Eskubi:
                direction = Vector2.left;
                break;
        }

        // Rota el sprite para que apunte hacia la dirección
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Contains("DestroyBall"))
        {
            BallManager.bm.Dinamite(6);
        }

        if (collision.gameObject.name.Contains("StopBall"))
        {
            FreezeManager.fm.StartFreeze(6);
        }

        if (collision.CompareTag("ball"))
        {
            collision.GetComponent<Ball>().Split();
        }

        if (collision.CompareTag("Hexagon"))
        {
            collision.GetComponent<Hexagon>().Split();
        }

        if (!collision.CompareTag("Player") && !collision.CompareTag("leader"))
        {
            Destroy(gameObject);
            ShootManager.shm.DestroyShot();
        }
    }
}