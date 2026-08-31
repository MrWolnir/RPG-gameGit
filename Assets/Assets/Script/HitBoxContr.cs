using System.ComponentModel.Design;
using UnityEngine;

public class HitBoxContr : MonoBehaviour
{
    [SerializeField] private bool DisableCircleFromMouse;
    public bool tempBlockCircle = false;
    [SerializeField] private CursorController cursorController;

    [SerializeField] private PlayerControl control;
    [SerializeField] private Creature creature;

    [SerializeField] private MeshRenderer mesh;

    [SerializeField] private Material enemyMaterial;
    [SerializeField] private Material enemyMaterialLight;
    [SerializeField] private Material neutralBase;
    [SerializeField] private Material neutralLight;
    [SerializeField] private Material friendMaterial;
    [SerializeField] private Material friendLightMaterial;

    private void OnMouseEnter()
    {
        if (control != null)
        {
            if (!control.cursorController.PrepearingCastSpell)
            {
                mesh.material = friendLightMaterial;
                if (DisableCircleFromMouse)
                {
                    if (!tempBlockCircle)
                    {
                        control.SelectCircle.SetActive(true);
                    }
                }
                if (!control.InFight)
                {
                    control.hpBarController.gameObject.SetActive(true);
                }
            }
        }
        else if (creature != null)
        {
            if (creature.isEnemy)
            {
                mesh.material = enemyMaterialLight;
            }
            else
            {
                mesh.material = neutralLight;
            }
            if (DisableCircleFromMouse)
            {
                if (!tempBlockCircle)
                {
                    creature.SelectCircle.SetActive(true);
                }
            }
            if (!creature.InFight)
            {
                creature.hpBarController.gameObject.SetActive(true);
            }
        }
    }
    private void OnMouseExit()
    {
        if (control != null)
        {
            if (!control.cursorController.PrepearingCastSpell)
            {
                mesh.material = friendMaterial;
                if (DisableCircleFromMouse)
                {
                    if (!tempBlockCircle)
                    {
                        //control.SelectCircle.SetActive(false);
                    }
                }
                if (!control.InFight)
                {
                    control.hpBarController.gameObject.SetActive(false);
                }
            }
        }
        else if (creature != null)
        {
            if (creature.isEnemy)
            {
                mesh.material = enemyMaterial;
            }
            else
            {
                mesh.material = neutralBase;
            }
            if (DisableCircleFromMouse)
            {
                if (!tempBlockCircle)
                {
                    //creature.SelectCircle.SetActive(false);
                }
            }
            if (!creature.InFight)
            {
                creature.hpBarController.gameObject.SetActive(false);
            }
        }
    }
}
