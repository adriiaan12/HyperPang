using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player; // Para usar Player.Surface

public class ShotGun : MonoBehaviour
{
    float speed = 8f;
    Vector2 direction = Vector2.up;

    void Update()
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si toca un enemigo
        if (collision.CompareTag("ball"))
        {
            collision.GetComponent<Ball>().Split();
        }
        else if (collision.CompareTag("Hexagon"))
        {
            collision.GetComponent<Hexagon>().Split();
        }

        // Si toca cualquier superficie en el layer "Ground"
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("ball"))
        {   
            Destroy(gameObject);
            ShootManager.shm.DestroyShot();
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

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}
