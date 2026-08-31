using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using DG.Tweening;
public class UiController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PlayerControl playerControl;

    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    [SerializeField] TextMeshProUGUI playerName;

    //Inventory
    public GameObject Inventory;
    public List<GameObject> statsAddButtons;
    [Header("Head/Chest/Boots/Neck/Finger/Weapon")]
    public List<Cell> ArmorCells;

    public GameObject CurrentPlayer;
    public PlayerControl CurrentPlayerController;

    public GameObject StatsParent;

    public Stats[] list;
    public Stats[] sortedList;

    [SerializeField] private GameObject PlayerStatsParent; //Parent  Of Stats
    public List<Stats> stats;

    //Dialog    
    [Header("Dialog")]
    [SerializeField] private GameObject dialogMenu;
    [SerializeField] private UnityEngine.UI.Button[] dialogButtons;
    [SerializeField] private TextMeshProUGUI dialogText;
    private int dialogIndex;
    private Creature talkCreature;

    [SerializeField] private GameObject inGameUi;

    //InfoPanel
    [Header("InfoPanel")]
    [SerializeField] private TextMeshProUGUI infoPanelText;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private List<string> infoTexts;
    [SerializeField] private bool infoCour;


    //Spell Panel
    [Header("SpellPanel")]
    private GameObject ActiveSpeellPanel;
    private Sprite AfterEnable;
    private Sprite AfterDisable;
    private UnityEngine.UI.Image img;
    [SerializeField] private List<GameObject> panels;
    [SerializeField] private GameObject BackPanel;

    public bool timeStop = false;

    //Spells
    private List<GameObject> SortedPanels;
    public GameObject SpellCell;
    public GameObject[] SpellPanelBackObj;

    public GameObject CellPrefab;

    //LootTab
    [Header("LootTab")]
    [SerializeField] private GameObject LootTab;
    public List<Cell> lootCells = new List<Cell>();


    public bool isPauseAvable = true;
    public float lastTimeScale = 1f;
    [SerializeField] private GameObject pauseScreen;

    [SerializeField] private UnityEngine.UI.Image pauseButtonImg;
    [SerializeField] private Sprite pauseButtonStop;
    [SerializeField] private Sprite pauseButtonPlay;

    [Header("Quest Menu")]
    [SerializeField] private GameObject questsMenu;
    [SerializeField] private Quest[] questsAll;
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questProgressText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private UnityEngine.UI.Image lastImgButton;
    [SerializeField] private Scrollbar scrollBar;

    [SerializeField] private UnityEngine.UI.Button[] questButtons;

    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> completedQuests = new List<Quest>();


    [Header("Map Menu")]
    [SerializeField] private GameObject mapMenu;
    [SerializeField] private GameObject FogCamera;
    [SerializeField] private GameObject MapCamera;

    [SerializeField] private GameObject CreditsMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;
    private void Awake()
    {
        if (playerControl != null)
        {
            CurrentPlayerController = playerControl;
            CurrentPlayer = playerControl.gameObject;
        }
    }
    void Start()
    {


        UpdateList();

        // Вызываем с задержкой на 1 кадр для полной инициализации
        StartCoroutine(DelayedStart());
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private void OnEnable()
    {
        EventBus.setQuestPar += SetQuestPar;
    }
    private void OnDisable()
    {
        EventBus.setQuestPar -= SetQuestPar;
    }
    private IEnumerator DelayedStart()
    {
       // Ждем один кадр для полной инициализации всех компонентов
        yield return null;
        SetSkillPanel();
    }
    //InfoTab
    //use to show Info
    public void showTextInfo(string txt)
    {
        infoTexts.Add(txt);
        if (!infoCour)
        {
            StartCoroutine(InfoCour());
        }
    }
    private IEnumerator InfoCour()
    {
        infoCour = true;
        while (infoTexts.Count > 0)
        {
            yield return infoPanel.transform.DOMoveX(infoPanel.transform.position.x - 443, 1.5f).SetEase(Ease.OutBack);
            infoPanelText.text = infoTexts[0];
            yield return new WaitForSeconds(2);
            infoTexts.Remove(infoTexts[0]);
            yield return infoPanel.transform.DOMoveX(infoPanel.transform.position.x + 443, 1.5f).SetEase(Ease.OutBack);
        }
        infoCour = false;
    }
    public void OpenLootMenu(Lootable loot)
    {
        List<Item> items = loot.Items;
        LootTab.SetActive(true);
        foreach (Cell cell in lootCells)
        {
            cell.ClearSlot();
        }
        for (int i = 0; i < items.Count; i++)
        {
            lootCells[i].CurrentItem = items[i];
            lootCells[i].FillSlot();

        }
        Inventory.SetActive(true);

    }
    public void updateWearStats()
    {
        CurrentPlayerController.helmet = ArmorCells[0].CurrentItem as Armor;
        CurrentPlayerController.chest = ArmorCells[1].CurrentItem as Armor;
        CurrentPlayerController.pants = ArmorCells[2].CurrentItem as Armor;
        CurrentPlayerController.boots = ArmorCells[3].CurrentItem as Armor;
        CurrentPlayerController.bracers = ArmorCells[4].CurrentItem as Armor;
        CurrentPlayerController.cape = ArmorCells[5].CurrentItem as Armor;
        CurrentPlayerController.Weapon = ArmorCells[6].CurrentItem as Armor;
        CurrentPlayerController.SecondWeapon = ArmorCells[7].CurrentItem as Armor;
        CurrentPlayerController.updateArmorStats();
    }
    

    public void updateInventory()
    {
        //stats
        if (CurrentPlayerController.levelPoints > 0)
        {
            foreach (GameObject bt in statsAddButtons)
            {
                bt.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject bt in statsAddButtons)
            {
                bt.SetActive(false);
            }
        }
        playerName.text = CurrentPlayerController.name;
        //armorCells
        ArmorCells[0].CurrentItem = CurrentPlayerController.helmet;
        ArmorCells[1].CurrentItem = CurrentPlayerController.chest;
        ArmorCells[2].CurrentItem = CurrentPlayerController.pants;
        ArmorCells[3].CurrentItem = CurrentPlayerController.boots;
        ArmorCells[4].CurrentItem = CurrentPlayerController.bracers;
        ArmorCells[5].CurrentItem = CurrentPlayerController.cape;
        ArmorCells[6].CurrentItem = CurrentPlayerController.Weapon;
        ArmorCells[7].CurrentItem = CurrentPlayerController.SecondWeapon;
        foreach (Cell it in ArmorCells) { it.FillSlot(); }

        CurrentPlayerController.updateArmorStats();
        

    }

    // Add Player Stats
    public void fillStatsSlider(UnityEngine.UI.Image img)
    {
        img.fillAmount = Mathf.Clamp(img.fillAmount + 0.166f, 0, 1);
        CurrentPlayerController.levelPoints -= 1;
        updateInventory();
    }
    public void atributeStrengthAdd(int col)
    {
        CurrentPlayerController.Strength += 1;
    }
    public void atributeDexterityAdd(int col)
    {
        CurrentPlayerController.Dexterity += 1;
    }
    public void atributeConstitutionAdd(int col)
    {
        CurrentPlayerController.Constitution += 1;
    }
    public void atributeIntelligenceAdd(int col)
    {
        CurrentPlayerController.Intelligence += 1;
    }


    public void CloseUi()
    {
        closeMapMenu();
        closeMainMenu();
        Inventory.SetActive(false);

    }
    public void openOrCloseInventoryMenu()
    {
        if (Inventory.activeInHierarchy)
        {
            Inventory.SetActive(false);
        }
        else
        {
            updateInventory();
            Inventory.SetActive(true);
        }
    }
    public void closeInventoryMenu()
    {
        Inventory.SetActive(false);
    }
    //Pause Game
    public void PauseOrUnpauseGame()
    {
        if (isPauseAvable)
        {
            if (!timeStop)
            {
                timeStop = true;
                Time.timeScale = 0f;
                pauseButtonImg.sprite = pauseButtonPlay;
                pauseScreen.SetActive(true);
            }
            else
            {
                timeStop = false;
                pauseButtonImg.sprite = pauseButtonStop;
                Time.timeScale = 1f;
                pauseScreen.SetActive(false);
            }
        }

    }
    //menu
    public void openOrCloseMainMenu()
    {
        if (mainMenu.active)
        {
            isPauseAvable = true;
            Time.timeScale = lastTimeScale;
            mainMenu.SetActive(false);
        }
        else 
        { 
            lastTimeScale = Time.timeScale;
            isPauseAvable = false;
            Time.timeScale = 0f;
            mainMenu.SetActive(true); 
        }
    }
    public void closeMainMenu()
    {
        mainMenu.SetActive(false);
    }
    public void openOrCloseSettingsMenu()
    {
        if (settingsMenu.active)
        {
            settingsMenu.SetActive(false);
        }
        else
        {
            settingsMenu.SetActive(true);
        }
    }
    public void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
    }
    public void closeCreditsMenu()
    {
        CreditsMenu.SetActive(false);
    }
    public void openOrCloseCreditsMenu()
    {
        if (CreditsMenu.active)
        {
            CreditsMenu.SetActive(false);
        }
        else
        {
            CreditsMenu.SetActive(true);
        }
    }
    //map
    public void openOrCloseMapMenu()
    {
        if (mapMenu.active)
        {
            mapMenu.SetActive(false);
            FogCamera.SetActive(false);
            MapCamera.SetActive(false);
        }
        else
        {
            mapMenu.SetActive(true);
            FogCamera.SetActive(true);
            MapCamera.SetActive(true);
        }

    }
    public void closeMapMenu()
    {
        mapMenu.SetActive(false);
        FogCamera.SetActive(false);
        MapCamera.SetActive(false);
    }



    public void UpdateDiscription(Item item)
    {
        Description.text = item.Description;
        Name.text = item.Name;
    }
    public void UpdateList()
    {
        list = StatsParent.GetComponentsInChildren<Stats>(true);
        sortedList = StatsParent.GetComponentsInChildren<Stats>();
    }
    public void ActivateOrDisableAndChangeSprite(GameObject obj)
    {
        if (obj != ActiveSpeellPanel && ActiveSpeellPanel != null)
        {
            ActiveSpeellPanel.SetActive(false);
        }

        if (obj.activeInHierarchy)
        {
            obj.SetActive(false);
            if (img != null)
            {
                img.sprite = AfterDisable;
            }
        }
        else
        {
            obj.SetActive(true);
            if (img != null)
            {
                img.sprite = AfterEnable;
            }
        }
        ActiveSpeellPanel = obj;
    }
    public void EnableSetSkillPanelButoonSprite(Sprite sprite)
    {
        AfterEnable = sprite;
    }
    public void DisableSetSkillPanelButoonSprite(Sprite sprite)
    {
        AfterDisable = sprite;
    }
    public void SetSkillPanelButoonImage(UnityEngine.UI.Image image)
    {
        img = image;
    }



    public void SetTeamStats(List<PlayerControl> mainTeam)
    {
        //Get Current Stats Positions
        Stats[] stat = PlayerStatsParent.GetComponentsInChildren<Stats>();

        foreach (Stats control in stat)
        {
            if (!mainTeam.Contains(control.playerControl))
            {
                control.gameObject.SetActive(false);
            }
        }
    }
    //dialog
    //actions in dialog
    public void dialogContinueDialog(int index)
    {
        if (index != null)
        {        
            dialogIndex = index;
        }
        else
        {
            dialogIndex += 1;
        }

            DialogActions(talkCreature);
    }
    public void dialogCloseDialog()
    {
        dialogMenu.SetActive(false);
        inGameUi.SetActive(true);
        talkCreature = null;
    }
    public void dialogQuestActivate(Quest quest)
    {
        quest.isActive = true;
        showTextInfo("Quest received");
    }
    public void dialogStartFight()
    {
        
    }
    public void dialogStartBuyingBuy()
    {
        
    }
    public void dialogRemoveAnswers(int[][] ind)
    {
        
    }
    //
    public void OpenDialog(Creature creature)
    {
        dialogIndex = 0;
        DialogActions(creature);
        dialogMenu.SetActive(true);
        //inGameUi.SetActive(false);
    }
    public void DialogActions(Creature creature)//0-continue 1-leave 2-fight 3-buy 4-take quest
    {
        talkCreature = creature;
        UnityEvent[] action = creature.dialogControl.Actions[dialogIndex].events;
        dialogText.text = creature.dialogControl.Texts[dialogIndex];

        for (int i = 0; i < dialogButtons.Length; i++)
        {
            if (i < action.Length)
            {
                dialogButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = creature.dialogControl.Answers[dialogIndex].array[i];
                dialogButtons[i].onClick.RemoveAllListeners();
                UnityEvent actionCode = action[i];
                dialogButtons[i].onClick.AddListener(() => actionCode.Invoke());
                dialogButtons[i].gameObject.SetActive(true);
            }
            else
            {
                dialogButtons[i].gameObject.SetActive(false);
            }
        }


    }
    //Quest
    private void SetQuestPar(string name, int col, bool add)
    {
        foreach (Quest q in questsAll)
        {
            if (q.questName == name)
            {
                if (q.fulfillOnlyWhenActive && q.isActive || !q.fulfillOnlyWhenActive)
                {
                    if (add)
                    {
                        q.currentProgress = Mathf.Clamp(q.currentProgress + col, 0, q.maxProgress);
                    }
                    else
                    {
                        q.currentProgress = Mathf.Clamp(col, 0, q.maxProgress);
                    }
                    break;
                }
            }
        }
    }

    public void openOrCloseQuestsMenu()
    {
        if (questsMenu.active)
        {
            questsMenu.SetActive(false);
        }
        else
        {
            questsMenu.SetActive(true);
            updateQuestMenu(true);
            updateCurrentQuest(activeQuests[0]);
        }
    }
    public void blackQuestButton(UnityEngine.UI.Image img)
    {
        if (lastImgButton != null )
        {
            lastImgButton.color = Color.white;
        }
        img.color = new Color(1, 1, 1, 0.7f);
        lastImgButton = img;
    }
    public void updateCurrentQuest(Quest quest)
    {
        questDescriptionText.text = quest.description;
        questTitleText.text = quest.title;

        questProgressText.text = $"{quest.currentProgress}, {quest.maxProgress}";
    }
    public void updateQuestMenu(bool isActiveQuests)
    {
        foreach (UnityEngine.UI.Button bt in questButtons)
        {
            bt.gameObject.SetActive(false);
        }
        if (isActiveQuests)
        {
            activeQuests.Clear();
            foreach (Quest qst in questsAll)
            {
                if (qst.isActive && !qst.isCompleted)
                {
                    activeQuests.Add(qst);
                }
            }
            foreach (Quest qst in activeQuests)
            {
                foreach (UnityEngine.UI.Button bt in questButtons)
                {
                    if (bt.gameObject.name == qst.questName)
                    {
                        bt.gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
        else
        {
            completedQuests.Clear();
            foreach (Quest qst in questsAll)
            {
                if (qst.isCompleted)
                {
                    completedQuests.Add(qst);
                }
            }
            foreach (Quest qst in completedQuests)
            {
                foreach (UnityEngine.UI.Button bt in questButtons)
                {
                    if (bt.name == qst.questName)
                    {
                        gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
        scrollBar.value = 1;
    }

    //при выборе персонажа меняет панель способностей
    public void SetSkillPanel()
    {
        CurrentPlayerController.UpdateAllSpells();
        //Установка количества панелей
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
        int ind = 0;
        for (int i = 0; i< CurrentPlayerController.SkillActivated.Count; i++)
        {
            if (CurrentPlayerController.SkillActivated[i] == 1)
            {
                panels[i].SetActive(true);
                ind = i;
            }
        }
        //количество способностей в панелях (панель -> ячейка)
        for (int i = 0; i < CurrentPlayerController.AllSpells.Length; i++)
        {
            foreach (Transform child in SpellPanelBackObj[i + 2].transform)
            {
                child.gameObject.SetActive(false);
            }
            //[1][2][3][4][5]
            if (CurrentPlayerController.AllSpells[i] != null)
            {
                for (int index = 0; index < CurrentPlayerController.AllSpells[i].Length; index++)
                {
                    Debug.Log(CurrentPlayerController.AllSpells[i].Length);
                    if (CurrentPlayerController.AllSpells[i][index] != null)
                    {
                        Transform cel = SpellPanelBackObj[i + 2].transform.GetChild(index);
                        Cell cellControl = cel.gameObject.GetComponent<Cell>();

                        cellControl.CurrentSpell = CurrentPlayerController.AllSpells[i][index];
                        cel.gameObject.SetActive(true);
                        cellControl.FillSlot();
                    }
                }
                RectTransform contr = SpellPanelBackObj[i+2].GetComponent<RectTransform>();
                contr.anchoredPosition = new Vector3(0, Mathf.Ceil(CurrentPlayerController.AllSpells[i].Length / 2.0f) * 50f + 75f, 0);
                contr.sizeDelta = new Vector2(100f, Mathf.Ceil(CurrentPlayerController.AllSpells[i].Length / 2.0f) * 50f);




            }

        }
        BackPanel.transform.position = panels[ind].transform.position;
    }
}
