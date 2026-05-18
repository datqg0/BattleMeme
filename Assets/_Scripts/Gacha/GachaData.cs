using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GachaItem
{
    public string id;
    public string characterName;
    public string rarity; 
    public float rate; 
    public string prefabPath; 
    public bool isCurrency;   
    public int coinReward;
    public int ticketReward;
}

[Serializable]
public class GachaPool
{
    public List<GachaItem> items = new List<GachaItem>();
}

[Serializable]
public class PlayerInventory
{
    public int coinCount;
    public int ticketCount;
    public List<string> ownedCharacterIds = new List<string>();
    public List<string> selectedTeamIds = new List<string>();
}
[Serializable]
public class TeamData
{
    public List<string> selectedHeroIds = new List<string>();
}
