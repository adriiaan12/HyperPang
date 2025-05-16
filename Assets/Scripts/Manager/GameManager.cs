using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameMode {TOUR};

public class GameManager : MonoBehaviour {
    public static GameManager gm;
    public GameObject ready;
    public GameMode gamemode;

    public static bool inGame;
    Player player;
    LifeManager lm;
    Fruits fruits;
    public float time = 100;
    public Text timeTXT;
    public int ballsDestroyed = 0;
    public int fruitsTaken = 0;
    public GameObject panel;
    PanelPoints panelPoints;

     Image progressBar;

     Text levelTXT;
    public int currentLvl = 1;

    private void Awake()
    {
        if (gm == null)
        {
            gm = this;
        }
        if (gm != this)
        {
            Destroy(gameObject);
        }
        player = FindObjectOfType<Player>();
        lm = FindObjectOfType<LifeManager>();
        fruits = FindObjectOfType<Fruits>();

        
        gamemode = GameMode.TOUR;
        
        

    }

    // Use this for initialization
    void Start () {
        StartCoroutine(GameStart());
        ScoreManager.sm.UpdateHiScore();
       
      
    }
	
	// Update is called once per frame
	void Update () {
        if (gamemode == GameMode.TOUR)
        {
            if (BallManager.bm.balls.Count == 0 && HexagonManager.hm.hexagons.Count == 0)
            {
                inGame = false;
                player.Win();
                lm.LifeWin();
                panel.SetActive(true);
                panelPoints = panel.GetComponent<PanelPoints>();
            }
            if (inGame)
            {
                time -= Time.deltaTime;
                timeTXT.text = "Tiempo " + time.ToString("f0");

            }
        }
        else
        {
            if (BallManager.bm.balls.Count == 0 && HexagonManager.hm.hexagons.Count == 0 && BallSpawner.bs.free)
            {
             
                BallSpawner.bs.NewBall();
                
            }
        }
           
        
	}


    public void UpdateBallsDestroyed()
    {
        ballsDestroyed++;

        if(ballsDestroyed % Random.Range(5,18) == 0 && BallManager.bm.balls.Count > 0)
        {
            fruits.InstanciateFruit();
        } 
    }

    public void NextLevel()
    {
        lm.RestartLifesDoll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public int AleatoryNumber()
    {
        return Random.Range(0, 3);
    }



    public IEnumerator GameStart()
    {
        yield return new WaitForSeconds(2);
        ready.SetActive(false);
        if(gamemode == GameMode.TOUR)
        {
            BallManager.bm.StartGame();
            HexagonManager.hm.StartGame();
        }
        else
        {
            BallSpawner.bs.NewBall();
        }
        
        inGame = true;
    }
}
