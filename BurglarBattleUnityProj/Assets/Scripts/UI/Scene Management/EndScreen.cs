using UnityEngine;
using TMPro;

public class EndScreen : MonoBehaviour
{
    public TextMeshProUGUI[] goldCounter;
    public Transform[] stackingStartPositions;
    public Transform[] nextModStack;
    public Transform[] playerSpawnPositions;
    public Transform[] mvpEmitters;
    public bool[] stopMovement;
    public int[] coinsCollected;
    private bool[] coinsAdded = new bool[4];
    public Transform[] _coinsParent;
    public GameObject modularCoinStack;
    public GameObject largeCoinStack;
    public GameObject mediumCoinStack;
    public GameObject coinSingle;
    public GameObject player;

    public float coinStackMovementRate = 0.3f;

    public float overallEmitterTimer;
    public float timeBetweenSpawns = 0.2f;
    private float _team1Countdown;
    private float _team2Countdown;
    private float spawnCountdown;
    private float mvpCountdown = 1;

    private int _team1Gold = GoldTransferToEnd.team1Gold;
    private int _team2Gold = GoldTransferToEnd.team2Gold;
    private int _team1GoldCounter = 0;
    private int _team2GoldCounter = 0;
    private float numberTimer;
    private float numberTimerDefault = 0.05f;

    private int timeToFinishGoldCounting = 10;

    private int mostCoins;
    private int mvpIndex;

    //Creates the base instances that will be built on top of
    //Uses player models and base stacks to create platforms that can be animated using the MoveStack function if coinscollected is above 14
    void Start()
    {
        float goldDivider = Mathf.Max(_team1Gold, _team2Gold) / timeToFinishGoldCounting;
        _team1Countdown = _team1Gold / goldDivider;
        _team2Countdown = _team2Gold / goldDivider;
        overallEmitterTimer = Mathf.Max(_team1Countdown, _team2Countdown); 

        HighestScore();
        //UpdateCoinCounter();
/*
        goldCounter[0].text = GoldTransferToEnd.team1Gold.ToString();
        goldCounter[1].text = GoldTransferToEnd.team2Gold.ToString();*/
    }


    void UpdateCoinCounter()
    {
        for (int i = 0; i < coinsCollected.Length; i++)
        {
            goldCounter[i].text = coinsCollected[i].ToString();
        }
    }


    //Finds players from all available to get highest coinscollected
    //is used for the MVP function
    void HighestScore()
    {
        for (int i = 0; i < coinsCollected.Length; i++)
        {
            if (coinsCollected[i] > mostCoins)
            {
                mostCoins = coinsCollected[i];
                mvpIndex = i;
            }
        }
    }

    //Emits coins over the mvp of the match and does so over a timer
    void EmitCoins(int index)
    {
        if (overallEmitterTimer > 0)
        {
            overallEmitterTimer -= Time.deltaTime;
            spawnCountdown -= Time.deltaTime;
            _team1Countdown -= Time.deltaTime;
            _team2Countdown -= Time.deltaTime;
            if (spawnCountdown <= 0)
            {
                spawnCountdown = timeBetweenSpawns + Random.Range(-0.1f, 0.1f);
                if (_team1Countdown > 0)
                {
                    Instantiate(coinSingle, mvpEmitters[0].transform.position, mvpEmitters[0].transform.rotation, _coinsParent[0]);
                }

                if (_team2Countdown > 0)
                {
                    Instantiate(coinSingle, mvpEmitters[1].transform.position, mvpEmitters[1].transform.rotation, _coinsParent[1]);
                }
            }
        }
    }

    //Spawn the coinstacks to create higher overall stacks
    //will be moved over time by the MoveStack function
    void AddCoinstack(int playerID, int coins)
    {
        int stacksToAdd = coins / 4;
        for (int i = 0; i < stacksToAdd; i++)
        {
            var coin = Instantiate(modularCoinStack, stackingStartPositions[playerID].position, stackingStartPositions[playerID].rotation, _coinsParent[playerID]);
            nextModStack[playerID].position = new Vector3(nextModStack[playerID].position.x, nextModStack[playerID].position.y - 0.1f, nextModStack[playerID].position.z);
            coin.transform.position = nextModStack[playerID].position;
        }
    }

    //Moves stack over time
    void MoveStack(int playerID)
    {
        playerSpawnPositions[playerID].position = new Vector3(playerSpawnPositions[playerID].position.x, playerSpawnPositions[playerID].position.y + (0.4f * Time.deltaTime), playerSpawnPositions[playerID].position.z);
    }

    private void Update()
    {
        //Moves stack over time till its collider touches the floor
        //Responsible for creating stacks
        for (int i = 0; i < stopMovement.Length; i++)
        {
            if (coinsCollected[i] > 10 && !coinsAdded[i])
            {
                AddCoinstack(i, coinsCollected[i]);
                if (!stopMovement[i])
                {
                    MoveStack(i);
                }
                coinsAdded[i] = true;
            }

            if (!stopMovement[i] && coinsCollected[i] > 10)
            {
                MoveStack(i);
            }
        }

        //MVP emitter countdown
        //Gets triggered when countdown reaches 0 so its not immediate
        mvpCountdown -= Time.deltaTime;
        if (mvpCountdown <= 0)
        {
            UpdateGoldCounter(); 
            EmitCoins(mvpIndex);
        }
    }

    private void UpdateGoldCounter()
    {
        int temp = 0;
        if (numberTimer <= 0)
        {
            if (_team1GoldCounter < _team1Gold)
            {
                temp = Mathf.RoundToInt(_team1Gold / _team1Countdown / 30);
                _team1GoldCounter += temp;
            }
            else
            {
                _team1GoldCounter = _team1Gold; 
            }
            if (_team2GoldCounter < _team2Gold)
            {
                temp = Mathf.RoundToInt(_team2Gold / _team2Countdown / 30);
                _team2GoldCounter +=temp;
     
            }
            else
            {
                _team2GoldCounter = _team2Gold;
            }
            numberTimer = numberTimerDefault; 
        }
        else
        {
            numberTimer -= Time.deltaTime;
        }
  
        goldCounter[0].text = Strings.numbers[_team1GoldCounter];
        goldCounter[1].text = Strings.numbers[_team2GoldCounter];
    }
}
