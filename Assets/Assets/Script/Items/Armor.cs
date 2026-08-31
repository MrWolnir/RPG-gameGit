using UnityEngine;
[CreateAssetMenu(menuName = "Data/Item/Armor", fileName = "Armor")]
public class Armor : Item
{
    [Header("Armor")]
    public bool helmet;
    public bool chest;
    public bool pants;
    public bool boots;
    public bool bracers;
    public bool cape;

    [Header("Is Weapon?")]
    public bool weapon;
    public bool SecondWeapon;


    [Header("Weapon type")]
    public bool RangeStaff;
    public bool Sword;
    public bool GreatSword;
    public bool Dagger;
    public bool Bow;

    [Header("Only for weapon")]
    public bool twoHand;

    [Header("Armor equip 0) light 1) medium 3) heavy 4.. 5..")]
    public int[] indexes;
    //Stats
    [Header("Stats")]
    public int defence;

    public int Strength; //damage and block
    public int Dexterity; // Damage and dodge
    public int Constitution; // Hp
    public int Intelligence; // Mage Damage

    public bool disableChest;
    public bool disablePants;

    public float stamina;
    public int Damage;
    public float AttackDistance;
    [Header("For Animation")]
    public float AttackDurationBefore;
    public float AttackDurationAfter;

    public override void Use(CursorController C)
    {
        Debug.Log("Armor");
    }
}
