using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float startTime = 30f;
    [SerializeField] private bool timerIsRunning = true;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Scene")]
    [SerializeField] private string loseSceneName = "Defeat";

    private float currentTime;
    private bool gameEnded = false;

    private void Start()
    {
        currentTime = startTime;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!timerIsRunning || gameEnded)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateTimerUI();

            gameEnded = true;
            timerIsRunning = false;
            SceneManager.LoadScene(loseSceneName);
            return;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(currentTime).ToString();
        }
    }

    public void StopTimer()
    {
        timerIsRunning = false;
    }
}