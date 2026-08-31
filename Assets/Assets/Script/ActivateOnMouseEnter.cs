using UnityEngine;
using UnityEngine.EventSystems;

public class ActivateOnMouseEnter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //private void OnMouseEnter()
    //{
    //    gameObject.SetActive(true);
    //}
    //private void OnMouseExit()
    //{
    //    gameObject.SetActive(false);
    //}
    public GameObject Obj;
    public void OnPointerEnter(PointerEventData eventData)
    {
        Obj.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Obj.SetActive(false);
    }
}
