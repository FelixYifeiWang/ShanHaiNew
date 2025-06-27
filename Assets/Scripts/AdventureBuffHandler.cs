using UnityEngine;

public class AdventureBuffHandler : MonoBehaviour
{
    void Start()
    {
        ApplySelectedBuff();
    }
    
    private void ApplySelectedBuff()
    {
        string selectedBuff = AdventureDataManager.GetSelectedBuff();
        
        if (string.IsNullOrEmpty(selectedBuff))
        {
            Debug.Log("No buff selected for adventure");
            return;
        }
        
        Debug.Log($"Applying buff: {selectedBuff}");
        
        switch (selectedBuff)
        {
            case "Blue Bird青鸟":
                ApplyBlueBirdBuff();
                break;
                
            default:
                Debug.LogWarning($"Buff '{selectedBuff}' not implemented yet");
                break;
        }
    }
    
    private void ApplyBlueBirdBuff()
    {
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        if (gameManager == null)
        {
            Debug.LogError("AdventureGameManager not found! Cannot apply Blue Bird buff");
            return;
        }
        
        // Get current max values
        int originalMaxHP = gameManager.GetMaxHP();
        int originalMaxSteps = gameManager.GetMaxSteps();
        
        // Calculate buffed values (15% increase, ceiling)
        int buffedMaxHP = Mathf.CeilToInt(originalMaxHP * 1.15f);
        int buffedMaxSteps = Mathf.CeilToInt(originalMaxSteps * 1.15f);
        
        // Apply the buff
        gameManager.SetMaxHP(buffedMaxHP);
        gameManager.SetMaxSteps(buffedMaxSteps);
        
        // Also set current values to the new maximums
        gameManager.SetCurrentHP(buffedMaxHP);
        gameManager.SetCurrentSteps(buffedMaxSteps);
        
        Debug.Log($"Blue Bird buff applied! HP: {originalMaxHP} → {buffedMaxHP}, Steps: {originalMaxSteps} → {buffedMaxSteps}");
    }
}