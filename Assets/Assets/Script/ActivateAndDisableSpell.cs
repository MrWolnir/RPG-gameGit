using System.Collections;
using UnityEngine;

public class ActivateAndDisableSpell : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject HitBox;

    public float HitBoxActivateInterval;
    public float HitBoxDisbleInterval;
    public float GameobjectDisbleInterval;

    private void OnEnable()
    {
        StartCoroutine(ActivateSpell());
    }
    public IEnumerator ActivateSpell()
    {
        yield return new WaitForSeconds(HitBoxActivateInterval);
        HitBox.SetActive(true);
        yield return new WaitForSeconds(HitBoxDisbleInterval);
        HitBox.SetActive(false);
        yield return new WaitForSeconds(GameobjectDisbleInterval);
        gameObject.SetActive(false);
    }
}
