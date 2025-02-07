using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MovingCube : MonoBehaviour
{
    public static MovingCube CurrentCube { get; private set; }
    public static MovingCube LastCube { get; internal set; }
    public MoveDirection MoveDirection { get; internal set; }

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] internal bool isStartCube;

    private int toucdownCounter;

    private void Start()
    {
        if (moveSpeed > 0)
            CurrentCube = this;
        if (LastCube == null && isStartCube)
            LastCube = GameManager.Instance.baseCube;
        moveSpeed = GameManager.Instance.moveSpeed;
        GetComponent<Renderer>().material.color = GetRandomColor();

        transform.localScale = new Vector3(LastCube.transform.localScale.x, transform.localScale.y, LastCube.transform.localScale.z);
    }

    private Color GetRandomColor()
    {
        return new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
    }


    void Update()
    {
        if (isStartCube)
            return;
        switch (MoveDirection)
        {
            case MoveDirection.Z:
                transform.position += transform.forward * Time.deltaTime * moveSpeed;
                break;
            case MoveDirection.X:
                transform.position += transform.right * Time.deltaTime * moveSpeed;
                break;
            default:
                break;
        }

    }

    internal void Stop()
    {
        moveSpeed = 0;
        float hangover = GetHangover();
        float max = MoveDirection == MoveDirection.Z ? LastCube.transform.localScale.z : LastCube.transform.localScale.x;
        if (MathF.Abs(hangover) >= max)
        {
            LastCube = null;
            CurrentCube = null;
            SceneManager.LoadScene(0);
            return;
        }
        float direction = hangover > 0 ? 1f : -1f;

        float penalty = GetPenalty(hangover);

        switch (MoveDirection)
        {
            case MoveDirection.Z:
                SplitCubeOnZ(hangover, direction);
                break;
            case MoveDirection.X:
                SplitCubeOnX(hangover, direction);
                break;
            default:
                break;
        }
        GameManager.Instance.Score();


        if (!isStartCube)
            LastCube = this;
    }

    private static float GetPenalty(float hangover)
    {
        //Debug.Log(hangover);
        return hangover;
    }

    private float GetHangover()
    {
        if (MoveDirection == MoveDirection.Z)
            return transform.position.z - LastCube.transform.position.z;
        else
            return transform.position.x - LastCube.transform.position.x;
    }

    private void SplitCubeOnX(float hangover, float direction)
    {
        //Debug.Log(hangover);
        if (hangover >= 0.01 || hangover <= -0.05)
        {
            float newXSize = LastCube.transform.localScale.x - MathF.Abs(hangover);
            float fallingBlockSize = transform.localScale.x - newXSize;

            float newXPosition = LastCube.transform.position.x + (hangover / 2);
            transform.localScale = new Vector3(newXSize, transform.localScale.y, transform.localScale.z);
            transform.position = new Vector3(newXPosition, transform.position.y, transform.position.z);

            float cubeEdge = transform.position.x + (newXSize / 2f * direction);
            float fallingBlockXPosition = cubeEdge + fallingBlockSize / 2f * direction;

            SpawnDropCube(fallingBlockXPosition, fallingBlockSize);
        }
        else
        {
            TouchDown(LastCube.transform.position);
        }
    }
    private void SplitCubeOnZ(float hangover, float direction)
    {
        //Debug.Log(hangover);
        if (hangover >= 0.015 || hangover <= -0.05)
        {
            float newZSize = LastCube.transform.localScale.z - MathF.Abs(hangover);
            float fallingBlockSize = transform.localScale.z - newZSize;

            float newZPosition = LastCube.transform.position.z + (hangover / 2);
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, newZSize);
            transform.position = new Vector3(transform.position.x, transform.position.y, newZPosition);

            float cubeEdge = transform.position.z + (newZSize / 2f * direction);
            float fallingBlockZPosition = cubeEdge + fallingBlockSize / 2f * direction;

            SpawnDropCube(fallingBlockZPosition, fallingBlockSize);
        }
        else
        {
            TouchDown(LastCube.transform.position);
        }
    }
    private void TouchDown(Vector3 lastCubePos)
    {
        toucdownCounter = SoundManager.Instance.PlaySound();
        GameManager.Instance.moveSpeed += 0.2f;
        Vector3 newPos;
        if (!LastCube.isStartCube)
            newPos = new Vector3(lastCubePos.x, lastCubePos.y + LastCube.transform.localScale.y, lastCubePos.z);
        else
            newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        transform.position = newPos;

        if (toucdownCounter >= 1)
        {
            StartCoroutine(ScaleCoroutine());
        }
        else
        {
            LastCube = this;
            GameManager.Instance.SpawnCube();
        }
    }
    public float scalingDuration = 2f;
    IEnumerator ScaleCoroutine()
    {
        Vector3 initialScale = transform.localScale;

        // Hedef ölçek: x ve z, başlangıç değerlerinin 1.2 katı,
        // ancak 1'den büyük olamaz.
        float targetX = Mathf.Min(initialScale.x * 1.2f, 1f);
        float targetZ = Mathf.Min(initialScale.z * 1.2f, 1f);
        Vector3 targetScale = new Vector3(targetX, initialScale.y, targetZ);

        float elapsed = 0f;

        // Belirlenen süre boyunca ölçek geçişini gerçekleştir
        while (elapsed < scalingDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scalingDuration;

            // Lerp ile x ve z eksenlerinde yumuşak geçiş sağlanır
            float newX = Mathf.Lerp(initialScale.x, targetScale.x, t);
            float newZ = Mathf.Lerp(initialScale.z, targetScale.z, t);
            transform.localScale = new Vector3(newX, initialScale.y, newZ);

            yield return null;
        }

        // Son durumda kesin hedef ölçeğe ulaşalım
        transform.localScale = targetScale;

        LastCube = this;
        GameManager.Instance.SpawnCube();
    }

    private void SpawnDropCube(float fallingBlockZPosition, float fallingBlockSize)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (MoveDirection == MoveDirection.Z)
        {
            cube.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, fallingBlockSize);
            cube.transform.position = new Vector3(transform.position.x, transform.position.y, fallingBlockZPosition);
        }
        else
        {
            cube.transform.localScale = new Vector3(fallingBlockSize, transform.localScale.y, transform.localScale.z);
            cube.transform.position = new Vector3(fallingBlockZPosition, transform.position.y, transform.position.z);
        }
        cube.AddComponent<Rigidbody>();
        cube.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;
        SoundManager.Instance.ResetCounter();
        SoundManager.Instance.PlayCutSound();
        LastCube = this;
        GameManager.Instance.moveSpeed = 1.5f;
        GameManager.Instance.SpawnCube();
        Destroy(cube, 4f);
    }
}
