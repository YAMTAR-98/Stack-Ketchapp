using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CinemachineVirtualCamera virtualCamera;

    public MovingCube baseCube;
    public TMP_Text scoreText;
    private int score = 0;

    public CubeSpawner[] cubeSpawners;
    private int spawnerIndex;
    private CubeSpawner currentSpawner;
    public float moveSpeed = 1.5f;
    public bool isRunnerGame;

    internal PlayerController playerController;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Invoke(nameof(SpawnCube), 1f);
    }
    private void Start()
    {
        if (isRunnerGame)
            playerController = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (MovingCube.CurrentCube != null && !isRunnerGame)
            {
                virtualCamera.Follow = MovingCube.CurrentCube.transform;
                virtualCamera.LookAt = MovingCube.CurrentCube.transform;
                playerController.SetTarget(MovingCube.CurrentCube.transform);
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
    }
    public void SpawnCube()
    {
        spawnerIndex = spawnerIndex == 0 ? 1 : 0;
        currentSpawner = cubeSpawners[spawnerIndex];

        currentSpawner.SpawnCube();
    }
    internal void Score()
    {
        score++;
        scoreText.text = score.ToString();
    }
}
