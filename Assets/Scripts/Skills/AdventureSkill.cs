using UnityEngine;

public abstract class AdventureSkill : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public int mpCost;
    public int cooldownSteps;
    public bool requiresTargeting;
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description;
    
    // Override these in specific skill implementations
    public abstract void ExecuteSkill(int skillLevel, Vector2Int? targetTile = null);
    public abstract bool IsValidTarget(int row, int col);
    public virtual void ApplyTargetingVisuals() { }
    public virtual void RemoveTargetingVisuals() { }
}