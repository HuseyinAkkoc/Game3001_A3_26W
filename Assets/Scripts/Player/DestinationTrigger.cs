using UnityEngine;
using UnityEngine.SceneManagement;

public class DestinationTrigger : MonoBehaviour
{
    [SerializeField] private string winSceneName = "VictoryScene";
    [SerializeField] private GameTimer gameTimer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameTimer != null)
            {
                gameTimer.StopTimer();
            }

            SceneManager.LoadScene(winSceneName);
        }
    }
}