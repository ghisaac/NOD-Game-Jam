﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class KSR_RaceManager : MonoBehaviour
{
    public static KSR_RaceManager instance = null;

    public List<KSR_Checkpoint> checkpoints;
    public float raceTime = 300;
    public int lapsToWin = 4;
    public Transform startLine;
    public GameObject[] playerPrefab;

    float startTime;
    float startDelayTimer;
    bool raceStarted = false;
    public List<KSR_Racer> result;
    public List<KSR_Racer> racers;
    public List<KSR_Racer> positions;

    public List<Camera> cameras2p;
    public List<Camera> cameras3p;
    public List<Camera> cameras4p;
    public List<Canvas> canvases2p;
    public List<Canvas> canvases3p;
    public List<Canvas> canvases4p;


    public GameObject[] AllPlayerPrefabs;
    [Header("This bool will spawn some fake players into allplayers before the spawing of the playercontollers.")]
    [Tooltip("OLY USED FOR DEBBUGING, DUMMY!")]
    public bool DEBUGGING = false;

    public static List<GameObject> allSpawnedPlayerControllers = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    //void Start()
    //{
    //    int i = 0;
    //    Player.SpawnTestPlayers(2);
    //    foreach (Player player in Player.AllPlayers)
    //    {
    //        GameObject p = Instantiate(playerPrefab[i], startLine.position + new Vector3(i * 2, 0, 0), startLine.rotation);
    //        p.AddComponent<KSR_Racer>();
    //        p.GetComponent<KSR_Racer>().player = player;
    //        p.GetComponent<KSR_Racer>().nextCheckpoint = checkpoints[0];

    //        racers.Add(p.GetComponent<KSR_Racer>());
    //        p.gameObject.name = "Player " + i;

    //        if (Player.AllPlayers.Count == 2)
    //        {
    //            cameras2p[i].GetComponent<KSR_CameraFollow>()._player = p;
    //        }
    //        else if (Player.AllPlayers.Count == 3)
    //        {

    //        }
    //        else
    //        {

    //        }
    //        i++;
    //    }
    //}

    void Start()
    {

        SoundManager.Instance.PlayBGM();

        if (DEBUGGING)
            Player.SpawnTestPlayers(4);

        allSpawnedPlayerControllers.Clear();

        for (int i = 0; i < Player.AllPlayers.Count; i++)
        {
            GameObject obj = Instantiate(AllPlayerPrefabs[Player.AllPlayers[i].RewierdId]);
            obj.SendMessage("CreatePlayerController", Player.AllPlayers[i]);
            allSpawnedPlayerControllers.Add(obj);
        }

        int j = 0;
        foreach (GameObject go in allSpawnedPlayerControllers)
        {
            go.transform.position = startLine.position + new Vector3(j * 2, 0, 0);
            go.transform.rotation = startLine.rotation;
            racers.Add(go.GetComponent<KSR_Racer>());

            if (allSpawnedPlayerControllers.Count == 2)
            {
                go.GetComponent<KSR_Racer>().playerCamera.rect = new Rect(0f, 0.5f * j, 1f, 0.5f);
            }
            else if (allSpawnedPlayerControllers.Count == 3)
            {
                go.GetComponent<KSR_Racer>().playerCamera.rect = new Rect(((j / 2 + 1) % 2) * 0.501f, j % 2 * 0.501f, 0.498f, 0.498f);
            }
            else
            {
                go.GetComponent<KSR_Racer>().playerCamera.rect = new Rect(((j / 2 + 1) % 2) * 0.501f, j % 2 * 0.501f, 0.498f, 0.498f);
            }
            j++;
        }

        SoundManager.Instance.PlayCountdown();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= 3 && Time.time <= raceTime)
        {
            if (!raceStarted)
            {
                StartRace();
            }
        }
        UpdatePositions();
        if (raceStarted)
        {
            CheckTime();
        }
    }

    void StartTime()
    {
        startTime = Time.time;
        Debug.Log("Race Start!");
    }

    void EndRace()
    {
        foreach (KSR_Racer racer in racers)
        {
            if (racer != null)
            {
                //racer.GetComponent<PlayerController>().enabled = false;
            }
        }

        List<Player> points = new List<Player>();
        foreach (KSR_Racer racer in result)
        {
            points.Add(racer.player);
        }

        Player.DistributePoints(points.ToArray());
        SceneManager.LoadScene("ScoreScreenScene");
        Debug.Log("Race Over!");
        raceStarted = false;
    }

    void StartRace()
    {
        //foreach (KSR_Racer racer in racers)
        //{
        //    //racer.GetComponent<PlayerController>().enabled = true;
        //}
        StartTime();
        raceStarted = true;
    }

    public KSR_Checkpoint Checkpoint(KSR_Racer racer)
    {
        if (racer.lastCheckpoint == checkpoints[0] && racer.nextCheckpoint == checkpoints[0])
        {
            racer.lap += 1;
            racer.lapText.text = racer.lap.ToString() + " Lap";
            racer.lapTimes.Add(Time.time);
            if (racer.lap == lapsToWin + 1)
            {
                result.Add(racer);
                racer.Finished();
                SoundManager.Instance.PlayVictorySound();
                racer.GetComponent<PlayerController>().gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
                SoundManager.Instance.PlayFireworks();
            }
            if (racers.Count == result.Count)
            {
                EndRace();
            }
        }
        if (racer.lastCheckpoint == racer.nextCheckpoint)
        {
            if (checkpoints.IndexOf(racer.lastCheckpoint) != checkpoints.Count - 1)
            {
                racer.checkpointsCleared += 1;
                return checkpoints[checkpoints.IndexOf(racer.lastCheckpoint) + 1];
            }
            else
            {
                racer.checkpointsCleared += 1;
                return checkpoints[0];
            }
        }
        else return racer.nextCheckpoint;
    }

    private void UpdatePositions()
    {
        positions.Clear();
        foreach (KSR_Racer racer in racers)
        {
            racer.CalculateScore();
            positions.Add(racer);
        }
        //Sorting list and check it count
        if (positions.Count > 0)
        {
            positions.Sort(delegate (KSR_Racer a, KSR_Racer b)
            {
                return (a.positionScore).CompareTo(b.positionScore);
            });
        }
        positions.Reverse();
        int k = 1;
        foreach (KSR_Racer racer in positions)
        {
            racer.placementText.text = k.ToString();
            k++;
        }
    }

    private void CheckTime()
    {
        if (Time.time - startTime > raceTime)
        {
            foreach (KSR_Racer racer in positions)
            {
                if (!result.Contains(racer))
                {
                    result.Add(racer);
                }
            }
            EndRace();
        }
    }
}
