using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Oyun modlarının ortak davranışlarını tanımlayan arayüz
public interface IGameMode
{
    void StartGame();
    void OnFire1Pressed();
    void SpawnCube();
    void Score();
}

// Runner moduna özgü işlemleri gerçekleştiren sınıf
public class RunnerGameMode : IGameMode
{
    private GameManager gameManager;

    public RunnerGameMode(GameManager gm)
    {
        gameManager = gm;
    }

    public void StartGame()
    {
        gameManager.moveSpeed += gameManager.levelManager.extraSpeed;

        gameManager.isGameStarted = true;
        gameManager.levelManager.OpenOrCloseStartUI(false);

        gameManager.cameraManager.rotateLeftTrigger = false;
        gameManager.playerController.animator.SetBool("Dance", false);
        gameManager.scoreManager.ShowScore();
        //gameManager.virtualCamera.transform.rotation = gameManager.cameraFollowRotation;

        gameManager.playerController.SetTarget(null);

        gameManager.levelManager.SetFinishLine();
        gameManager.baseCubeRenderer.enabled = true;
        SpawnCube();
    }

    public void OnFire1Pressed()
    {
        if (MovingCube.CurrentCube != null)
        {
            // Runner modunda, oyuncu hareket ederken hedef, geçerli cube oluyor
            gameManager.playerController.SetTarget(MovingCube.CurrentCube.transform);
            MovingCube.CurrentCube.Stop();
        }
    }

    public void SpawnCube()
    {
        // Level verisine bağlı olarak spawn sayısı kontrol ediliyor.
        if (gameManager.spawnCount < LevelManager.CurrentLevel.levelLength - 1)
        {
            gameManager.spawnerIndex = gameManager.spawnerIndex == 0 ? 1 : 0;
            gameManager.currentSpawner = gameManager.cubeSpawners[gameManager.spawnerIndex];
            gameManager.currentSpawner.SpawnCube();
        }
    }

    public void Score()
    {
        gameManager.spawnCount++;
        gameManager.scoreManager.Score();

        // Belirlenen level uzunluğuna ulaşıldıysa, finish line'a yönlendir.
        if (gameManager.scoreManager.score == LevelManager.CurrentLevel.levelLength)
        {
            gameManager.isGameStarted = false;
            gameManager.playerController.SetTarget(gameManager.levelManager.finishLine.transform);
        }
    }
}

// Stacking moduna özgü işlemleri gerçekleştiren sınıf
public class StackingGameMode : IGameMode
{
    private GameManager gameManager;

    public StackingGameMode(GameManager gm)
    {
        gameManager = gm;
    }

    public void StartGame()
    {
        gameManager.isGameStarted = true;
        gameManager.levelManager.OpenOrCloseStartUI(false);
        SpawnCube();
    }

    public void OnFire1Pressed()
    {
        if (MovingCube.CurrentCube != null)
        {
            gameManager.virtualCamera.Follow = MovingCube.CurrentCube.transform;
            gameManager.virtualCamera.LookAt = MovingCube.CurrentCube.transform;
            MovingCube.CurrentCube.Stop();
        }
    }

    public void SpawnCube()
    {
        gameManager.spawnerIndex = gameManager.spawnerIndex == 0 ? 1 : 0;
        gameManager.currentSpawner = gameManager.cubeSpawners[gameManager.spawnerIndex];
        gameManager.currentSpawner.SpawnCube();
    }

    public void Score()
    {
        gameManager.spawnCount++;
        gameManager.scoreManager.Score();
    }
}

// Ana GameManager sınıfı
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Camera Manager For Only Runner Game")]
    [Tooltip("For Only Runner Game")]
    public CameraManager cameraManager;
    [Header("Virtual Camera For Only Building Game")]
    [Tooltip("For Only Building Game")]
    public CinemachineVirtualCamera virtualCamera;
    public ScoreManager scoreManager;
    [HideInInspector]
    public Quaternion cameraFollowRotation;

    public MovingCube baseCube;

    internal int spawnCount = 0;

    public CubeSpawner[] cubeSpawners;
    internal int spawnerIndex;
    public int scaleCount = 2;
    internal CubeSpawner currentSpawner;
    public float moveSpeed = 1.5f;
    public bool isRunnerGame;
    public bool isGameStarted;
    public LevelManager levelManager;

    internal PlayerController playerController;

    public bool canCameraRotate;

    // Şu anki oyun modunu tutan alan
    public IGameMode gameMode;
    public float rotationSpeed;
    internal Renderer baseCubeRenderer;
    internal float DEFAULT_MOVE_SPEED;



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
        DEFAULT_MOVE_SPEED = moveSpeed;
        //cameraFollowRotation = virtualCamera.transform.rotation;

        // Oyun moduna göre uygun mod sınıfını seçiyoruz.
        if (isRunnerGame)
        {
            playerController = FindAnyObjectByType<PlayerController>();
            levelManager.finishLine.gameObject.SetActive(true);
            gameMode = new RunnerGameMode(this);
        }
        else
        {
            gameMode = new StackingGameMode(this);
        }
        baseCubeRenderer = baseCube.GetComponent<Renderer>();
    }

    public void StartGame()
    {
        scoreManager.ToggleLevelAndScoreTexts(true);
        // Seçilen mod üzerinden oyunu başlatıyoruz.
        gameMode.StartGame();

    }

    public bool IsGameStarted()
    {
        return isGameStarted;
    }
    public void ResetMoveSpeed()
    {
        moveSpeed = DEFAULT_MOVE_SPEED;
    }

    void Update()
    {
        // Fire1 tuşuna basıldığında ilgili modun OnFire1Pressed metodunu çağırıyoruz.
        if (Input.GetButtonDown("Fire1"))
        {
            if (IsGameStarted())
            {
                gameMode.OnFire1Pressed();
            }
            else
            {
                StartGame();
            }
        }

    }

    internal void FinisSession()
    {
        TotalScore();

        levelManager.finishLine.enabled = true;
        baseCube.transform.position = new Vector3(
            levelManager.finishLine.transform.position.x,
            baseCube.transform.position.y,
            baseCube.transform.position.z);
        baseCubeRenderer.enabled = false;

        MovingCube.LastCube = baseCube;
        MovingCube.CurrentCube = baseCube;
        baseCube.isStartCube = false;
        levelManager.levelCount++;
        cameraManager.rotateLeftTrigger = true;
        playerController.animator.SetBool("Dance", true);

        scoreManager.score = 0;
        spawnCount = 0;
    }
    private void TotalScore()
    {
        scoreManager.CalculateTotalScore(levelManager.GetCurrentLevel());
    }

    public void SpawnCube()
    {
        gameMode.SpawnCube();
    }

    internal void Score()
    {
        gameMode.Score();
    }
}
