using TMPro;
using UnityEngine;

public class NPCStateUI : MonoBehaviour
{
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text transitionsText;

    public void UpdateUI(string stateName, float timer, string transitions)
    {
        if (stateText != null)
            stateText.text = "NPC State: " + stateName;

        if (timerText != null)
        {
            if (timer > 0f)
                timerText.text = "Timer: " + timer.ToString("F1");
            else
                timerText.text = "Timer: --";
        }

        if (transitionsText != null)
            transitionsText.text = "Transitions:\n" + transitions;
    }
}