using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreService : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _MaxscoreText;
    private int _score;
    private int _MaxScore;
    public void Start()
    {
        _score = 0;
        _scoreText.text = "0";
    }
    public void AddScore(int score)
    {
        _score += score;
        _scoreText.text = _score.ToString();
    }
}

