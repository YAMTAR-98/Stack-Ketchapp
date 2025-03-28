using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text maxScore;
    internal int score = 0;
    private int temporaryMaxScore;
    public GameObject scoreAndLevelText;
    public GameObject maxScoreText;

    private const string SCORE_KEY = "PlayerScore";
    private const string MAX_SCORE_KEY = "MaxScore";

    private void Start()
    {
        // Daha önce kaydedilmiş skor varsa yükle, yoksa 0 başlat
        temporaryMaxScore = PlayerPrefs.GetInt(MAX_SCORE_KEY, 0);
        Debug.Log(temporaryMaxScore + " Temp Score");
        ShowScore();
    }

    // Puan eklemek için çağrılan metot
    public void Score()
    {
        score++;
        // Her puan alındığında skoru kaydet
        PlayerPrefs.SetInt(SCORE_KEY, score);
        PlayerPrefs.Save();
        ShowScore();
    }
    internal void ToggleLevelAndScoreTexts(bool isActive)
    {
        scoreAndLevelText.SetActive(isActive);
        maxScoreText.SetActive(!isActive);
    }

    // Skoru UI üzerinde göstermek için metot
    public void ShowScore()
    {
        scoreText.text = score.ToString();
        maxScore.text = "Max Score: " + temporaryMaxScore.ToString();
    }

    // Seviye bilgisine göre toplam skoru hesaplar ve kaydeder
    internal void CalculateTotalScore(int currentLevel = 0)
    {
        int levelScore = (currentLevel - 1) * 100;
        // Seviye 0 veya negatifse levelScore'ı 0 olarak ayarla
        levelScore = currentLevel < 0 ? 0 : levelScore;
        int totalScore = levelScore + score;
        Debug.Log(totalScore);


        if (totalScore < PlayerPrefs.GetInt(MAX_SCORE_KEY, 0))
            return;

        PlayerPrefs.SetInt(MAX_SCORE_KEY, totalScore);
        maxScore.text = "Max Score: " + totalScore;
        PlayerPrefs.Save();

        //TODO: Send To Cloud
    }
}
