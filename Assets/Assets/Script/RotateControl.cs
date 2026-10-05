
using DG.Tweening;
using UnityEngine;

public class RotateControl : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float xSpeed;
    [SerializeField] private float ySpeed;
    [SerializeField] private float zSpeed;

    private void Start()
    {
        Vector3 rotateDir = new Vector3 (xSpeed, ySpeed, zSpeed);
        transform.DORotate(rotateDir, 1, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
    }
}

