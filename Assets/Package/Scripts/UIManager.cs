using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI monsterCountText;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI summonText;

    private void Update()
    {
        monsterCountText.text = GameManager.instance.monsters.Count.ToString() + " / 100";
        moneyText.text = GameManager.instance.money.ToString();
        summonText.text = GameManager.instance.summonCount.ToString();

        summonText.color = GameManager.instance.money < GameManager.instance.summonCount ? Color.red : Color.white;
    }
}
