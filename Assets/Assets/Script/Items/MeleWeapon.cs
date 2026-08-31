using UnityEngine;
[CreateAssetMenu(menuName = "Data/Item/MeleWeapon", fileName = "MeleWeapon")]
public class Weapon : Item
{
    public float Damage;
    public float AttackDistance;
    public override void Use(CursorController C)
    {
        

    }
}
