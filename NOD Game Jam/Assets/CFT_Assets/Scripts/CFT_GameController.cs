﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class CFT_GameController : MonoBehaviour
{
    //Enum för game state
    private enum GameState { init, game, pause, checkTowerHeight, end };
    private GameState _gameState;

    //Variabler
    [Header ("Players")]
    [SerializeField]
    private int _numberOfPlayers;
    [Header("Timer")]
    [SerializeField]
    private float _setInitTimer;
    [SerializeField]
    private float _setRoundTimer;
    [SerializeField]
    private float _setCheckTimer;
    private float _timer;

    [Header("Rounds")]
    [SerializeField]
    private int _maxRound;
    private int _currentRound;

    private List<Camera> _cameras;
    private List<CFT_CupController> _gameBoards;
    private float _cameraOffset = 0;

    [Header("UI")]
    public Text PlayerName;
    public GameObject PlayerNameDisplayBord;

    [Header(" ")]
    [SerializeField]
    private bool _isProduction = false;

    //Spawn Stuff
    [SerializeField]
    private GameObject[] _playerSpawnPoint;
    [SerializeField]
    private GameObject _boardGame;

    //PlayerManager
    private CFT_PlayerManager _playerManager;

    //UI
    [SerializeField]
    private Text _timerText;

    //Data för placering
    //private CFT_RoundData _roundData = new CFT_RoundData();
    private CFT_WinnerData _winnerData;
  
    private int _winner;

    private bool _gameOver = false;

    private void Awake()
    {
        _gameBoards = new List<CFT_CupController>();
        _cameras = new List<Camera>();      
        _playerManager = FindObjectOfType<CFT_PlayerManager>();
        new Player(0, "Micke");
        new Player(1, "Kalle");
        new Player(2, "Steffan");
        new Player(3, "Arne");

        if (_isProduction)
        {       
            _numberOfPlayers = Player.AllPlayers.Count;
            InitializePlayerProduction();
            _playerManager.InitProduction();           
        }
        else
        {
            InitializePlayer();
            _playerManager.Init(_numberOfPlayers);            
        }

        SetCameraViewport();
        _gameState = GameState.init;
        _timer = _setInitTimer;
        _winnerData = new CFT_WinnerData();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        GameLoop();
        UpdateUI();  
    }

    private void GameLoop()
    {
        switch(_gameState)
        {
            case GameState.init:
                GameStateInit();
                break;
            case GameState.game:
                GameStateGame();
                break;
            case GameState.checkTowerHeight:
                GameStateCheckTowerHeight();
                break;
            case GameState.pause:
                GameStatePause();
                break;
            case GameState.end:
                GameStateEnd();
                break;
        }
    }

    #region GameStateInit
    private void GameStateInit()
    {
        bool drop = false;
        _playerManager.CanDrop(drop);
        if (_timer <= 0) { _timer = _setRoundTimer; _gameState = GameState.game; }
    }
    #endregion

    #region GameStateGame
    private void GameStateGame()
    {
        bool drop = true;
        _playerManager.CanDrop(drop);
        if (_timer <= 0)
        {
            _timer = _setCheckTimer;
            _gameState = GameState.checkTowerHeight;
        }
    }
    #endregion

    #region GameStateCheckTowerHeight
    private void GameStateCheckTowerHeight()
    {
        bool drop = false;
        _playerManager.CanDrop(drop);
        if (_timer <= 0)
        {
            CheckRoundWinner();
            ClearGameBoard();
            _timer = _setInitTimer;
            _currentRound += 1;
            if (_currentRound >= _maxRound) { _gameState = GameState.end; }
            else
                _gameState = GameState.pause;
        }
    }
    #endregion

    #region GameStatePause
    private void GameStatePause()
    {
        if (_timer <= 0)
        {
            _timer = _setInitTimer;
            _gameState = GameState.init;
        }
    }
    #endregion

    #region GameStateEnd
    private void GameStateEnd()
    {
        if(!_gameOver)
        {

            _winner = _winnerData.GetWinner();
            _gameOver = true;
            StartCoroutine(LoadEndScen());
        }      
    }

    IEnumerator LoadEndScen()
    {
        yield return new WaitForSeconds(3);
      

        SceneManager.LoadScene("ScoreScreenScene");
    }
    #endregion

    private void CheckRoundWinner()
    {
        CFT_RoundData _roundData = new CFT_RoundData();

        foreach (CFT_CupController g in _gameBoards)
        {
            var _playerCups = g.GetComponent<CFT_CupController>();
            _roundData.roundScores.Add(_playerCups.playerID, _playerCups.BoxCastHeight());       
        }
        _roundData.SetRoundPlacement(_roundData.roundScores);
        _winnerData.rounds.Add(_roundData);
        _winnerData.SetRoundScore((_currentRound));
    }

    private void ClearGameBoard()
    {
        foreach (CFT_CupController c in _gameBoards)
        {
            c.RemoveCups();
        }
    }

    private void SetCameraViewport()
    {     
        for (int i = 0; i < _cameras.Count; i++)
        {
            _cameras[i].rect = new Rect(_cameraOffset, 0, 1.0f / _cameras.Count, 1);
            _cameraOffset += (1.0f / _cameras.Count);
        } 
    }

    private void InitializePlayer()
    {
        for (int i = 0; i < _numberOfPlayers; i++)
        {
            GameObject bg = Instantiate(_boardGame, _playerSpawnPoint[i].transform.position, Quaternion.Euler(0, 0, 0), _playerSpawnPoint[i].transform);
            bg.GetComponentInChildren<CFT_CupController>().Init(i);
            CFT_CupController _cup = bg.GetComponentInChildren<CFT_CupController>();
            _gameBoards.Add(_cup);
            Camera go = bg.GetComponentInChildren<Camera>();
            _cameras.Add(go);
            Text pName = Instantiate(PlayerName, PlayerNameDisplayBord.transform);
            pName.text = "Player" + (i + 1);
        }
    }

    private void InitializePlayerProduction()
    {
        for (int i = 0; i < _numberOfPlayers; i++)
        {
            GameObject bg = Instantiate(_boardGame, _playerSpawnPoint[i].transform.position, Quaternion.Euler(0, 0, 0), _playerSpawnPoint[i].transform);
            bg.GetComponentInChildren<CFT_CupController>().Init(Player.AllPlayers[i].RewierdId);
            CFT_CupController _cup = bg.GetComponentInChildren<CFT_CupController>();
            _gameBoards.Add(_cup);
            Camera go = bg.GetComponentInChildren<Camera>();
            _cameras.Add(go);
            Text pName = Instantiate(PlayerName, PlayerNameDisplayBord.transform);
            pName.text = Player.AllPlayers[i].Name;
        }
    }

    private void UpdateUI()
    {
        if (_gameState == GameState.init)
        {
            DisplayName(true);
            _timerText.text = "Round starts in: " + (int)_timer;
        }
        else if (_gameState == GameState.game)
        {
            DisplayName(false);
            _timerText.text = "Round: " + (_currentRound + 1) + "\n" + "Time left: " + (int)_timer;
        }  
        else if (_gameState == GameState.checkTowerHeight)
        {
            _timerText.text = "Checking height..";
        }
        else if (_gameState == GameState.pause)
        {
            //Ändra roundwinner till metod som kikar om det är tie eller inte.
            _timerText.text = "Round winner is: " + RoundWinner();
        }
        else if (_gameState == GameState.end)
        {
            _timerText.text = "MATCH ENDED!" + "\n" + "Winner is: " + "Player" + _winner.ToString();
        }       
    }

    private string RoundWinner()
    {
        string winner = null;
        CFT_RoundData placementHolder = _winnerData.rounds[(_currentRound - 1)];
        int[] winners = placementHolder.GetPlacement(1);


        if (winners.Length == 1)
        {
            winner = "Player" + winners[0].ToString();
        }
        else if (winners.Length > 1)
        {
            winner = "Tie";
        }
        return winner;
    }

    private void DisplayName(bool v)
    {
        foreach(Text t in PlayerNameDisplayBord.GetComponentsInChildren<Text>())
        {
            t.gameObject.SetActive(v);
        }
    }
}
