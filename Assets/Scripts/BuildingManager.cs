using System.Collections.Generic;
using UnityEngine;

public enum BuildingStatus
{
    Idle,
    Working,
    Upgrading,
}

[System.Serializable]
public class Building
{
    public string buildingType;
    public int buildingID;
    public int level;
    public BuildingStatus status;
    
    public Building(string type, int ID)
    {
        buildingType = type;
        buildingID = ID;
        level = 1;
        status = BuildingStatus.Idle;
    }
}

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private List<Building> buildings = new List<Building>();
    
    void Start()
    {
        // Subscribe to next day events to handle building status updates
        NextDayEvents.OnNextDay += OnNextDay;
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        NextDayEvents.OnNextDay -= OnNextDay;
    }
    
    private void OnNextDay()
    {
        // Process all upgrading buildings
        foreach (Building building in buildings)
        {
            if (building.status == BuildingStatus.Upgrading)
            {
                building.level++;
                building.status = BuildingStatus.Idle;
                Debug.Log($"Building {building.buildingType} (ID: {building.buildingID}) upgrade completed! New level: {building.level}");
            }
        }
    }
    
    public void AddBuilding(string buildingType, int ID)
    {
        // Check if building already exists
        Building existingBuilding = GetBuildingByID(ID);
        if (existingBuilding == null)
        {
            Building newBuilding = new Building(buildingType, ID);
            buildings.Add(newBuilding);
        }
    }
    
    public Building GetBuilding(int index)
    {
        if (index >= 0 && index < buildings.Count)
        {
            return buildings[index];
        }
        return null;
    }
    
    public Building GetBuildingByID(int buildingID)
    {
        foreach (Building building in buildings)
        {
            if (building.buildingID == buildingID)
            {
                return building;
            }
        }
        return null;
    }
    
    public List<Building> GetAllBuildings()
    {
        return buildings;
    }
    
    public void SetBuildingStatus(int buildingID, BuildingStatus status)
    {
        Building building = GetBuildingByID(buildingID);
        if (building != null)
        {
            building.status = status;
        }
    }
    
    public void SetBuildingStatusByIndex(int index, BuildingStatus status)
    {
        if (index >= 0 && index < buildings.Count)
        {
            buildings[index].status = status;
        }
    }
    
    public void UpgradeBuilding(int buildingID)
    {
        Building building = GetBuildingByID(buildingID);
        if (building != null && building.status != BuildingStatus.Upgrading)
        {
            building.status = BuildingStatus.Upgrading;
            Debug.Log($"Building {building.buildingType} (ID: {buildingID}) started upgrading");
        }
    }
    
    public void UpgradeBuildingByIndex(int index)
    {
        if (index >= 0 && index < buildings.Count)
        {
            buildings[index].level++;
        }
    }
    
    public int GetUpgradingBuildingsCount()
    {
        int count = 0;
        foreach (Building building in buildings)
        {
            if (building.status == BuildingStatus.Upgrading)
            {
                count++;
            }
        }
        return count;
    }
    
    public List<Building> GetBuildingsByStatus(BuildingStatus status)
    {
        List<Building> filteredBuildings = new List<Building>();
        foreach (Building building in buildings)
        {
            if (building.status == status)
            {
                filteredBuildings.Add(building);
            }
        }
        return filteredBuildings;
    }
}