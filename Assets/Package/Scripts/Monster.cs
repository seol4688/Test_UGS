using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Monster : Character
{

    [SerializeField] protected float speed;
    [SerializeField] HitText HitText;
    [SerializeField] Image m_Fill;
    [SerializeField] Image m_Fill_Deco;

    public int targetValue = 0;
    public int HP, MaxHP, Gold;
    bool isDead = false;
    public List<Vector2> movePath = new List<Vector2>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        HP = MaxHP;
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        m_Fill_Deco.fillAmount = Mathf.Lerp(m_Fill_Deco.fillAmount, m_Fill.fillAmount, Time.deltaTime * 2f);

        if (isDead)
            return;

        var target = movePath[targetValue];
        transform.position = Vector2.MoveTowards(transform.position, target, Time.deltaTime * speed);
        if (Vector2.Distance(transform.position, target) <= 0.01f)
        {
            targetValue++;
            spriteRenderer.flipX = targetValue >= 3 ? true : false;
            if (targetValue >= movePath.Count)
            {
                targetValue = 0;
            }
        }
    }

    public void Init(List<Vector2> path)
    {
        movePath = path;
    }

    public void GetDamage(int dmg)
    {
        if (!IsServer) return;
        if (isDead) return;

        GetDamageMonater(dmg);
        GetDamageMonater_ClientRpc(HP -= dmg, dmg);
    }

    private void GetDamageMonater(int dmg)
    {
        HP -= dmg;
        m_Fill.fillAmount = (float)HP / MaxHP;
        Instantiate(HitText, transform.position, Quaternion.identity).Initalize(dmg);

        if (HP <= 0)
        {
            isDead = true;
            gameObject.layer = LayerMask.NameToLayer("Default");
            //GameManager.instance.money += Gold;
            //GameManager.instance.RemoveMonster(this);
            AnimatorChange("DEAD", true);
            StartCoroutine(IE_Dead());
        }
    }

    [ClientRpc]
    public void GetDamageMonater_ClientRpc(int hp , int dmg)
    {
        HP  = hp;
        m_Fill.fillAmount = (float)HP / MaxHP;
        Instantiate(HitText, transform.position, Quaternion.identity).Initalize(dmg);

        if (HP <= 0)
        {
            isDead = true;
            gameObject.layer = LayerMask.NameToLayer("Default");
            //GameManager.instance.money += Gold;
            //GameManager.instance.RemoveMonster(this);
            AnimatorChange("DEAD", true);
            StartCoroutine(IE_Dead());
        }
    }

    IEnumerator IE_Dead()
    {
        float Alpha = 1.0f;

        while (spriteRenderer.color.a > 0f)
        {
            Alpha -= Time.deltaTime;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Alpha);

            yield return null;
        }

        DestroyMonster();
        //if (IsServer)
        //{
        //    DestroyMonster();
        //}
        //else if(IsClient)
        //{
        //    DestroyMonster_ServerRpc();
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyMonster_ServerRpc()
    {
        DestroyMonster();
    }

    private void DestroyMonster()
    {
        if (IsServer)
            NetworkObject.Destroy(this);
    }
}
