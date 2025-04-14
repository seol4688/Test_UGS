using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;

public class Spawner : NetworkBehaviour
{
    [SerializeField] Hero spawnPrefab;
    [SerializeField] Monster spawnMonsterPerfab;

    public List<Vector2> Player_Move_List = new List<Vector2>();
    public List<Vector2> Rival_Move_List = new List<Vector2>();

    List<Vector2> Player_Summon_Point = new List<Vector2>();
    List<Vector2> Rival_Summon_Point = new List<Vector2>();
    List<bool> Player_Summon_Possible = new List<bool>();
    List<bool> Rival_Summon_Possible = new List<bool>();

    [SerializeField] float spawnDelay = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetGrid();
        StartCoroutine(IES_pawnMonsterSummon());
    }

    private void SetGrid()
    {
        SetSummonPoint(transform.GetChild(0), true);
        SetSummonPoint(transform.GetChild(1), false);

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            Player_Move_List.Add(transform.GetChild(0).GetChild(i).position);
        }

        for (int i = 0; i < transform.GetChild(1).childCount; i++)
        {
            Rival_Move_List.Add(transform.GetChild(1).GetChild(i).position);
        }
    }

    void SetSummonPoint(Transform tf, bool playr)
    {
        SpriteRenderer parentSprite = tf.GetComponent<SpriteRenderer>();
        float parentWidth = parentSprite.bounds.size.x;
        float parentHeight = parentSprite.bounds.size.y;

        float xCount = tf.localScale.x / 6;
        float yCount = tf.localScale.y / 3;
        for (int y = 0; y < 3; y++)   //»óÇÏ
        {
            for (int x = 0; x < 6; x++)   //ÁÂ¿ì
            {
                float xPos = (-parentWidth / 2) + (x * xCount) + (xCount / 2);
                float yPos = ((playr ? parentHeight : -parentHeight) / 2) + ((playr ? -1 : 1) * (y * yCount)) + (yCount / 2);
                switch (playr)
                {
                    case true:
                        Player_Summon_Point.Add(new Vector2(xPos, yPos + tf.localPosition.y - yCount));
                        Player_Summon_Possible.Add(false);
                        break;
                    case false:
                        Rival_Summon_Point.Add(new Vector2(xPos, yPos + tf.localPosition.y));
                        Rival_Summon_Possible.Add(false);
                        break;
                }
            }
        }
    }
    #region HeroSummon
    public void OnClickSummon()
    {
        //if (GameManager.instance.money < GameManager.instance.summonCount)
        //    return;

        //GameManager.instance.money -= GameManager.instance.summonCount;
        //GameManager.instance.summonCount += 2;



        if (IsClient)
        {
            ServerHeroSpawn_ServerRpc(LocalID());
        }
        else if (IsServer)
        {
            HeroSpawn(LocalID());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerHeroSpawn_ServerRpc(ulong clientID)
    {
        HeroSpawn(clientID);
    }

    private void HeroSpawn(ulong clientID)
    {
        var go = Instantiate(spawnPrefab);
        NetworkObject networkObject = go.GetComponent<NetworkObject>();
        networkObject.Spawn();

        ClientHero_ClientRpc(networkObject.NetworkObjectId, clientID);
    }

    [ClientRpc]
    private void ClientHero_ClientRpc(ulong networkObjectID, ulong clientID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectID, out NetworkObject hero))
        {
            if (clientID == LocalID())
            {
                SetHeroPositon(hero, true);
            }
            else
            {
                SetHeroPositon(hero, false);
            }
        }
    }

    private void SetHeroPositon(NetworkObject obj, bool Player)
    {
        List<bool> Summon_Possible = Player ? Player_Summon_Possible : Rival_Summon_Possible;
        List<Vector2> Summon_Point = Player ? Player_Summon_Point : Rival_Summon_Point;

        int posValue = -1;
        for (int i = 0; i < Summon_Possible.Count; i++)
        {
            if (Summon_Possible[i] == false)
            {
                posValue = i;
                Summon_Possible[i] = true;
                break;
            }
        }
        obj.transform.position = Summon_Point[posValue];
    }
    #endregion
    #region MonsterSummon
    IEnumerator IES_pawnMonsterSummon()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (IsClient)
        {
            ServerMonsterSpawn_ServerRpc(LocalID());
        }
        else if (IsServer)
        {
            MonsterSpawn(LocalID());
        }

        StartCoroutine(IES_pawnMonsterSummon());
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerMonsterSpawn_ServerRpc(ulong clientID)
    {
        MonsterSpawn(clientID);
    }

    private void MonsterSpawn(ulong clientID)
    {
        var go = Instantiate(spawnMonsterPerfab);
        NetworkObject networkObject = go.GetComponent<NetworkObject>();
        networkObject.Spawn();

        ClientMonster_ClientRpc(networkObject.NetworkObjectId, clientID);
    }

    [ClientRpc]
    private void ClientMonster_ClientRpc(ulong networkObjectID, ulong clientID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectID, out NetworkObject monster))
        {
            if (clientID == LocalID())
            {
                monster.transform.position = Player_Move_List[0];
                monster.GetComponent<Monster>().Init(Player_Move_List);
            }
            else
            {
                monster.transform.position = Rival_Move_List[0];
                monster.GetComponent<Monster>().Init(Rival_Move_List);
            }
        }
    }
    #endregion

    private ulong LocalID()
    {
        return NetworkManager.Singleton.LocalClientId;
    }
}
