using UnityEngine;

[CreateAssetMenu(menuName = "Data/Item/BaseItem", fileName = "BaseItem")]
public class Item : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public bool multiplyable;

    public virtual void Use(CursorController _CursorController)
    {

    }
}

//[System.Serializable]
//public class ItemInf: MonoBehaviour
//{
//    public Item Data;
//    public int count;
//    public int durability;

//}

