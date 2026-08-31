using UnityEngine;

public class EnableOrDisableTogether : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    private void OnEnable()
    {
        obj.SetActive(true);
    }
    private void OnDisable()
    {
        obj.SetActive(false);
    }
}
