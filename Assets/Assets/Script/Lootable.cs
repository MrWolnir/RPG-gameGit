using System.Collections.Generic;
using UnityEngine;

public class Lootable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is create
    //public GameObject BlueOverlay;
    [SerializeField] private MeshRenderer mesh;

    [SerializeField] private Material materialNormal;
    [SerializeField] private Material materialBlue;

    public float _DistanceToLoot;
    public List<Item> Items;
    private void OnMouseEnter()
    {
        mesh.material = materialBlue;

    }
    private void OnMouseExit()
    {
        mesh.material = materialNormal;
    }
}
