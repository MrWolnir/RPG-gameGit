using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UnityEngine.UI.Image image;
    public bool wearCell;
    [Header("CanUse")]
    public bool isBlocked;
    

    [Header("Armor")]
    public bool helmet;
    public bool chest;
    public bool pants;
    public bool boots;
    public bool bracers;
    public bool cape;
    [Header("Else")]
    public bool Weapon;
    public bool SecondWeapon;
    public bool Resource;
    public bool Spell;
    public CursorController _CursorController;

    public Sprite currentSprite;
    public Image currentImage;
    public Sprite AddSprite;

    public Spell CurrentSpell;
    public Item CurrentItem;

    public int count = 0;

    [SerializeField] private bool multiplyable;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private bool isChangeColor;
    [SerializeField] private Color changeColor;





    private void Awake()
    {
        if (_CursorController == null) 
        {
            _CursorController = Object.FindFirstObjectByType<CursorController>();
        }
    }
    void Start()
    {

        FillSlot();
    }

    // Update is called once per frame
    public void OnClick()
    {
        if (_CursorController == null)
        {
            Debug.Log("Cursor is null");
        }
        else if (CurrentItem != null)
        {
            _CursorController.StartCoroutine(_CursorController.DragItem(CurrentItem, this));
            //CurrentItem.Use(_CursorController);
        }
        else if (CurrentSpell != null)
        {
            Debug.Log("SpellClicked");
            CurrentSpell.Use(_CursorController);
        }
    }
    public void ClearSlot()
    {
        if (wearCell) { currentImage.sprite = currentSprite; }
        CurrentItem = null;
        image.sprite = null;
        image.color = new Color(1f, 1f, 1f, 0f);
        if (text != null) {    text.text = "";}
        
    }
    public void FillSlot()
    {
        if (CurrentItem != null && CurrentItem.Icon != null)
        {
            if (wearCell) { currentImage.sprite = _CursorController.cellBasic; }
            image.sprite = CurrentItem.Icon;
            if (isChangeColor)
            { image.color = changeColor; }
            else
            {
                image.color = Color.white;
            }
            multiplyable = CurrentItem.multiplyable;
            if (multiplyable)
            {
                text.text = count.ToString();
            }
            else
            {
                text.text = "";
            }
        }
        else if (CurrentSpell != null && CurrentSpell.Icon != null)
        {
            image.sprite = CurrentSpell.Icon;
            if (isChangeColor)
            { image.color = changeColor; }
            else
            {image.color = Color.white;}
        }
        else if (AddSprite != null)
        {
            image.sprite = AddSprite;
            if (isChangeColor)
            { image.color = changeColor; }
            else
            {image.color = Color.white;}
        }
        else
        {
            ClearSlot();
        }
    }
}
