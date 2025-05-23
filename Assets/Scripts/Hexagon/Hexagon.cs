﻿using UnityEngine;

public class Hexagon : MonoBehaviour {

    public GameObject nextHexagon;
    Rigidbody2D rb;
    public bool right;

    public float forceX = 1;
    public float forceY = 1;

    float currentforceX;
    float currentforceY;

    float rotSpeed;

    public GameObject powerUp;

    // Bandera para evitar dividir varias veces
    private bool alreadySplit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        rb.linearVelocity = Vector2.zero;
    }
    private void OnBecameInvisible()
    {
        if (GameManager.gm.gamemode == GameMode.TOUR)
        {
            if (HexagonManager.hm.hexagons.Contains(gameObject))
            {
                HexagonManager.hm.hexagons.Remove(gameObject);
            }

            Destroy(gameObject);
        }
    }
    private void Update()
    {
        if (GameManager.inGame)
        {
            rotSpeed = 250 * Time.deltaTime;
            transform.Rotate(0, 0, rotSpeed);
            rb.linearVelocity = new Vector2(forceX, forceY);
        }
        else
        {
            rotSpeed = 0;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Split()
    {
        // Evitar división múltiple
        if (alreadySplit)
        {
            Debug.LogWarning("Hexagon ya dividido, ignorando Split: " + name);
            return;
        }
        alreadySplit = true;

        Vector2 spawnOffset = new Vector2(0, 0.2f); // Desplaza hacia arriba
        
        if (nextHexagon != null)
        {
            if (GameManager.gm.gamemode == GameMode.TOUR)
            {
                InstantiatePrize();
            }

            GameObject hex1 = Instantiate(nextHexagon, rb.position + Vector2.right / 4 + spawnOffset, Quaternion.identity);
            hex1.GetComponent<Hexagon>().right = true;
            hex1.GetComponent<Hexagon>().alreadySplit = false;  // Asegurarse que el clon no esté marcado

            GameObject hex2 = Instantiate(nextHexagon, rb.position + Vector2.left / 4 + spawnOffset, Quaternion.identity);
            hex2.GetComponent<Hexagon>().right = false;
            hex2.GetComponent<Hexagon>().alreadySplit = false;  // Igual aquí

            if (!FreezeManager.fm.freeze)
            {
                hex1.GetComponent<Hexagon>().forceX = forceX;
                hex1.GetComponent<Hexagon>().forceY = -forceY;
                hex2.GetComponent<Hexagon>().forceX = -forceX;
                hex2.GetComponent<Hexagon>().forceY = -forceY;
            }
            else
            {
                hex1.GetComponent<Hexagon>().currentforceX = forceX;
                hex1.GetComponent<Hexagon>().currentforceY = -forceY;

                hex2.GetComponent<Hexagon>().currentforceX = -forceX;
                hex2.GetComponent<Hexagon>().currentforceY = -forceY;
            }

            if (!HexagonManager.hm.spliting)
            {
                HexagonManager.hm.DestroyHexagon(gameObject, hex1, hex2);
            }
        }
        else
        {
            HexagonManager.hm.LastHexagon(gameObject);
        }

        int score = Random.Range(300, 601);
        PopUpManager.pop.InstanciatePopUpText(gameObject.transform.position, score);
        ScoreManager.sm.UpdateScore(score);
        GameManager.gm.UpdateBallsDestroyed();
    }
    public void StartForce(GameObject hex)
    {
        if (right)
        {
            hex.GetComponent<Hexagon>().forceX = forceX;
        }
        else
        {
            hex.GetComponent<Hexagon>().forceX = -forceX;
        }

        hex.GetComponent<Hexagon>().forceY = forceY;
    }
    public void FreezeHexagon(params GameObject[] hexagons)
    {
        foreach (GameObject item in hexagons)
        {
            if (item != null)
            {
                currentforceX = item.GetComponent<Hexagon>().forceX;
                currentforceY = item.GetComponent<Hexagon>().forceY;

                item.GetComponent<Hexagon>().forceX = 0;
                item.GetComponent<Hexagon>().forceY = 0;
                item.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            }
        }
    }
    public void UnFreezeHexagon(params GameObject[] hexagons)
    {
        foreach (GameObject item in hexagons)
        {
            if (item != null)
            {
                item.GetComponent<Hexagon>().forceX = currentforceX;
                item.GetComponent<Hexagon>().forceY = currentforceY;
                item.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(currentforceX, currentforceY);
            }
        }
    }

    public void SlowHexagon()
    {
        if (rb.linearVelocity.x < 0)
        {
            forceX = -1;
        }
        else
        {
            forceX = 1;
        }
        if (rb.linearVelocity.y < 0)
        {
            forceY = -1;
        }
        else
        {
            forceY = 1;
        }
    }
    public void NormalSpeedHexagon()
    {
        if (rb.linearVelocity.x < 0)
        {
            forceX = -2;
        }
        else
        {
            forceX = 2;
        }
        if (rb.linearVelocity.y < 0)
        {
            forceY = -2;
        }
        else
        {
            forceY = 2;
        }
        rb.linearVelocity = new Vector2(forceX, forceY);
    }
    void InstantiatePrize()
    {
        int aleatory = GameManager.gm.AleatoryNumber();

        if (aleatory == 1)
        {
            Instantiate(powerUp, transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Suelo" || other.gameObject.tag == "Techo")
        {
            forceY *= -1;
        }
        if (other.gameObject.tag == "Izquierda" || other.gameObject.tag == "Derecha")
        {
            forceX *= -1;
        }
  
    }
}
