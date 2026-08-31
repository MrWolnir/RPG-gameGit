
using UnityEngine;

public class Stats : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CursorController cursorController;

    public PlayerControl playerControl;
    public GameObject player;

    public Sprite sprite;

    public GameObject RGBBack;

    public UnityEngine.UI.Image icon;

    private void Awake()
    {
        if (playerControl != null)
        {
            sprite = playerControl.Icon;
            icon.sprite = sprite;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    //T ochange player
    public void SetPanel(PlayerControl control)
    {
        icon.sprite = control.Icon;
        player = control.gameObject;
        playerControl = control;
        gameObject.SetActive(true);

    }
    public void ActivateRgb()
    {
        RGBBack.SetActive(true);
    }
    public void DisableRgb()
    {
        RGBBack.SetActive(false);
    }

    public void OnClick()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
        {
            if (cursorController.selectedTeam.Contains(playerControl))
            {
                cursorController.RemoveFromCurrentTeam(playerControl);
            }
            else
            {
                cursorController.AddToCurrentTeam(playerControl);
            }
        }
        else
        {
            cursorController.SetOnePersonTeam(playerControl);
            cursorController.StartCoroutine(cursorController.DragStat(sprite, playerControl, player, this));
        }
    }
}

