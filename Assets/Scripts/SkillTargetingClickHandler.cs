using UnityEngine;

public class SkillTargetingClickHandler : MonoBehaviour
{
    private HexTileController tileController;
    
    void Start()
    {
        tileController = GetComponent<HexTileController>();
    }
    
    void OnMouseDown()
    {
        // Check if we're in skill targeting mode first
        if (AdventureSkillUI.Instance != null)
        {
            // If skill targeting is active, handle it and prevent normal tile click
            AdventureSkillUI skillUI = AdventureSkillUI.Instance;
            if (skillUI.IsWaitingForTargetSelection())
            {
                if (tileController != null)
                {
                    skillUI.OnTileClicked(tileController.row, tileController.column);
                }
                return; // Don't let the click propagate to HexTileClickHandler
            }
        }
    }
}