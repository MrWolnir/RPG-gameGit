using GDS.Core.Events;
using sc.terrain.proceduralpainter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Unity.Burst.Intrinsics;
using Unity.Cinemachine;
using Unity.Multiplayer.PlayMode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AdaptivePerformance.Provider;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{//cursor texture
    [Header("Cursor Texture")]
    public Texture2D DefaultCursour;
    public Texture2D ClickedCursour;
    public Texture2D CantClickCursour;

    public UnityEngine.Sprite cellBasic;
    public Color cellBlur;
    //Drag Item
    [Header("Drag Item")]
    public Image CursorDragPicture;
    public GameObject CursorDragObj;
    private bool IsDrag;
    private bool IsDragStats;
    public Stats MovingStats;

    public UiController uiControl;


    [Header("Current Player")]
    private GameObject CurrentPlayer;
    private Rigidbody CurrentPlayerRb;
    public PlayerControl CurrentPlayerContr;

    public PlayerControl MainHero;

    //Party
    public List<PlayerControl> mainTeam; //full team
    public List<PlayerControl> selectedTeam; //To move

    public List<Creature> enemyTeam;

    [Header("Camera Sett")]
    public float xLeft;
    public float xRight;
    public float zUp;
    public float zDown;
    private bool moveCam;

    private float FOVSpeed = 2509f;
    private float MinCameraFOV = 45f;
    private float MaxCameraFOV = 77f;
    private float CurrentCameraFOV = 67f;
    public GameObject _CameraObj;
    public CinemachineCamera _Camera;
    public GameObject cameraTrackObj;
    public bool cameraFollow;
    public PlayerControl cameraFollowObj;

    [SerializeField] public bool PrepearingCastSpell; 
    [SerializeField] private GameObject DragSpellObj;//Circle under cursor while preparing spell
    public float ScreenSpeed;
    public GameObject UiCanvas;

    //PrepearedSkill
    [Header("Prepeared Skill")]
    [SerializeField] private Spell spell;
    [SerializeField] private Transform SpellTransform;
    [SerializeField] private List<Transform> SpellTransforms =new List<Transform>();
    public float expMulti;

    [Header("layers")]
    public LayerMask UiLayer;
    public LayerMask TalkOrFightLayer;
    public LayerMask LootLayer;
    public LayerMask GroundLayer;

    [Header("Visualize WayPoints")]
    [SerializeField] private float distLayers;
    [SerializeField] private float distBetw;

    private float FocusCameraTimer;
    private Vector3 MousePos;
    private Vector3 OldMousePos;

    private Vector3 FollowOffset = new Vector3(0f, 6.01999998f, -4.05000019f);

    private Transform PlayerTargetPos = null;

    private Item item;
    private Cell cell;



    private bool VisualizePoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
     
        foreach (Transform tr in SpellTransform)
        { SpellTransforms.Add(tr.transform); }
        CurrentPlayer = GameObject.Find("Player");
        CurrentPlayerRb = CurrentPlayer.GetComponent<Rigidbody>();
        CurrentPlayerContr = CurrentPlayer.GetComponent<PlayerControl>();

        uiControl.CurrentPlayer = CurrentPlayer;
        uiControl.CurrentPlayerController = CurrentPlayerContr;
        selectedTeam.Add(CurrentPlayerContr);
        CurrentPlayerContr.isSelected = true;
    }

    // Update is called once per frame
    void Update()
    {
        CameraMovement();

        DragItems();
        DragStats();
        ClickControl();

    }
    private void FixedUpdate()
    {
        VisualiseSpellnderCursor();
    }
    public List<RaycastResult> IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster raycaster = UiCanvas.GetComponent<GraphicRaycaster>();
        raycaster.Raycast(eventData, results);

        List <RaycastResult> sortedResult = new List<RaycastResult>();
        foreach (RaycastResult result in results)
        {
            if ((UiLayer.value & ( 1 << result.gameObject.layer)) != 0)
            { 
                sortedResult.Add(result);
            }
        }

        return sortedResult;
    }
    public void CameraLimitation()
    {
        cameraTrackObj.transform.position = new Vector3(Mathf.Clamp(cameraTrackObj.transform.position.x, xLeft, xRight), cameraTrackObj.transform.position.y, Mathf.Clamp(cameraTrackObj.transform.position.z, zDown, zUp));
    }
    private void CameraMovement()
    {
        if (cameraFollow)
        {
            cameraTrackObj.transform.position = cameraFollowObj.transform.position;
        }
        else
        {
            Ray ray = new Ray(new Vector3(cameraTrackObj.transform.position.x, cameraTrackObj.transform.position.y + 100f, cameraTrackObj.transform.position.z), -transform.up);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, GroundLayer))
            {
                cameraTrackObj.transform.position = new Vector3(hit.point[0], hit.point[1] + 0.7f, hit.point[2]);
                //Debug.Log(hit.collider.name);
            }

        }
;
        //близость к экрану
        if (Input.mousePosition.x <= Screen.width * 0.03f)
        {
            cameraFollow = false;
            cameraTrackObj.transform.position += new Vector3(-ScreenSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.mousePosition.x >= Screen.width * 0.97f)
        {
            cameraFollow = false;
            cameraTrackObj.transform.position += new Vector3(ScreenSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.mousePosition.y <= Screen.height * 0.03f)
        {
            cameraFollow = false;
            cameraTrackObj.transform.position += new Vector3(0, 0, -ScreenSpeed * Time.deltaTime);
        }
        if (Input.mousePosition.y >= Screen.height * 0.97f)
        {
            cameraFollow = false;
            cameraTrackObj.transform.position += new Vector3(0, 0, ScreenSpeed * Time.deltaTime);
        }

        //колесо мыши
        if (Input.GetMouseButtonDown(2))
        {
            OldMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            MousePos = Input.mousePosition;
            Vector3 delt = MousePos - OldMousePos;
            Vector3 CameraMovement = new Vector3(delt.x / 1.75f, 0, delt.y / 1.75f) * Time.deltaTime * 3;
            cameraTrackObj.transform.position -= CameraMovement;
            OldMousePos = Input.mousePosition;
        }
        float Scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Scroll != 0)
        {
            CurrentCameraFOV -= Scroll * FOVSpeed * 0.01f;
            CurrentCameraFOV = Mathf.Clamp(CurrentCameraFOV, MinCameraFOV, MaxCameraFOV);
            _Camera.Lens.FieldOfView = CurrentCameraFOV;
        }
        CameraLimitation();
    }
    public void ClickControl()
    {
        //клик ЛКМ
        if (Input.GetMouseButtonDown(0))
        {

            //Talk/Fight
            Ray TOF = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit TOFhit;
            bool isTOF = Physics.Raycast(TOF, out TOFhit, Mathf.Infinity, TalkOrFightLayer);

            //Loot
            Ray Loot = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit Loothit;
            bool didHit = Physics.Raycast(Loot, out Loothit, Mathf.Infinity, LootLayer);

            //Move
            Ray GroundRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit Groundhit;
            bool isGround = Physics.Raycast(GroundRay, out Groundhit, Mathf.Infinity, GroundLayer);

            //Если готовится заклинание


            //проверка на нажатие ui
            List<RaycastResult> UiRay = IsPointerOverUIElement();
            if (UiRay.Count > 0)
            {
                if (UiRay[0].gameObject.TryGetComponent<Cell>(out Cell CellControl))
                {
                    Debug.Log("Cell");
                    CellControl.OnClick();
                }
                else if (UiRay[0].gameObject.GetComponentInParent<Stats>() != null)
                {
                    Stats StatsControl = UiRay[0].gameObject.GetComponentInParent<Stats>();
                    StatsControl.OnClick();
                    Debug.Log("Stats");
                }
            }
            //Клик на существо
            else if (TOFhit.collider != null && isTOF)
            {
                Debug.Log(TOFhit.collider.tag);
                if (TOFhit.collider.CompareTag("PlayerHitBox"))
                {
                    if (PrepearingCastSpell)
                    {
                        PlayerControl pl = TOFhit.collider.GetComponentInParent<PlayerControl>();
                        CurrentPlayerContr.TryCast(spell, Vector3.zero, pl, null, SpellTransforms[spell.SpellTransformIndex]);
                        CurrentPlayerContr.spellPos = pl.transform.position;
                        PrepearingCastSpell = false;

                    }
                    else
                    {
                        //set current player
                        CurrentPlayerContr = TOFhit.collider.GetComponentInParent<PlayerControl>();
                        CurrentPlayerRb = TOFhit.collider.GetComponentInParent<Rigidbody>();
                        CurrentPlayer = TOFhit.collider.transform.parent.gameObject;

                        uiControl.CurrentPlayer = CurrentPlayer;
                        uiControl.CurrentPlayerController = CurrentPlayerContr;
                        uiControl.SetSkillPanel();
                        //Add to selected Team
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
                        {
                            AddToCurrentTeam(CurrentPlayerContr);
                        }
                        else
                        {
                            SetOnePersonTeam(CurrentPlayerContr);
                        }
                        //focus camera on player
                        if (FocusCameraTimer + 0.2f > Time.time)
                        {
                            StartCoroutine(FollowPlayer());
                        }
                    }
                }
                else if (TOFhit.collider.CompareTag("CreatureHitBox"))
                {

                    Creature creature = TOFhit.collider.gameObject.GetComponentInParent<Creature>();
                    if (creature.isEnemy)
                    {
                        //атака/испрользование направленного скила
                        if (PrepearingCastSpell)
                        {
                            CurrentPlayerContr.TryCast(spell, Vector3.zero, null, creature, SpellTransforms[spell.SpellTransformIndex]);
                            CurrentPlayerContr.spellPos = creature.transform.position;
                            PrepearingCastSpell = false;
                        }
                        else
                        {
                            foreach (PlayerControl control in selectedTeam)
                            {
                                control.TalkTargetControl = null;
                                //control.TargetPositions = new Vector3[0];
                                control.agent.ResetPath();
                                control.EnemyTarget = creature;
                                control.LootTarget = null;
                                control._TargetPointCircle.SetActive(false);
                            }
                        }
                    }
                    else if (creature.isFriend)
                    {
                        if (!MainHero.InFight && creature.canTalk && creature.dialogControl != null)
                        {
                            MainHero.TalkTargetControl = creature;
                            //MainHero.TargetPositions = new Vector3[0];
                            MainHero.agent.ResetPath();
                            MainHero.EnemyTarget = null;
                            MainHero.LootTarget = null;
                            MainHero._TargetPointCircle.SetActive(false);

                        }
                    }

                }

                FocusCameraTimer = Time.time;
            }
            //If lootable object
            else if (Loothit.collider != null && didHit)
            {
                if (!CurrentPlayerContr.InFight)
                {
                    Lootable loot = Loothit.collider.gameObject.GetComponent<Lootable>();
                    CurrentPlayerContr._DistanceToLoot = loot._DistanceToLoot;
                    CurrentPlayerContr.TalkTargetControl = null;
                    //CurrentPlayerContr.TargetPositions.Clear();
                    CurrentPlayerContr.agent.ResetPath();
                    CurrentPlayerContr.EnemyTarget = null;
                    CurrentPlayerContr.LootTarget = loot;
                    CurrentPlayerContr._TargetPointCircle.SetActive(false);
                }


            }
            //Если готовится заклинание
            else if (PrepearingCastSpell && Groundhit.collider != null && isGround && !spell.CastDirected)
            {
                //CurrentPlayerContr.TargetPositions = new Vector3[0];
                CurrentPlayerContr.agent.ResetPath();
                CurrentPlayerContr.EnemyTarget = null;
                CurrentPlayerContr.ViewPointAfterMove = null;
                CurrentPlayerContr.spellPos = Groundhit.point;
                if (CurrentPlayerContr)
                    CurrentPlayerContr.TryCast(spell, Groundhit.point, null, null, SpellTransforms[spell.SpellTransformIndex]);
                PrepearingCastSpell = false;
                if (DragSpellObj)
                { DragSpellObj.SetActive(false); }
                DragSpellObj = null;
            }
            //клик на землю
            else if (Groundhit.collider != null && isGround)
            {
                if (CurrentPlayerContr != null)
                {
                    if (CurrentPlayerContr.CastingSpellCour != null)
                    {
                        CurrentPlayerContr.animator.SetBool("SpellPrepare", false);
                        StopCoroutine(CurrentPlayerContr.CastingSpellCour);
                        CurrentPlayerContr.CastingSpellCour = null;
                        CurrentPlayerContr.animator.SetFloat("SpellCast", 0);
                    }
                }
                if (Groundhit.collider.CompareTag("CantMoveHere"))
                {
                    StartCoroutine(ChangeCoursor(CantClickCursour, 0.3f));
                }
                else
                {
                    foreach (PlayerControl control in selectedTeam)
                    {
                        control._TargetPointCircle.SetActive(true);
                        control.agent.updateRotation = true;
                        //Если персонаж готовит заклинание/бЪёт то отключаем корутину
                        if (control.AttackingCour != null)
                        {
                            StopCoroutine(control.AttackingCour);
                        }
                        if (CurrentPlayerContr.CastingSpellCour != null)
                        {
                            StopCoroutine(control.CastingSpellCour);
                            control.CastingSpellCour = null;
                            control.animator.SetFloat("SpellCast", 0);
                        }

                    }
                    StartCoroutine(MovePlayerToClickPoint(Groundhit.point, Groundhit.collider.tag));


                }
            }


        }

        //клик ПКМ
        if (Input.GetMouseButtonDown(1))
        {
            //Talk/Fight
            Ray TOF = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit TOFhit;
            bool isTOF = Physics.Raycast(TOF, out TOFhit, Mathf.Infinity, TalkOrFightLayer);

            //Loot
            Ray Loot = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit Loothit;
            bool didHit = Physics.Raycast(Loot, out Loothit, Mathf.Infinity, LootLayer);

            //Move
            Ray GroundRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit Groundhit;
            bool isGround = Physics.Raycast(GroundRay, out Groundhit, Mathf.Infinity, GroundLayer);


            if (PrepearingCastSpell)
            {
                PrepearingCastSpell = false;
                CurrentPlayerContr.animator.SetBool("SpellPrepare", false);
                CurrentPlayerContr.animator.SetFloat("SpellCast", 0);
                DragSpellObj.SetActive(false);
                DragSpellObj = null;

            }
            //проверка на нажатие ui
            List<RaycastResult> UiRay = IsPointerOverUIElement();
            if (UiRay.Count > 0)
            {
                if (UiRay[0].gameObject.TryGetComponent<Cell>(out Cell CellControl))
                {
                    Debug.Log("Cell");
                    CellControl.OnClick();
                }
                else if (UiRay[0].gameObject.GetComponentInParent<Stats>() != null)
                {
                    Stats StatsControl = UiRay[0].gameObject.GetComponentInParent<Stats>();
                    StatsControl.OnClick();
                    Debug.Log("Stats");

                }


            }

            //Клик на существо
            else if (TOFhit.collider != null && isTOF)
            {   //focus camera on player
                if (TOFhit.collider.CompareTag("Player") || TOFhit.collider.CompareTag("Enemy"))

                    if (FocusCameraTimer + 0.2f > Time.time && TOFhit.collider.CompareTag("Player"))
                    {
                        StartCoroutine(FollowPlayer());
                    }

                    else if (TOFhit.collider.CompareTag("Enemy"))
                    {
                        foreach (PlayerControl control in selectedTeam)
                        {
                            control.EnemyTarget = TOFhit.collider.gameObject.GetComponent<Creature>();
                        }
                    }



                FocusCameraTimer = Time.time;
            }

            else if (Groundhit.collider != null && isGround)
            {
                //Если персонаж готовит заклинание/бЪёт то отключаем корутину
                if (Groundhit.collider.CompareTag("CantMoveHere"))
                {
                    StartCoroutine(ChangeCoursor(CantClickCursour, 0.3f));
                }
                else
                {

                    foreach (PlayerControl control in selectedTeam)
                    {
                        control._TargetPointCircle.SetActive(true);
                        control.agent.updateRotation = true;
                        //Если персонаж готовит заклинание/бЪёт то отключаем корутину
                        if (control.AttackingCour != null)
                        {
                            StopCoroutine(control.AttackingCour);
                        }
                        if (CurrentPlayerContr.CastingSpellCour != null)
                        {
                            StopCoroutine(control.CastingSpellCour);
                            control.CastingSpellCour = null;
                            control.animator.SetFloat("SpellCast", 0);
                        }

                    }

                    CurrentPlayerContr.EnemyTarget = null;

                    MovePlayerToClickPoint(Groundhit.point, Groundhit.collider.tag);
                    Debug.Log($"Point: {Groundhit.point}, Tag: {Groundhit.collider.tag}");
                }
            }


        }

    }

    //Team Control
    public void SetMainTeam(List<PlayerControl> playerControls)
    {
        if (playerControls != null && playerControls.Count < 6)
        {
            mainTeam = playerControls;
            uiControl.SetTeamStats(mainTeam);
        }
    }


    public void StartFight()
    {
        foreach (PlayerControl playerControl in mainTeam)
        {
            playerControl.InFight = true;
            playerControl.hpBarController.gameObject.SetActive(true);
            playerControl.animator.SetBool("InFight", true);
            playerControl.FightStarted();
        }
    }
    public void EndFight()
    {
        foreach (PlayerControl playerControl in mainTeam)
        {
            playerControl.InFight = false;
            playerControl.hpBarController.gameObject.SetActive(false);
            playerControl.animator.SetBool("InFight", false);
            playerControl.FightEnded();
        }
    }
    public void AddToMainTeam(PlayerControl control)
    {
        if (!mainTeam.Contains(control))
        { mainTeam.Add(control); }
        UpdateMainTeamStats();

    }
    public void AddToCurrentTeam(PlayerControl control)
    {
        if (!selectedTeam.Contains(control))
        { selectedTeam.Add(control);
            control.isSelected = true;
            control.SelectCircleLight.SetActive(true);
        }
        UpdateStatsRGB();
    }
    public void RemoveFromCurrentTeam(PlayerControl control)
    {
        if (selectedTeam.Contains(control))
        { selectedTeam.Remove(control); }
        control.isSelected = false;
        control.SelectCircleLight.SetActive(false);
        UpdateStatsRGB();
    }

    public void SetOnePersonTeam(PlayerControl control)
    {
        foreach (PlayerControl cntr in selectedTeam)
        {cntr.isSelected = false;
        cntr.SelectCircleLight.SetActive(false);
        }
        selectedTeam = new List<PlayerControl>() {control};
        control.isSelected = true;
        control.SelectCircleLight.SetActive(true);
        UpdateStatsRGB();
    }
    public void RemoveFromMainTeam(PlayerControl control)
    {
        if (mainTeam.Contains(control))
        { mainTeam.Remove(control);}
        UpdateMainTeamStats();
    }
    private void UpdateMainTeamStats()
    {
        for (int i = 0; i < uiControl.stats.Count; i++)
        {
            if (i < mainTeam.Count)
            {
                uiControl.stats[i].SetPanel(mainTeam[i]);
            }
            else
            {
                uiControl.stats[i].gameObject.SetActive(false);
            }
        }
    }
    private void UpdateStatsRGB()
    {
        foreach (Stats stat in uiControl.stats)
        {
            if (selectedTeam.Contains(stat.playerControl))
            {
                stat.ActivateRgb();
            }
            else
            {
                stat.DisableRgb();
            }
        }
    }


    //SpellCast
    private void VisualiseSpellnderCursor()
    {
        if (PrepearingCastSpell)
        {
            if (DragSpellObj != null)
            {
                Ray gr = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                bool isHit = Physics.Raycast(gr, out hit, Mathf.Infinity, GroundLayer);

                if (isHit == true && hit.collider != null)
                {
                    if (spell.CastAtPoint)
                    {
                        DragSpellObj.transform.position = hit.point;
                    }
                    else
                    {
                        DragSpellObj.transform.position = CurrentPlayer.transform.position;
                        DragSpellObj.transform.LookAt(hit.point);
                        DragSpellObj.transform.rotation = Quaternion.Euler(0, 180 + DragSpellObj.transform.eulerAngles.y, 0);
                    }
                }
            }
        }
    }
    public void PrepareSpell(Spell spell)
    {
        this.spell  = spell;
        if (spell.SpellPrepareObj != null)
        {
            GameObject obj = Instantiate(spell.SpellPrepareObj);
            obj.GetComponent<SpellPrepare>().mask = spell.mask;
            DragSpellObj = obj;
            DragSpellObj.SetActive(true);
        }
        PrepearingCastSpell = true;
    }
    
    //public IEnumerator CastSpell(GameObject SpellObj, float KastTimer, bool CastAtPoint, Vector3 CursorPosition)
    //{
    //    PrepearingCastSpell = false;

    //    yield return KastTimer;
    //    DragSpellObj.SetActive(false);
    //    if (CastAtPoint)
    //    {
    //        GameObject skill = Instantiate(SpellObj, CursorPosition, Quaternion.identity);
    //    }
    //    else
    //    {
    //        Vector3 direction = ((CursorPosition - CurrentPlayer.transform.position).normalized);
    //        Quaternion Rotation = Quaternion.LookRotation(direction);
    //        Rotation.x = 0;
    //        Rotation.z = 0;
    //        GameObject skill = Instantiate(SpellObj, CurrentPlayer.transform.position, Rotation);
    //    }

    //}
    
    public void DragItems()
    {
        if (IsDrag)
        {
            CursorDragObj.transform.position = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                List<RaycastResult> results = IsPointerOverUIElement();
                if (results.Count > 0f)
                {
                    if (results[0].gameObject.TryGetComponent<Cell>(out Cell cell))
                    {
                        if (cell != this.cell)
                        {
                            var arm = this.cell.CurrentItem as Armor;
                            if (cell.helmet == true || cell.chest == true || cell.boots == true || cell.Weapon == true || cell.bracers || cell.SecondWeapon || cell.pants || cell.cape)
                            {
                                if (arm != null && ((arm.helmet == true && cell.helmet == true) || (arm.chest == true && cell.chest == true) || (arm.boots == true && cell.boots == true) || (arm.weapon == true && cell.Weapon == true) || (arm.bracers == true && cell.bracers == true) || (arm.cape == true && cell.cape == true) || (arm.SecondWeapon == true && cell.SecondWeapon == true) || (arm.pants == true && cell.pants == true)))
                                {
                                    if (!cell.isBlocked && !this.cell.isBlocked)
                                    {
                                        if (!(arm.twoHand && uiControl.ArmorCells[7].CurrentItem != null))
                                        {
                                            Item targetItem = cell.CurrentItem;
                                            if (cell.CurrentItem != null && targetItem != null && cell.CurrentItem == targetItem && targetItem.multiplyable && cell.CurrentItem.multiplyable)
                                            {
                                                cell.count += this.cell.count;
                                                this.cell.CurrentItem = null;
                                            }
                                            else
                                            {
                                                int temp = cell.count;
                                                cell.count = this.cell.count;
                                                this.cell.count = temp;
                                                cell.CurrentItem = this.cell.CurrentItem;
                                                this.cell.CurrentItem = targetItem;
                                            }
                                            if (arm.twoHand)
                                            {
                                                cell.AddSprite = arm.Icon;
                                                uiControl.ArmorCells[7].isBlocked = true;
                                                uiControl.ArmorCells[7].image.sprite = arm.Icon;
                                                uiControl.ArmorCells[7].currentImage.color = cellBlur;
                                            }
                                            cell.FillSlot();
                                            this.cell.FillSlot();
                                        }
                                    }
                                }

                            }
                            else
                            {
                                if (!cell.isBlocked && !this.cell.isBlocked)
                                {

                                    Item targetItem = cell.CurrentItem;
                                    if (cell.CurrentItem != null && targetItem != null && cell.CurrentItem == targetItem && targetItem.multiplyable && cell.CurrentItem.multiplyable)
                                    {
                                        cell.count += this.cell.count;
                                        this.cell.CurrentItem = null;
                                    }
                                    else
                                    {
                                        int temp = cell.count;
                                        cell.count = this.cell.count;
                                        this.cell.count = temp;
                                        cell.CurrentItem = this.cell.CurrentItem;
                                        this.cell.CurrentItem = targetItem;
                                    }
                                    if (this.cell.wearCell && arm.twoHand)
                                    {
                                        this.cell.AddSprite = null;
                                        uiControl.ArmorCells[7].isBlocked = false;
                                        uiControl.ArmorCells[7].image.sprite = null;
                                        uiControl.ArmorCells[7].currentImage.color = new Color(1, 1, 1, 0);
                                    }
                                    cell.FillSlot();
                                    this.cell.FillSlot();
                                }
                            }
                            if (cell.wearCell || this.cell.wearCell)
                            {
                                uiControl.updateWearStats();
                            }
                            
                        }
                    }
                }
                else
                {

                }
                IsDrag = false;
            }
            if (Input.GetMouseButtonUp(1))
            {
                List<RaycastResult> results = IsPointerOverUIElement();
                if (results.Count > 0f)
                {
                    if (results[0].gameObject.TryGetComponent<Cell>(out Cell cell))
                    {
                        if (cell != this.cell)
                        {
                            if (cell.helmet == true || cell.chest == true || cell.boots == true || cell.pants == true || cell.SecondWeapon == true || cell.Weapon == true || cell.bracers == true || cell.cape)
                            {
                                var arm = this.cell.CurrentItem as Armor;
                                if (arm != null && ((arm.helmet == true && cell.helmet == true) || (arm.chest == true && cell.chest == true) || (arm.boots == true && cell.boots == true) || (arm.weapon == true && cell.Weapon == true) || (arm.bracers == true && cell.bracers == true) || (arm.cape == true && cell.cape == true)))
                                {
                                    Item targetItem = cell.CurrentItem;
                                    if ((cell.CurrentItem == null && targetItem.multiplyable) || (cell.CurrentItem != null && targetItem != null && cell.CurrentItem == targetItem && targetItem.multiplyable && cell.CurrentItem.multiplyable))
                                    {
                                        cell.count += Convert.ToInt32(Mathf.Ceil(this.cell.count/2));
                                        this.cell.count -= Convert.ToInt32(Mathf.Ceil(this.cell.count / 2));
                                        cell.CurrentItem = this.cell.CurrentItem;
                                        if (this.cell.count == 0) { this.cell.CurrentItem = null; }
                                    }
                                    else
                                    {
                                        int temp = cell.count;
                                        cell.count = this.cell.count;
                                        this.cell.count = temp;
                                        cell.CurrentItem = this.cell.CurrentItem;
                                        this.cell.CurrentItem = targetItem;
                                    }
                                    cell.FillSlot();
                                    this.cell.FillSlot();
                                }
                            }
                            else
                            {
                                Item targetItem = cell.CurrentItem;
                                if (cell.CurrentItem != null && targetItem != null && cell.CurrentItem == targetItem && targetItem.multiplyable && cell.CurrentItem.multiplyable)
                                {
                                    cell.count += this.cell.count;
                                    this.cell.CurrentItem = null;
                                }
                                else
                                {
                                    int temp = cell.count;
                                    cell.count = this.cell.count;
                                    this.cell.count = temp;
                                    cell.CurrentItem = this.cell.CurrentItem;
                                    this.cell.CurrentItem = targetItem;
                                }
                                cell.FillSlot();
                                this.cell.FillSlot();
                            }
                            if (cell.wearCell || this.cell.wearCell)
                            {
                                uiControl.updateWearStats();
                            }
                        }
                    }
                }
                else
                {

                }
                IsDrag = false;
            }
        }
    }
    public IEnumerator DragItem(Item item, Cell cell)
    {
        CursorDragPicture.color = Color.white;
        CursorDragPicture.sprite = item.Icon;
        uiControl.UpdateDiscription(cell.CurrentItem);
        this.cell = cell;
        IsDrag = true;
        yield return new WaitWhile(() => IsDrag);

        this.cell = null;
        CursorDragPicture.color = new Color(0, 0, 0, 0);
        CursorDragPicture.sprite = null;
    }

    //Иконки
    public void DragStats()
    {
        if (IsDragStats)
        {
            CursorDragObj.transform.position = Input.mousePosition;
            Vector3 MousePosit = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                Stats[] list = uiControl.sortedList;

                if (list.Length == 2)
                {
                    if (MousePosit.x < list[0].gameObject.transform.position.x)
                    {
                        MovingStats.transform.SetSiblingIndex(0);
                        Debug.Log("0");
                    }
                    else if (MousePosit.x < list[1].gameObject.transform.position.x - 10 && MousePosit.x > list[0].gameObject.transform.position.x - 10)
                    {
                        MovingStats.transform.SetSiblingIndex(1);
                        Debug.Log("1");
                    }
                    else if (MousePosit.x > list[1].gameObject.transform.position.x - 10)
                    {
                        MovingStats.transform.SetSiblingIndex(2);
                        Debug.Log("2");
                    }

                }
                else if (list.Length == 3)
                {
                    if (MousePosit.x < list[0].gameObject.transform.position.x)
                    {
                        MovingStats.transform.SetSiblingIndex(0);
                        Debug.Log("0");
                    }
                    else if (MousePosit.x < list[1].gameObject.transform.position.x && MousePosit.x > list[0].gameObject.transform.position.x)
                    {
                        MovingStats.transform.SetSiblingIndex(1);
                        Debug.Log("1");
                    }
                    else if (MousePosit.x < list[2].gameObject.transform.position.x && MousePosit.x > list[1].gameObject.transform.position.x)
                    {
                        MovingStats.transform.SetSiblingIndex(2);
                        Debug.Log("2");
                    }
                    else if (MousePosit.x > list[2].gameObject.transform.position.x)
                    {
                        MovingStats.transform.SetSiblingIndex(3);
                        Debug.Log("3");
                    }

                }
                else if (list.Length == 4)
                {
                    if (MousePosit.x < list[0].gameObject.transform.position.x)
                    {
                        MovingStats.transform.SetSiblingIndex(0);
                        Debug.Log("0");
                    }
                    else if (MousePosit.x < list[1].gameObject.transform.position.x - 10 && MousePosit.x > list[0].gameObject.transform.position.x - 10)
                    {
                        MovingStats.transform.SetSiblingIndex(1);
                        Debug.Log("1");
                    }
                    else if (MousePosit.x < list[2].gameObject.transform.position.x - 10 && MousePosit.x > list[1].gameObject.transform.position.x - 10)
                    {
                        MovingStats.transform.SetSiblingIndex(2);
                        Debug.Log("2");
                    }
                    else if (MousePosit.x < list[3].gameObject.transform.position.x - 10 && MousePosit.x > list[2].gameObject.transform.position.x - 10)
                    {
                        MovingStats.transform.SetSiblingIndex(3);
                        Debug.Log("3");
                    }
                    else if (MousePosit.x > list[3].gameObject.transform.position.x - 10)
                    {
                        MovingStats.transform.SetSiblingIndex(4);
                        Debug.Log("4");
                    }
                }


                uiControl.UpdateList();
                IsDragStats = false;
            }
        }
    }
    public IEnumerator DragStat(UnityEngine.Sprite sprite, PlayerControl playerControl, GameObject player, Stats stat)
    {
        CurrentPlayerContr = playerControl;
        MovingStats = stat;
        uiControl.CurrentPlayer = player;
        uiControl.CurrentPlayerController = playerControl;
        uiControl.SetSkillPanel();

        CursorDragPicture.color = Color.white;
        CursorDragPicture.sprite = sprite;

        IsDragStats = true;
        yield return new WaitWhile(() => IsDragStats);

        CursorDragPicture.color = new Color(0, 0, 0, 0);
        CursorDragPicture.sprite = null;
    }    

    //Double click on player

    private IEnumerator FollowPlayer()
    {
        yield return null;
        cameraFollowObj = CurrentPlayerContr;
        cameraFollow = true;
    }

    //Change Cursor
    public IEnumerator ChangeCoursor(Texture2D texture, float timer)
    {
        Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
        yield return new WaitForSeconds(timer);
        Cursor.SetCursor(DefaultCursour, Vector2.zero, CursorMode.Auto);
    }
    public IEnumerator VisualizePlayersWayPoints(Vector3 pos, List<GameObject> spis)
    {
        while (VisualizePoint)
        {
            Ray GroundRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit Groundhit;
            bool isGround = Physics.Raycast(GroundRay, out Groundhit, Mathf.Infinity, GroundLayer);
           

            if (isGround)
            {
                Vector3 dist = (Groundhit.point - pos).normalized;
                int rowCount = 0;
                int count = 0;
                int countInRow = 0;
                int maxInRow = 1;
                Vector3 rotatedDist = Quaternion.Euler(0, 90, 0) * dist;
                foreach (GameObject obj in spis)
                {//distLayers distBetw
                    if (!obj.activeInHierarchy)
                    { obj.SetActive(true);}

                    count += 1;
                    countInRow += 1;

                    Vector3 vect = pos - dist * distLayers * rowCount;
                    if (count > 1)
                    {
                        vect += rotatedDist * distLayers *  0.5f;
                        vect -= rotatedDist * distLayers * (countInRow-1);
                    }

                    obj.transform.position = vect;

                    if (countInRow == maxInRow)
                    {
                        if (maxInRow == 1)
                        { 
                            maxInRow += 1;
                        }
                        rowCount += 1;
                        countInRow = 0;
                    }
                }
            }
            yield return null;
        }
    }
    public IEnumerator MovePlayerToClickPoint(Vector3 pos, string Tag)
    {
        List<GameObject> spis = new List<GameObject>();
        foreach (PlayerControl control in selectedTeam)
        {
            spis.Add(control._TargetPointCircle);
        }
        VisualizePoint = true;
        Coroutine cour = StartCoroutine(VisualizePlayersWayPoints(pos, spis));
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
        VisualizePoint = false;
        StopCoroutine(cour);
        Ray GroundRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit Groundhit;
        bool isGround = Physics.Raycast(GroundRay, out Groundhit, Mathf.Infinity, GroundLayer);
        int i = 0;
        foreach (PlayerControl control in selectedTeam)
        {
            control.TalkTargetControl = null;
            control.EnemyTarget = null;
            control.LootTarget = null;
            control.currentActivityImage.sprite = null;

            control.SetTargetPosition(spis[i].transform.position, false, Groundhit.point);
            StartCoroutine(ChangeCoursor(ClickedCursour, 0.2f));
            i += 1;
        }
            
    }

    //Set choosed person (ЛКМ) 
    private void SetChoosedPerson()
    {

    }
}