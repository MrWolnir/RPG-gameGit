using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public float hp;
    public float maxHp;

    public float stamina;
    public float maxStamina;



    [SerializeField] private HpBarControlelr barController;

    public GameObject SelectCircle;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        barController.UpdateStamina(1, 1);

    }
    public void DamageTake(float damage)
    {
        hp -= damage;
        barController.UpdateHp(hp, maxHp);

    }

    public void GetHeal(float heal)
    {
        hp+= heal;
        barController.UpdateHp(hp,maxHp);
    }
}
