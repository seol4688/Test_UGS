using Unity.Netcode;
using UnityEngine;

public class Hero : Character
{
    public float attackRange = 1.0f;
    public float attackSpeed = 1.0f;
    public float attackDelay = 1.0f;
    public int attackDMG = 10;
    public NetworkObject target;
    [SerializeField] protected LayerMask enemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForEnemies();
    }

    void CheckForEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackRange, enemy);
        attackSpeed += Time.deltaTime;

        if (enemiesInRange.Length > 0)
        {
            target = enemiesInRange[0].transform.GetComponent<NetworkObject>();
            if (attackSpeed >= attackDelay)
            {
                attackSpeed = 0f;
                //AttackEnemy(target);
                AnimatorChange("ATTACK", true);
                AttackMonster_ServerRpc(target.NetworkObjectId);
            }
        }
        else
        {
            target = null;
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackMonster_ServerRpc(ulong monsterID)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(monsterID, out var obj))
        {
            Monster monster = obj.GetComponent<Monster>();
            if(monster != null)
            {
                monster.GetDamage(attackDMG);
            }
        }
    }

    //void AttackEnemy(Monster enemy)
    //{
    //    AnimatorChange("ATTACK", true);
    //    enemy.GetDamage(attackDMG);
    //}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
