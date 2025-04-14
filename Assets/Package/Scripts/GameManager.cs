using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance = null;

    public int money = 50;
    public int summonCount = 10;
    public List<Monster> monsters = new List<Monster>();
    int monsterCount;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void AddMonster(Monster monster)
    {
        monsters.Add(monster);
        monsterCount++;
    }

    public void RemoveMonster(Monster monster)
    {
        monsters.Remove(monster);
        monsterCount--;
    }

    [ClientRpc]
    private void MonsterCount_ClientRpc(int count)
    {

    }
}
