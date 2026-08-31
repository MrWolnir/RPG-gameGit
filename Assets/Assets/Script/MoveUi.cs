using UnityEngine;
using UnityEngine.EventSystems;

public class MoveUi : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [SerializeField] private GameObject MoveObject;

    private Vector3 OldPos;
    private Vector3 NewPos;
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerDrag.tag);
        if (eventData.pointerDrag.CompareTag("Movable"))
        {
            OldPos = Input.mousePosition;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag.CompareTag("Movable"))
        {
            NewPos = Input.mousePosition;
            MoveObject.transform.position += (NewPos - OldPos);
            OldPos = NewPos;
        }
    }
}
