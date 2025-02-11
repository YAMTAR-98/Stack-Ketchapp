using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MovingCube : MonoBehaviour
{
    public static MovingCube CurrentCube { get; internal set; }
    public static MovingCube LastCube { get; internal set; }
    public MoveDirection MoveDirection { get; internal set; }
    public List<Material> materials = new List<Material>();
    public float scalingDuration = 0.5f;
    public float minHangover = -0.015f;
    public float maxHangover = 0.015f;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] internal bool isStartCube;
    [SerializeField] internal bool lockObjectMovement;
    private float DEFAULT_MOVE_SPEED = 1.5f;

    private const float MIN_SIZE_THRESHOLD = 0.01f;
    private const float SCALE_MULTIPLIER = 1.2f;
    private int touchdownCounter;
    private Renderer cubeRenderer;

    private void Awake()
    {
        cubeRenderer = GetComponent<Renderer>();
        if (LastCube == null && isStartCube)
        {
            LastCube = this;
        }
    }

    private void Start()
    {
        if (moveSpeed > 0)
            CurrentCube = this;

        if (GameManager.Instance != null)
            moveSpeed = GameManager.Instance.moveSpeed;
        DEFAULT_MOVE_SPEED = moveSpeed;

        if (!isStartCube && !lockObjectMovement)
            cubeRenderer.material = GetRandomColor();

        if (LastCube != null && !isStartCube && !lockObjectMovement)
        {
            // Adjust to match the last cube's x and z scale
            Debug.Log(gameObject.name);
            transform.localScale = new Vector3(LastCube.transform.localScale.x, transform.localScale.y, LastCube.transform.localScale.z);
        }
    }

    private Material GetRandomColor()
    {
        return materials[UnityEngine.Random.Range(0, materials.Count - 1)];
    }

    void Update()
    {
        if (lockObjectMovement)
            return;

        // Move according to the movement direction
        switch (MoveDirection)
        {
            case MoveDirection.Z:
                transform.position += transform.forward * Time.deltaTime * moveSpeed;
                break;
            case MoveDirection.X:
                transform.position += transform.right * Time.deltaTime * moveSpeed;
                break;
            case MoveDirection.left:
                transform.position += -transform.right * Time.deltaTime * moveSpeed;
                break;
            case MoveDirection.back:
                transform.position += -transform.forward * Time.deltaTime * moveSpeed;
                break;
            default:
                break;
        }
    }

    internal void Stop()
    {
        if (isStartCube || lockObjectMovement)
            return;

        moveSpeed = 0;
        float hangover = GetHangover();
        Debug.Log(hangover);
        if (LastCube == null)
        {
            Debug.LogError("LastCube is null!");
            ResetScene();
            return;
        }

        // Target size is determined based on the axis of movement.
        float maxSize = (MoveDirection == MoveDirection.Z || MoveDirection == MoveDirection.back)
                            ? LastCube.transform.localScale.z
                            : LastCube.transform.localScale.x;

        // If the offset (hangover) is larger than the last cube's size (extremely misaligned placement)
        if (MathF.Abs(hangover) >= maxSize)
        {
            Debug.Log("asasasasas");
            Rigidbody rb = CurrentCube.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            GameManager.Instance.playerController.SetTarget(GameManager.Instance.levelManager.finishLine.transform);
            LastCube = null;
            CurrentCube = null;
            //ResetScene();
            return;
        }

        // If the offset is within the defined threshold values (minHangover and maxHangover),
        // call TouchDown instead of splitting.
        if (hangover < maxHangover && hangover > minHangover)
        {
            TouchDown(LastCube.transform.position);
            return;
        }

        float direction = hangover > 0 ? 1f : -1f;
        // If the offset is outside the threshold, perform the splitting operations.
        switch (MoveDirection)
        {
            case MoveDirection.Z:
                SplitCubeOnAxis(hangover, direction, isXAxis: false);
                break;
            case MoveDirection.X:
                SplitCubeOnAxis(hangover, direction, isXAxis: true);
                break;
            case MoveDirection.back:
                SplitCubeOnBack(hangover, direction);
                break;
            default:
                break;
        }
        GameManager.Instance.Score();

        if (!isStartCube)
            LastCube = this;
    }

    void ResetScene()
    {
        if (isStartCube || lockObjectMovement)
            return;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static float GetPenalty(float hangover)
    {
        return hangover;
    }

    private float GetHangover()
    {
        if (LastCube == null)
        {
            Debug.LogError("LastCube is null in GetHangover");
            return 0f;
        }
        if (MoveDirection == MoveDirection.Z)
            return transform.position.z - LastCube.transform.position.z;
        if (MoveDirection == MoveDirection.back)
            return LastCube.transform.position.z - transform.position.z;
        else
            return transform.position.x - LastCube.transform.position.x;
    }


    //Helper method to reduce code duplication for the X and Z axes
    private void SplitCubeOnAxis(float hangover, float direction, bool isXAxis)
    {
        if (isStartCube || lockObjectMovement)
            return;

        float lastScale = isXAxis ? LastCube.transform.localScale.x : LastCube.transform.localScale.z;
        float newSize = lastScale - MathF.Abs(hangover);
        if (newSize <= MIN_SIZE_THRESHOLD)
        {
            Debug.Log("Game Over");
            ResetScene();
            return;
        }
        float currentScale = isXAxis ? transform.localScale.x : transform.localScale.z;
        float fallingBlockSize = currentScale - newSize;
        float lastPos = isXAxis ? LastCube.transform.position.x : LastCube.transform.position.z;
        float newPos = lastPos + (hangover / 2);

        Vector3 newScale = transform.localScale;
        Vector3 newPosition = transform.position;
        if (isXAxis)
        {
            newScale.x = newSize;
            newPosition.x = newPos;
        }
        else
        {
            newScale.z = newSize;
            newPosition.z = newPos;
        }
        transform.localScale = newScale;
        transform.position = newPosition;

        float cubeEdge = newPos + (newSize / 2f * direction);
        float fallingBlockPos = cubeEdge + (fallingBlockSize / 2f * direction);
        SpawnDropCube(fallingBlockPos, fallingBlockSize);
    }

    //SplitCubeOnBack: Mathematical calculations clarified and explained with comments
    private void SplitCubeOnBack(float hangover, float direction)
    {
        if (isStartCube || lockObjectMovement)
            return;

        Debug.Log("SplitCubeOnBack hangover: " + hangover);

        float newZSize = LastCube.transform.localScale.z - MathF.Abs(hangover);
        if (newZSize <= MIN_SIZE_THRESHOLD)
        {
            Debug.Log("Game Over");
            ResetScene();
            return;
        }
        float fallingBlockSize = transform.localScale.z - newZSize;

        // For back direction, the new z position should be calculated by subtracting half the hangover
        float newZPosition = LastCube.transform.position.z - (hangover / 2f);
        Vector3 newPos = transform.position;
        newPos.z = newZPosition;
        transform.position = newPos;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, newZSize);

        // Calculate the position of the falling block:
        // For back direction, the kept block's back edge is:
        float cubeEdge = transform.position.z - (newZSize / 2f);
        // The falling block should be positioned further back by half of its own size
        float fallingBlockZPosition = cubeEdge - (fallingBlockSize / 2f * direction);

        SpawnDropCube(fallingBlockZPosition, fallingBlockSize);
    }


    private void TouchDown(Vector3 lastCubePos)
    {
        if (isStartCube || lockObjectMovement)
            return;


        touchdownCounter = SoundManager.Instance.PlaySound();
        GameManager.Instance.moveSpeed += 0.2f;
        Vector3 newPos;
        if (!LastCube.isStartCube)
        {
            if ((MoveDirection == MoveDirection.back && GameManager.Instance.isRunnerGame) ||
                (MoveDirection == MoveDirection.Z && GameManager.Instance.isRunnerGame))
            {
                if (LastCube.lockObjectMovement)
                    newPos = new Vector3(lastCubePos.x + LastCube.transform.localScale.x, transform.position.y, lastCubePos.z);
                else
                    newPos = new Vector3(lastCubePos.x + LastCube.transform.localScale.x, lastCubePos.y, lastCubePos.z);
            }
            else
            {
                newPos = new Vector3(lastCubePos.x, lastCubePos.y + LastCube.transform.localScale.y, lastCubePos.z);
            }
        }
        else
        {
            newPos = transform.position;
        }
        transform.position = newPos;

        if (touchdownCounter >= GameManager.Instance.scaleCount)
        {
            Debug.Log("albalbalb");
            StartCoroutine(ScaleCoroutine());
        }
        else
        {
            LastCube = this;
            Debug.Log("blablabla");
            GameManager.Instance.SpawnCube();
            GameManager.Instance.Score();
        }

    }

    IEnumerator ScaleCoroutine()
    {
        Vector3 initialScale = transform.localScale;
        float targetX = Mathf.Min(initialScale.x * SCALE_MULTIPLIER, 1f);
        float targetZ = Mathf.Min(initialScale.z * SCALE_MULTIPLIER, 1f);
        Vector3 targetScale = new Vector3(targetX, initialScale.y, targetZ);

        float elapsed = 0f;
        while (elapsed < scalingDuration)
        {
            elapsed += Time.deltaTime * 2f;
            float t = elapsed / scalingDuration;
            float newX = Mathf.Lerp(initialScale.x, targetScale.x, t);
            float newZ = Mathf.Lerp(initialScale.z, targetScale.z, t);
            transform.localScale = new Vector3(newX, initialScale.y, newZ);
            yield return null;
        }
        transform.localScale = targetScale;
        LastCube = this;
        GameManager.Instance.SpawnCube();
        GameManager.Instance.Score();
    }

    // Create the falling block (the cut part)
    private void SpawnDropCube(float dropPos, float fallingBlockSize)
    {
        if (isStartCube || lockObjectMovement)
            return;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (MoveDirection == MoveDirection.Z || MoveDirection == MoveDirection.back)
        {
            cube.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, fallingBlockSize);
            cube.transform.position = new Vector3(transform.position.x, transform.position.y, dropPos);
        }
        else
        {
            cube.transform.localScale = new Vector3(fallingBlockSize, transform.localScale.y, transform.localScale.z);
            cube.transform.position = new Vector3(dropPos, transform.position.y, transform.position.z);
        }
        cube.AddComponent<Rigidbody>();
        cube.GetComponent<BoxCollider>().isTrigger = true;
        cube.GetComponent<Renderer>().material.color = cubeRenderer.material.color;
        SoundManager.Instance.ResetCounter();
        SoundManager.Instance.PlayCutSound();
        LastCube = this;
        GameManager.Instance.moveSpeed = DEFAULT_MOVE_SPEED;
        GameManager.Instance.SpawnCube();
        Destroy(cube, 4f);
    }
}
