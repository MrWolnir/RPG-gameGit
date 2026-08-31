using UnityEngine;
using DG.Tweening;

public class BodyBag : MonoBehaviour
{
    [SerializeField] private Renderer bodyBagRender;
    private void OnEnable()
    {
        Color newColor = bodyBagRender.material.color;
        newColor.a = 0f;
        bodyBagRender.material.color = newColor;
        bodyBagRender.material.DOFade(1f, 2f);
    }
}
