using UnityEngine;

public class SkillTargetingClickHandler : MonoBehaviour
{
    void OnMouseDown()
    {
        // Only handle if skill UI is waiting for target selection
        if (AdventureSkillUI.Instance != null && AdventureSkillUI.Instance.IsWaitingForTargetSelection())
        {
            HexTileController tileController = GetComponent<HexTileController>();
            if (tileController != null)
            {
                AdventureSkillUI.Instance.OnTileClicked(tileController.row, tileController.column);
            }
        }
    }
}