
using UnityEngine;

public class ButtonPressController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private UiController uiController;
    [SerializeField] private CursorController cursorController;

    public bool buttonCatch = true;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            uiController.openOrCloseMapMenu();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            uiController.PauseOrUnpauseGame();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            uiController.openOrCloseSettingsMenu();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            uiController.openOrCloseInventoryMenu();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiController.openOrCloseMainMenu();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            uiController.openOrCloseQuestsMenu();
        }
    }
}
