using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingPanel;

    public void ShowLoadingPanel(bool show)
    {
        loadingPanel.SetActive(show);
    }
}