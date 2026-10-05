using UnityEngine;

public class hpText : MonoBehaviour
{
    [SerializeField] private Animation anim;

    private void OnEnable()
    {
        anim.Play();
    }
    public void disable()
    {
        gameObject.SetActive(false);
    }
}
