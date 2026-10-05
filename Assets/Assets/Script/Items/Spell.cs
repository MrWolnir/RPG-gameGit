using UnityEngine;
[CreateAssetMenu(menuName = "Data/Spell/BaseSpell", fileName = "BaseSpell")]
public class Spell : ScriptableObject
{
    public bool isBuff;
    public bool isHeal;

    [Header("1 - player; 2 - enemy; 3 - all")]
    public int mask; // 1 - player; 2 - enemy; 3 - all

    public string Name;
    public Sprite Icon;
    public int Count = 1; //multiple cust (!now casts at same moment)

    public float CastRadius;
    public float ÑastTimer;
    public float Kd;

    [Header("Check in CursorConroller (spells)")]
    public int SpellTransformIndex;

    [Header("Object under cursor")]
    public int SpellPrepareInd;

    public bool CastAtPoint; // false - cast from player

    public bool CastFromPlayer; // false - cast from player

    public bool CastDirected; // false - cast from player
    [Header("False - SpellAnimation")]
    public bool AttackCastAnimation;
    [Header("OnlyForAttackAnim")]
    public float AdditionalDamage;

    public virtual void Use(CursorController cursorController)
    {
        Debug.Log("Spl");
        cursorController.PrepareSpell(this);
    }
}
