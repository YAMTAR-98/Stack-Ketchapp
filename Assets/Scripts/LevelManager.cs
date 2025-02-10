using UnityEngine;

[System.Serializable]
public class LevelData
{
    [Header("Seviye Bilgileri")]
    [Tooltip("Seviye uzunluğu")]
    public int levelLength;

    [Tooltip("Zorluk seviyesi (1-10 arası)")]
    [Range(1, 10)]
    public int difficulty;

}

public class LevelManager : MonoBehaviour
{

    public static LevelData CurrentLevel { get; private set; }

    [Header("Level Verileri (5 seviye)")]
    public LevelData[] levels = new LevelData[5];

    public MovingCube finishLine;
    public GameObject StartUI;
    public int levelCount = 0;

    private void Start()
    {

        // Örnek olarak her seviyenin bilgilerini konsola yazdırabilirsiniz.
        for (int i = 0; i < levels.Length; i++)
        {
            Debug.Log($"Level {i + 1}: Uzunluk = {levels[i].levelLength}, Zorluk = {levels[i].difficulty}");
        }
        SetFinishLine();
    }
    internal void SetFinishLine()
    {
        if (!GameManager.Instance.isRunnerGame)
            return;
        CurrentLevel = levels[levelCount];
        float offsetX = (MovingCube.LastCube.transform.localScale.x / 2) + (finishLine.transform.localScale.z / 2) + levels[levelCount].levelLength;
        float offsetY = MovingCube.LastCube.transform.position.y + 0.5f;
        finishLine.transform.position = new Vector3(MovingCube.LastCube.transform.position.x + offsetX, offsetY, 0);
    }
    internal void OpenOrCloseStartUI(bool canOpen)
    {
        StartUI.SetActive(canOpen);
    }
}
