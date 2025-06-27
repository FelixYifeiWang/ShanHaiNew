using UnityEngine;
using System.Collections.Generic;

public class AdventureDataManager : MonoBehaviour
{
    public static AdventureDataManager Instance;
    
    [Header("Adventure Selection Data")]
    public string selectedBuffName;
    public List<string> selectedSkillNames = new List<string>();
    public List<int> selectedSkillLevels = new List<int>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void SetAdventureData(string buffName, List<string> skillNames, List<int> skillLevels)
    {
        // Create instance if it doesn't exist
        if (Instance == null)
        {
            GameObject dataManager = new GameObject("AdventureDataManager");
            Instance = dataManager.AddComponent<AdventureDataManager>();
            DontDestroyOnLoad(dataManager);
        }
        
        Instance.selectedBuffName = buffName;
        Instance.selectedSkillNames = new List<string>(skillNames);
        Instance.selectedSkillLevels = new List<int>(skillLevels);
    }
    
    public static string GetSelectedBuff()
    {
        return Instance != null ? Instance.selectedBuffName : "";
    }
    
    public static List<string> GetSelectedSkillNames()
    {
        return Instance != null ? Instance.selectedSkillNames : new List<string>();
    }
    
    public static List<int> GetSelectedSkillLevels()
    {
        return Instance != null ? Instance.selectedSkillLevels : new List<int>();
    }
    
    public static void ClearData()
    {
        if (Instance != null)
        {
            Instance.selectedBuffName = "";
            Instance.selectedSkillNames.Clear();
            Instance.selectedSkillLevels.Clear();
        }
    }
}