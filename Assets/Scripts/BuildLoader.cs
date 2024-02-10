using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildLoader : MonoBehaviour
{
    [SerializeField] public string startingSceneName;
    private void Awake()
    {
        if (!Application.isEditor)
            LoadPersistent();
    }

    private void LoadPersistent()
    {
        SceneManager.LoadSceneAsync(startingSceneName, LoadSceneMode.Additive);
    }
}