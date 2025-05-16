using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootManager : MonoBehaviour {
    // Eliminamos el enum duplicado (ya está en Player)
    public static ShootManager shm;
    public GameObject[] Shots;
    Transform player;
    public int maxShots;
    public int numberOfShots = 0;
    public int typeOfShot; // 0-Arrow //1-Double Arrow//2-Ancla//3-laser
    Animator animator;
    Player play;
    
    CurrentShotImage shotImage;

    private void Awake()
    {
        if(shm == null)
        {
            shm = this;
        }
        else if(shm!=this)
        {
            Destroy(gameObject);
        }
        player = GameObject.Find("Player").transform;
        play = FindObjectOfType<Player>();
        shotImage = FindObjectOfType<CurrentShotImage>();
    }

    void Start() {
        if (GameManager.gm.gamemode == GameMode.TOUR)
        {
            typeOfShot = 0;
            maxShots = 1;
        }
        else
        {
            ChangeShot(1);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeShot(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeShot(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeShot(1);
        }
        if (CanShot() && Input.GetKeyDown(KeyCode.Space) && GameManager.inGame )
        {
            Shot();
        }
        if(numberOfShots== maxShots && GameObject.FindGameObjectsWithTag("Arrow").Length == 0 && GameObject.FindGameObjectsWithTag("Ancla").Length == 0)
        {
            numberOfShots = 0;
        }
	        }
     bool CanShot()
    {
        return numberOfShots < maxShots;
    }

    void Shot()
    {

        switch (typeOfShot)
        {
            case 0:
                GameObject shot = Instantiate(Shots[typeOfShot], player.position, Quaternion.identity);
                ShotArrow arrow = shot.GetComponent<ShotArrow>();
                arrow.SetSurface(play.currentSurface);
                break;

            case 1:
                GameObject shot1 = Instantiate(Shots[typeOfShot], player.position, Quaternion.identity);
                ShotArrow arrow1 = shot1.GetComponent<ShotArrow>();
                arrow1.SetSurface(play.currentSurface);

                break;
            case 2:
                GameObject shotancla = Instantiate(Shots[typeOfShot], player.position, Quaternion.identity);
                ShotAncla ancla = shotancla.GetComponent<ShotAncla>();
                ancla.SetSurface(play.currentSurface);
                break;
            case 3:
                if(play.currentSurface.ToString() == "Zapaia" || play.currentSurface.ToString() == "Lurra" ){
                GameObject shot1gun = Instantiate(Shots[typeOfShot], new Vector2(player.position.x + .5f, player.position.y), Quaternion.Euler(new Vector3(0,0,-5)));
                GameObject shot2gun = Instantiate(Shots[typeOfShot], new Vector2(player.position.x , player.position.y), Quaternion.identity);
                GameObject shot3gun = Instantiate(Shots[typeOfShot], new Vector2(player.position.x - .5f, player.position.y), Quaternion.Euler(new Vector3(0, 0, 5)));
                ShotGun laser1 = shot1gun.GetComponent<ShotGun>();
                laser1.SetSurface(play.currentSurface);
                ShotGun laser2 = shot2gun.GetComponent<ShotGun>();
                laser2.SetSurface(play.currentSurface);
                ShotGun laser3 = shot3gun.GetComponent<ShotGun>();
                laser3.SetSurface(play.currentSurface);
                }
                else if(play.currentSurface.ToString() == "Ezkerra" || play.currentSurface.ToString() == "Eskubi" ){
                GameObject shot1gun = Instantiate(Shots[typeOfShot], new Vector2(player.position.x , player.position.y + 0.5f), Quaternion.Euler(new Vector3(0,0,-5)));
                GameObject shot2gun = Instantiate(Shots[typeOfShot], new Vector2(player.position.x , player.position.y), Quaternion.identity);
                GameObject shot3gun = Instantiate(Shots[typeOfShot], new Vector2(player.position.x , player.position.y - 0.5f), Quaternion.Euler(new Vector3(0, 0, 5)));
                ShotGun laser1 = shot1gun.GetComponent<ShotGun>();
                laser1.SetSurface(play.currentSurface);
                ShotGun laser2 = shot2gun.GetComponent<ShotGun>();
                laser2.SetSurface(play.currentSurface);
                ShotGun laser3 = shot3gun.GetComponent<ShotGun>();
                laser3.SetSurface(play.currentSurface);
                }
                break;
        }

        numberOfShots++;
    }
    public void DestroyShot()
    {
        if(numberOfShots>0 && numberOfShots < maxShots)
        {
            numberOfShots--;
        }
        
    }
    public void ChangeShot(int type)
    {
        if(typeOfShot != type)
        {
            switch (type)
            {
                case 0:
                    maxShots = 1;
                    shotImage.CurrentShot("");
                    break;

                case 1:
                    maxShots = 2;
                    shotImage.CurrentShot("Arrow");
                    break;
                case 2:
                    maxShots = 1;
                    shotImage.CurrentShot("Ancla");
                    break;
                case 3:
                    maxShots = 15;
                    shotImage.CurrentShot("Gun");
                    break;
            }
            typeOfShot = type;
            numberOfShots = 0;
        }
    }
}
