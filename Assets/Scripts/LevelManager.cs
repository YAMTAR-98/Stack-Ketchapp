using TMPro;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    [Header("Level Information")]
    [Tooltip("Level Lenght")]
    public int levelLength;

    [Tooltip("Difficulty (1-10)")]
    [Range(1, 10)]
    public int difficulty;

}

public class LevelManager : MonoBehaviour
{

    public static LevelData CurrentLevel { get; private set; }

    [Header("Difficulty (5 Level)")]
    public LevelData[] levels = new LevelData[5];

    public MovingCube finishLine;
    public GameObject StartUI;
    public TMP_Text levelText;
    public int levelCount = 0;
    internal float extraSpeed;

    private const int DIFFICULTY_DIVIDER = 5;

    private void Start()
    {
        SetFinishLine();
    }
    internal void SetFinishLine()
    {
        if (!GameManager.Instance.isRunnerGame)
            return;
        CurrentLevel = levels[levelCount];
        levelText.text = "Level:  " + (levelCount + 1).ToString();

        float offsetX = (MovingCube.LastCube.transform.localScale.x / 2) + finishLine.transform.localScale.z + levels[levelCount].levelLength;
        float offsetY = MovingCube.LastCube.transform.position.y + 0.5f;
        finishLine.transform.position = new Vector3(MovingCube.LastCube.transform.position.x + offsetX, offsetY, 0);
        if (GameManager.Instance.isRunnerGame)
            SetDifficulty();
    }
    internal void SetDifficulty()
    {
        if (!GameManager.Instance.isRunnerGame)
            return;
        extraSpeed = levels[levelCount].difficulty / DIFFICULTY_DIVIDER;
    }
    internal void OpenOrCloseStartUI(bool canOpen)
    {
        StartUI.SetActive(canOpen);
    }
}
