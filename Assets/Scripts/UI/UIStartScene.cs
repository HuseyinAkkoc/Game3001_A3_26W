using UnityEngine;
using UnityEngine.SceneManagement;
public class UIStartScene : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private GameObject Buttons;
    [SerializeField] private GameObject TitleText;

    [Header("Button Ui SFX")]
    [SerializeField] private AudioSource musicSource;

    public void Play()
    {
        TitleText.SetActive(true);
        SceneManager.LoadScene("Game");
        instructionPanel.SetActive(false);
    }

    public void ShowInstructions()
    {
        if (instructionPanel != null)
        {

            Time.timeScale = 0.0f;
            TitleText.SetActive(false);
            Buttons.SetActive(false);
            instructionPanel.SetActive(true);
        }
    }

    public void HideInstructions()
    {
        if (instructionPanel != null)
        {
            Time.timeScale = 1.0f;

            instructionPanel.SetActive(false);
            Buttons.SetActive(true);
            TitleText.SetActive(true);
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Start");
    }
    public void QuitGame()
    {
        Application.OpenURL("https://huseyinakkoc.itch.io/");
    }



    public void PlayButtonSFX()
    {
        if (musicSource != null)
        {
            musicSource.Play();
        }

    }

}
