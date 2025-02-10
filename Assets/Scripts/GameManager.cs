using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CinemachineVirtualCamera virtualCamera;
    private Vector3 cameraFollowPosition;

    public MovingCube baseCube;
    public TMP_Text scoreText;
    internal int score = 0;
    int spawnCount = 0;

    public CubeSpawner[] cubeSpawners;
    private int spawnerIndex;
    public int scaleCount = 2;
    private CubeSpawner currentSpawner;
    public float moveSpeed = 1.5f;
    public bool isRunnerGame;
    public bool isGameStarted;
    public LevelManager levelManager;

    internal PlayerController playerController;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        if (isRunnerGame)
        {
            playerController = FindAnyObjectByType<PlayerController>();
            levelManager.finishLine.gameObject.SetActive(true);
        }


        cameraFollowPosition = virtualCamera.transform.position;

    }
    public void StartGame()
    {


        isGameStarted = true;
        levelManager.OpenOrCloseStartUI(false);


        if (isRunnerGame)
        {
            virtualCamera.transform.position = cameraFollowPosition;
            playerController.SetTarget(null);
            levelManager.SetFinishLine();
        }
        SpawnCube();
    }
    public bool IsGameStarted()
    {
        return isGameStarted;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (IsGameStarted())
            {
                if (MovingCube.CurrentCube != null && !isRunnerGame)
                {
                    virtualCamera.Follow = MovingCube.CurrentCube.transform;
                    virtualCamera.LookAt = MovingCube.CurrentCube.transform;
                    //playerController.SetTarget(MovingCube.CurrentCube.transform);
                    MovingCube.CurrentCube.Stop();
                }
                else
                {
                    playerController.SetTarget(MovingCube.CurrentCube.transform);
                    virtualCamera.Follow = playerController.transform;
                    virtualCamera.LookAt = playerController.transform;
                    MovingCube.CurrentCube.Stop();
                }
            }
            else
            {
                StartGame();
            }
        }
    }
    internal void FinisSession()
    {
        levelManager.finishLine.enabled = true;
        baseCube.transform.position = new Vector3(levelManager.finishLine.transform.position.x, baseCube.transform.position.y, baseCube.transform.position.z);
        MovingCube.LastCube = baseCube;
        MovingCube.CurrentCube = baseCube;
        baseCube.isStartCube = false;
        levelManager.levelCount++;
        score = 0;
        spawnCount = 0;
        Debug.Log(MovingCube.LastCube.name);
    }
    void RotateCamera()
    {

    }
    public void SpawnCube()
    {
        spawnerIndex = spawnerIndex == 0 ? 1 : 0;
        currentSpawner = cubeSpawners[spawnerIndex];

        if (isRunnerGame)
        {
            if (spawnCount < LevelManager.CurrentLevel.levelLength - 1 && isRunnerGame)
                currentSpawner.SpawnCube();
        }
        else
            currentSpawner.SpawnCube();



    }
    internal void Score()
    {
        score++;
        spawnCount++;
        scoreText.text = score.ToString();

        if (isRunnerGame && spawnCount == LevelManager.CurrentLevel.levelLength)
        {
            isGameStarted = false;
            playerController.SetTarget(levelManager.finishLine.transform);
        }
    }
}


