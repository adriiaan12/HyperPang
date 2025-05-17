using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;

public class ShotAncla : MonoBehaviour
{
    float speed = 4;
    public GameObject chainGFX;
    
    Vector2 startPos;
    Vector2 direction = Vector2.up; // Dirección por defecto
    List<GameObject> chains = new List<GameObject>();

    void Start()
    {
        startPos = transform.position;
        GameObject chain = Instantiate(chainGFX, transform.position, Quaternion.identity);
        chain.transform.parent = transform;
        chains.Add(chain);
    }

    void Update()
    {
        // Usar la variable direction en lugar de Vector3.up
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        
        if (Vector2.Distance(transform.position, startPos) >= 0.2f)
        {
            GameObject chain = Instantiate(chainGFX, transform.position, Quaternion.identity);
            chain.transform.parent = transform;
            chains.Add(chain);
            startPos = transform.position;
        }
    }

    public void SetSurface(Surface surface)
    {
        switch (surface)
        {
            case Surface.Suelo:
                direction = Vector2.up;
                break;
            case Surface.Techo:
                direction = Vector2.down;
                break;
            case Surface.Izquierda:
                direction = Vector2.right;
                break;
            case Surface.Derecha:
                direction = Vector2.left;
                break;
        }

        // Ajustar rotación para que el ancla apunte correctamente
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("horma") )
        {
            StartCoroutine(DestroyAncle());
        }
        if (collision.gameObject.tag == "ball")
        {
            collision.gameObject.GetComponent<Ball>().Split();
            Destroy(gameObject);
            ShootManager.shm.DestroyShot();
        }
        if (collision.gameObject.tag == "Hexagon")
        {
            collision.gameObject.GetComponent<Hexagon>().Split();
            Destroy(gameObject);
            ShootManager.shm.DestroyShot();
        }
        
    }

    IEnumerator DestroyAncle()
    {
        speed = 0;
        yield return new WaitForSeconds(1);
        foreach (GameObject item in chains)
        {
            item.GetComponent<SpriteRenderer>().color = Color.red;
        }
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
        ShootManager.shm.DestroyShot();
    }
}