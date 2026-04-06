using UnityEngine;
using UnityEngine.SceneManagement;
public class UIStartScene : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private GameObject Buttons;
    [SerializeField] private GameObject TitleText;


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
            TitleText.SetActive(false);
            Buttons.SetActive(false);
            instructionPanel.SetActive(true);
        }
    }

    public void HideInstructions()
    {
        if (instructionPanel != null)
        {

            instructionPanel.SetActive(false);
            Buttons.SetActive(true);
            TitleText.SetActive(true);
        }
    }
    public void QuitGame()
    {
        Application.OpenURL("https://huseyinakkoc.itch.io/game3001-a2-w26");
    }



}
