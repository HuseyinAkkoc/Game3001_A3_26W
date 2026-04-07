using UnityEngine;

public class AudioBackUp : MonoBehaviour
{

    [SerializeField] private GameObject audioManagerPrefab;

    private void Awake()
    {
        if (AudioManager.Instance == null)
        {
            Instantiate(audioManagerPrefab);
        }
    }
}
