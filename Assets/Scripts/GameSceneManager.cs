using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager instance;
    
    public static GameSceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameSceneManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameSceneManager");
                    instance = go.AddComponent<GameSceneManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadAdventureScene()
    {
        Debug.Log("Loading Adventure Scene...");
        SceneManager.LoadScene("AdventureScene");
    }
    
    public void LoadMainScene()
    {
        Debug.Log("Loading Main Scene...");
        
        // Force a save before returning (in case anything changed in adventure scene)
        SaveSystem saveSystem = Object.FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            saveSystem.TriggerSave("Before returning to main");
        }
        
        // Try multiple ways to load the main scene
        try
        {
            // First try by name - replace "SampleScene" with your actual main scene name
            SceneManager.LoadScene("SampleScene");
        }
        catch
        {
            try
            {
                // If that fails, try by index
                SceneManager.LoadScene(0);
            }
            catch
            {
                Debug.LogError("Failed to load main scene! Check scene names in Build Settings.");
            }
        }
    }
    
    public void ReturnToMainScene()
    {
        LoadMainScene();
    }
}