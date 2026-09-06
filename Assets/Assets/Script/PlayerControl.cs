using Assets.Script;
using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.Multiplayer.PlayMode;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;
using UnityEngine.Splines.ExtrusionShapes;
using UnityEngine.UI;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private GameObject HitBox;
    public HpBarControlelr hpBarController;
    public Rigidbody rb;
    public CursorController cursorController;
    public UiController uiController;
    public Animator animator; //float: speed Attack CastSpell Bool: Sword Bow Magic Death InFight

    [Header("HP")]
    public float _Hp;
    public float _MaxHp;

    [Header("Stamina")]
    [SerializeField] private float handStamina;
    [SerializeField] private float maxStamina;
    [SerializeField] private float currentStamina;

    [Header("BuffStats")]
    public int buffDefence;
    public int buffStrength; //damage and block
    public int buffDexterity; // Damage and dodge
    public int buffConstitution; // Hp
    public int buffIntelligence; // Mage Damage

    [Header("Stats")]
    public int defence;

    public int Strength; //damage and hp
    public int Dexterity; // Damage and attackspeed
    public int Constitution; // Hp
    public int Intelligence; // Mage Damage

    public int level;
    [SerializeField] private TextMeshPro levelIconText;
    public int currentExp;
    public float maxExp;
    public int levelPoints;

    [Header("Movement")]
    public float _Speed;

    private float dop = 0f;
    //Ways to move/do
    //public List<Vector3> TargetPositions;
    public NavMeshAgent agent;

    public Creature? EnemyTarget;
    public Lootable? LootTarget;
    public Creature? TalkTargetControl;

    [SerializeField] private float _DistanceToTalk;
    [SerializeField] private float _DistanceToStop;
    public float _DistanceToLoot;
    private Vector3 PlayerDirection;
    public Vector3? ViewPointAfterMove;

    private Quaternion rot;
    private Vector3 isRot;

    public GameObject _TargetPointCircle;

    public LayerMask GroundLayer;
    public float groundOffset;
    public float snapSpeed;

    public GameObject SelectCircle;
    public GameObject SelectCircleLight;
    [SerializeField]  private GameObject DirectionCircle;
    public bool isSelected;

    public UnityEngine.UI.Image currentActivityImage;
    [SerializeField] private Sprite baseActivityImage;
    [SerializeField] private Sprite attackSprite;
    

    public GameObject bodyBag;
    public Renderer bodyBagRender;

    [Header("Attack")]
    private int Damage;
    private bool IsAttacking = false;
    private bool attack;
    public float AttackDistance;
    [SerializeField] private float handAttackDistance;
    [SerializeField] private float HandAttackDurationBefore;
    [SerializeField] private float HandAttackDurationAfter;
    [SerializeField] private int handDamage;
    [SerializeField] private bool dealDamage = false;

    public bool InFight;

    public Sprite Icon;

    public Coroutine AttackingCour;

    [Header("ARMOR")]
    public Armor helmet;
    public Armor chest;
    public Armor pants;
    public Armor boots;
    public Armor bracers;
    public Armor cape;


    public Armor Weapon;
    public Armor SecondWeapon;

    [Header("EquipedArmor 1)helm 2)chest 3)pants 4)boots 5)bracers 6)cape")]
    //WEAPON
    [SerializeField] private GameObject[] lastWeapons;
    private int lastShield = -1;

    [SerializeField] private GameObject[] activeSwords;
    [SerializeField] private GameObject[] activeDaggers;
    [SerializeField] private GameObject[] activeGreatSwords;
    [SerializeField] private GameObject[] activeBows;
    [SerializeField] private GameObject[] activeStaffs; 
    [SerializeField] private GameObject[] activeSecondWeapons;


    [SerializeField] private GameObject[] backSwords;
    [SerializeField] private GameObject[] backDaggers;
    [SerializeField] private GameObject[] backGreatSwords;
    [SerializeField] private GameObject[] backBows;
    [SerializeField] private GameObject[] backStaffs;
    [SerializeField] private GameObject[] backSecondWeapons;

    //armor
    [SerializeField] public GameObject basicChest;
    [SerializeField] public GameObject basicPants;

    [Serializable]
    public class AllArmor
    { public GameObject[] armors; }
    public AllArmor[] allArmor;
    private int[][] lastArmor = new int[6][];


    [Header("Spells")]
    public Creature spellTarget;
    public ProBuilderMesh SpellRadiusCircle;
    public Coroutine CastingSpellCour;

    public List<int> SkillActivated;

    public Vector3 spellPos; //spellPoint

    public Spell[] Spell1;
    public Spell[] Spell2;
    public Spell[] Spell3;
    public Spell[] Spell4;
    public Spell[] Spell5;

    public Spell[][] AllSpells;
    
    public bool isMove = false;

    public float speed;

    public float maxTimer = 0.3f;
    private float timer = 0;

    private void Awake()
    {
        AllSpells = new Spell[5][] { Spell1, Spell2, Spell3, Spell4, Spell5 };
    }

    void Start()
    {
        levelIconText.text = level.ToString();
        updateArmorStats();
    }

    public void UpdateAllSpells()
    {
        AllSpells = new Spell[5][] { Spell1, Spell2, Spell3, Spell4, Spell5 };
    }
    private void FixedUpdate()
    {
        //rb.AddForce(new Vector3(0, _GravityModifier, 0), ForceMode.Force);
        if (!isMove && EnemyTarget == null && agent.hasPath == false && agent.pathPending == false)
        {
            //StopMovement();
            //Debug.Log("2s");
        }
        //if (agent.hasPath || agent.pathPending)
        //{
        //    Debug.Log("” ‡„ÂÌÚ‡ ≈—“‹ ÔÛÚ¸");
        //}
        //else
        //{
        //    Debug.Log("” ‡„ÂÌÚ‡ Õ≈“ ÔÛÚË");
        //}
    }
    // Update is called once per frame
    void Update()
    {
        hpBarController.UpdateStamina(currentStamina, maxStamina);
        AnimationControl();

        attack = EnemyTarget != null;
        PlayerRotate();
        KdControl();
        GroundRaycast();
        if (EnemyTarget != null)
        {
            isMove = true;
            ViewPointAfterMove = null;
            if (AttackDistance < Vector3.Distance(gameObject.transform.position, EnemyTarget.transform.position)-dop)
            {
                timer += Time.deltaTime;
                if (timer > maxTimer)
                {
                    agent.SetDestination(EnemyTarget.transform.position);
                    timer = 0f;
                }
            }
            else if (AttackDistance > Vector3.Distance(gameObject.transform.position, EnemyTarget.transform.position)-dop)
            {
                dop = 0.2f;
                rb.angularVelocity = Vector3.zero;
                agent.ResetPath();
                if (currentStamina <= 0.01f && !IsAttacking)
                {
                    StartCoroutine(Attack(Damage));
                }
            }
        }
        else if (agent.hasPath)
        {
            isMove = true;

            Vector3 direction = (agent.destination - transform.position).normalized;
            direction.y = 0;
            if (Vector3.Distance(transform.position, new Vector3(agent.destination.x, transform.position.y, agent.destination.z)) < _DistanceToStop)
            {
                if (ViewPointAfterMove != null)
                {
                    Vector3 PlayerDirection = (ViewPointAfterMove.Value - transform.position);
                    PlayerDirection = new Vector3(PlayerDirection.x, 0, PlayerDirection.z).normalized;
                    StartCoroutine(LookAtPoint(PlayerDirection));
                }
                _TargetPointCircle.SetActive(false);
                agent.ResetPath();
                rb.linearVelocity = new Vector3(0, 0, 0);
            }
        }
        else if (TalkTargetControl != null)
        {
            isMove = true;

            timer += Time.deltaTime;
            if (timer > maxTimer)
            {
                agent.SetDestination(TalkTargetControl.transform.position);
                timer = 0f;
            }
            if (Vector3.Distance(transform.position, new Vector3(TalkTargetControl.transform.position.x, transform.position.y, TalkTargetControl.gameObject.transform.position.z)) < _DistanceToTalk)
            {
                uiController.OpenDialog(TalkTargetControl);
                Debug.Log("Talk");
                TalkTargetControl = null;
                agent.ResetPath();
                rb.linearVelocity = new Vector3(0, 0, 0);
            }
        }
        else if (LootTarget != null)
        {
            isMove = true;

            timer += Time.deltaTime;
            if (timer > maxTimer)
            {
                agent.SetDestination(LootTarget.transform.position);
                timer = 0f;
            }
            if (Vector3.Distance(transform.position, new Vector3(LootTarget.transform.position.x, transform.position.y, LootTarget.transform.position.z)) < _DistanceToLoot)
            {
                agent.ResetPath();
                rb.linearVelocity = Vector3.zero;
                uiController.OpenLootMenu(LootTarget);
                LootTarget = null;
            }
        }
        else
        {
            isMove = false;
        }
        if (isSelected)
        {
            DirectionCircleControl();
        }
    }
    private IEnumerator LookAtPoint(Vector3 pos)
    {
        yield return
        ViewPointAfterMove = null;
    }
    public void enemyDied()
    {
        EnemyTarget = null;
        rb.linearVelocity = Vector3.zero;
        agent.ResetPath();
    }


    public void FightStarted()
    {
        if (Weapon != null)
        {
            if (Weapon.Bow)
            { backBows[Weapon.indexes[0]].SetActive(false); activeBows[Weapon.indexes[0]].SetActive(true); }
            else if (Weapon.RangeStaff)
            { backStaffs[Weapon.indexes[0]].SetActive(false); activeStaffs[Weapon.indexes[0]].SetActive(true); }
            else if (Weapon.Sword)
            { backSwords[Weapon.indexes[0]].SetActive(false); activeSwords[Weapon.indexes[0]].SetActive(true); }
            else if (Weapon.GreatSword)
            { backGreatSwords[Weapon.indexes[0]].SetActive(false); activeGreatSwords[Weapon.indexes[0]].SetActive(true); }
            else if (Weapon.Dagger)
            { backDaggers[Weapon.indexes[0]].SetActive(false); activeDaggers[Weapon.indexes[0]].SetActive(true); }
        }
        if (SecondWeapon != null)
        {
            if (Weapon.Bow)
            { backSecondWeapons[Weapon.indexes[0]].SetActive(false); activeSecondWeapons[Weapon.indexes[0]].SetActive(true); }
        }
    }
    public void FightEnded()
    {
        if (Weapon != null)
        {
            if (Weapon.Bow)
            { backBows[Weapon.indexes[0]].SetActive(true); activeBows[Weapon.indexes[0]].SetActive(false); }
            else if (Weapon.RangeStaff)
            { backStaffs[Weapon.indexes[0]].SetActive(true); activeStaffs[Weapon.indexes[0]].SetActive(false); }
            else if (Weapon.Sword)
            { backSwords[Weapon.indexes[0]].SetActive(true); activeSwords[Weapon.indexes[0]].SetActive(false); }
            else if (Weapon.GreatSword)
            { backGreatSwords[Weapon.indexes[0]].SetActive(true); activeGreatSwords[Weapon.indexes[0]].SetActive(false); }
            else if (Weapon.Dagger)
            { backDaggers[Weapon.indexes[0]].SetActive(true); activeDaggers[Weapon.indexes[0]].SetActive(false); }
        }
        if (SecondWeapon != null)
        {
            if (Weapon.Bow)
            { backSecondWeapons[Weapon.indexes[0]].SetActive(true); activeSecondWeapons[Weapon.indexes[0]].SetActive(false); }
        }
    }

    public void updateArmorStats()
    {
        animator.SetBool("Bow", false);
        animator.SetBool("Sword", false);
        animator.SetBool("RangeStaff", false);
        animator.SetBool("GreatSword", false);
        animator.SetBool("Dagger", false);
        if (lastWeapons[0] != null) { lastWeapons[0].SetActive(false); lastWeapons[1].SetActive(false); }
        if (lastShield != -1) { activeSecondWeapons[lastShield].SetActive(false); backSecondWeapons[lastShield].SetActive(false); }


        if (Weapon != null)
        {
            AttackDistance = Weapon.AttackDistance;

            if (Weapon.Sword) { animator.SetBool("Sword", true); backSwords[Weapon.indexes[0]].SetActive(true); activeSwords[Weapon.indexes[0]].SetActive(false); lastWeapons[0] = activeSwords[Weapon.indexes[0]]; lastWeapons[1] = backSwords[Weapon.indexes[0]]; }
            else if (Weapon.Bow) { animator.SetBool("Bow", true); { backBows[Weapon.indexes[0]].SetActive(true); activeBows[Weapon.indexes[0]].SetActive(false); lastWeapons[0] = activeBows[Weapon.indexes[0]]; lastWeapons[1] = backBows[Weapon.indexes[0]]; } }
            else if (Weapon.RangeStaff) { animator.SetBool("RangeStaff", true); backStaffs[Weapon.indexes[0]].SetActive(true); activeStaffs[Weapon.indexes[0]].SetActive(false); lastWeapons[0] = activeStaffs[Weapon.indexes[0]]; lastWeapons[1] = backStaffs[Weapon.indexes[0]]; }
            else if (Weapon.Dagger) { animator.SetBool("Dagger", true); backDaggers[Weapon.indexes[0]].SetActive(true); activeDaggers[Weapon.indexes[0]].SetActive(false); lastWeapons[0] = activeDaggers[Weapon.indexes[0]]; lastWeapons[1] = backDaggers[Weapon.indexes[0]]; }
            else if (Weapon.GreatSword) { animator.SetBool("GreatSword", true); backGreatSwords[Weapon.indexes[0]].SetActive(true); activeGreatSwords[Weapon.indexes[0]].SetActive(false); lastWeapons[0] = activeGreatSwords[Weapon.indexes[0]]; lastWeapons[1] = backGreatSwords[Weapon.indexes[0]]; }
            if (SecondWeapon && SecondWeapon.indexes.Length > 0) { backSecondWeapons[SecondWeapon.indexes[0]].SetActive(true); activeSecondWeapons[SecondWeapon.indexes[0]].SetActive(false); lastShield = SecondWeapon.indexes[0]; }

            AttackDistance = Weapon.AttackDistance;
            Damage = Weapon.Damage;
        }
        else
        {
            animator.SetBool("Sword", true);
            AttackDistance = handAttackDistance;
            maxStamina = handStamina;
            Damage = handDamage; ;

        }
        Armor[] armor = new Armor[6] { helmet, chest, pants, boots, bracers, cape };
        bool fl1 = false;
        bool fl2 = false;
        for (int i = 0; i < armor.Length; i++)
        {
            if (armor[i] != null)
            {
                if (armor[i].disableChest) { fl1 = true; }
                if (armor[i].disablePants) { fl2 = true; }
                if (armor[i].indexes != lastArmor[i])
                {
                    if (lastArmor[i] != null)
                    {
                        foreach (int j in lastArmor[i])
                        {
                            allArmor[i].armors[j].SetActive(false);
                        }
                    }
                    foreach (int j in armor[i].indexes)
                    {
                        allArmor[i].armors[j].SetActive(true);
                    }
                    lastArmor[i] = armor[i].indexes;
                }
            }
            else
            {
                if (lastArmor[i] != null)
                {
                    if (lastArmor[i] != null)
                    {
                        foreach (int j in lastArmor[i])
                        {
                            allArmor[i].armors[j].SetActive(false);
                        }
                    }
                    lastArmor[i] = null;
                }
            }
        }
        if (fl1) { basicChest.SetActive(false); }
        else { basicChest.SetActive(true); }
        if (fl2) { basicPants.SetActive(false); }
        else { basicPants.SetActive(true); }

        if (helmet != null) { buffDefence += helmet.defence; buffStrength += helmet.Strength; buffDexterity += helmet.Dexterity; buffConstitution += helmet.Constitution; buffIntelligence += helmet.Intelligence; }
        if (chest != null) { buffDefence += chest.defence; buffStrength += chest.Strength; buffDexterity += chest.Dexterity; buffConstitution += chest.Constitution; buffIntelligence += chest.Intelligence; }
        if (pants != null) { buffDefence += pants.defence; buffStrength += pants.Strength; buffDexterity += pants.Dexterity; buffConstitution += pants.Constitution; buffIntelligence += pants.Intelligence; }
        if (boots != null) { buffDefence += boots.defence; buffStrength += boots.Strength; buffDexterity += boots.Dexterity; buffConstitution += boots.Constitution; buffIntelligence += boots.Intelligence;}
        if (bracers != null) { buffDefence += bracers.defence; buffStrength += bracers.Strength; buffDexterity += bracers.Dexterity; buffConstitution += bracers.Constitution; buffIntelligence += bracers.Intelligence; }
        if (cape != null) { buffDefence += cape.defence; buffStrength += cape.Strength; buffDexterity += cape.Dexterity; buffConstitution += cape.Constitution; buffIntelligence += cape.Intelligence; }
        if (Weapon != null) { buffDefence += Weapon.defence; buffStrength += Weapon.Strength; buffDexterity += Weapon.Dexterity; buffConstitution += Weapon.Constitution; buffIntelligence += Weapon.Intelligence; }
        if (SecondWeapon != null) { buffDefence += SecondWeapon.defence; buffStrength += SecondWeapon.Strength; buffDexterity += SecondWeapon.Dexterity; buffConstitution += SecondWeapon.Constitution; buffIntelligence += SecondWeapon.Intelligence; }

        //update stats (armor, max hp, speed)
        _MaxHp = 10 + buffConstitution * 2 + Strength;
        if (Weapon != null) { maxStamina = Weapon.stamina;}
        else { maxStamina = handStamina;}
        maxStamina -= Dexterity * 0.3f;
}
    public void getExp(int col)
    {
        currentExp += col;
        if (currentExp >= maxExp)
        {
            level += 1;
            levelIconText.text = level.ToString();
            levelPoints += 1;
            currentExp = Mathf.RoundToInt(maxExp - currentExp);
            maxExp = Mathf.Ceil(maxExp * cursorController.expMulti);

        }
    }
    private void StopMovement()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        agent.ResetPath();
    }
    private IEnumerator WaitSomeSec(float time)
    {
        yield return new WaitForSeconds(time);
        ViewPointAfterMove = null;
    }
    //Animation Control
    private void AnimationControl()
    {
        animator.SetFloat("speed", agent.velocity.magnitude);
        
    }

    //rotate control
    private void PlayerRotate()
    {
        if (EnemyTarget != null)
        {
            PlayerDirection = (EnemyTarget.transform.position - transform.position);
            PlayerDirection = new Vector3(PlayerDirection.x, 0, PlayerDirection.z).normalized;
            Quaternion look = Quaternion.LookRotation(PlayerDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 720f * Time.deltaTime);
        }
        else if (CastingSpellCour != null)
        {
            PlayerDirection = new Vector3(spellPos.x, transform.position.y, spellPos.z) - transform.position;
            Quaternion look = Quaternion.LookRotation(PlayerDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 720f * Time.deltaTime);
            Debug.Log("ROTATE");
        }
        else if (TalkTargetControl != null)
        {
            
            PlayerDirection = (TalkTargetControl.transform.position - transform.position);
            PlayerDirection = new Vector3(PlayerDirection.x, 0, PlayerDirection.z).normalized;
            Quaternion look = Quaternion.LookRotation(PlayerDirection);
            agent.ResetPath();
            ViewPointAfterMove = null;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 720f * Time.deltaTime);
        }
        //else if (agent.pathPending || agent.hasPath || ViewPointAfterMove != null)
        //{

        //    if (agent.hasPath || agent.pathPending)
        //    {
        //        PlayerDirection = (agent.destination - transform.position);
        //        PlayerDirection = new Vector3(PlayerDirection.x, 0, PlayerDirection.z).normalized;
        //        rot = PlayerDirection;
        //    }
        //    else if (ViewPointAfterMove != null)
        //    {
        //        PlayerDirection = (ViewPointAfterMove.Value - transform.position);
        //        PlayerDirection = new Vector3(PlayerDirection.x, 0, PlayerDirection.z).normalized;
        //        rot = PlayerDirection;
        //    }

        //    transform.rotation = Quaternion.LookRotation(PlayerDirection);



        //}

    }
    
    public void DirectionCircleControl()
    {

        if (EnemyTarget != null)
        {
            if (!DirectionCircle.activeInHierarchy)
            { DirectionCircle.SetActive(true); }
            PlayerDirection = (EnemyTarget.transform.position - transform.position);
            PlayerDirection = new Vector3(PlayerDirection.x, 0, PlayerDirection.z).normalized;
            DirectionCircle.transform.rotation = Quaternion.LookRotation(PlayerDirection);
                
        }
        else if (CastingSpellCour != null)
        {
            if (!DirectionCircle.activeInHierarchy)
            { DirectionCircle.SetActive(true); }
            PlayerDirection = new Vector3(spellPos.x, transform.position.y, spellPos.z) - transform.position;
            transform.rotation = Quaternion.LookRotation(PlayerDirection);
            DirectionCircle.transform.rotation = Quaternion.LookRotation(PlayerDirection);
        }
        else if (TalkTargetControl != null)
        {
            if (!DirectionCircle.activeInHierarchy)
            { DirectionCircle.SetActive(true); }
            PlayerDirection = (TalkTargetControl.transform.position - transform.position);
            PlayerDirection = new Vector3(PlayerDirection.x, 0, PlayerDirection.z).normalized;
            DirectionCircle.transform.rotation = Quaternion.LookRotation(PlayerDirection);
        }
        else if (agent.hasPath || agent.pathPending)
        {
            if (!DirectionCircle.activeInHierarchy)
            {DirectionCircle.SetActive(true);}
            PlayerDirection = (agent.destination - transform.position);
            PlayerDirection = new Vector3(PlayerDirection.x, 0, PlayerDirection.z).normalized;
            DirectionCircle.transform.rotation = Quaternion.LookRotation(PlayerDirection);
        }
        else
        {
            if (DirectionCircle.activeInHierarchy)
            { DirectionCircle.SetActive(false); }
        }
    }
    private void KdControl()
    {
        if (currentStamina > 0f)
        {
            currentStamina -= 1f * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, 100f);
        }
        else if (currentStamina < 0f)
        {
            currentStamina = Mathf.Clamp(currentStamina, 0, 100f);
        }
    }
    //spell
    public void TryCast(Spell spell, Vector3 CursorPosition, PlayerControl pl = null, Creature cr = null, Transform SpellTransform = null)
    {
        agent.updateRotation = false;
        currentActivityImage.sprite = spell.Icon;
        if (CastingSpellCour != null)
        {
            StopCoroutine(CastingSpellCour);
            CastingSpellCour = null;
            animator.SetFloat("SpellCast", 0);
        }
        if (AttackingCour != null)
        {
            StopCoroutine(AttackingCour);
        }
        SpellRadiusCircle.transform.localScale = new Vector3(spell.CastRadius * 2, spell.CastRadius * 2, 1);
        SpellRadiusCircle.gameObject.SetActive(true);
        CastingSpellCour = StartCoroutine(CastSpell(spell, CursorPosition,pl, cr, SpellTransform));
    }
    public IEnumerator CastSpell(Spell spell, Vector3 CursorPosition, PlayerControl pl = null, Creature cr = null, Transform SpellTransform = null)
    {
        SpellRadiusCircle.gameObject.SetActive(false);
        yield return new WaitUntil(() => currentStamina < 0.01f);
        animator.SetBool("SpellPrepare", true);
        yield return new WaitForSeconds(spell.—astTimer);
        animator.SetBool("SpellPrepare", false);
        animator.SetFloat("SpellCast", 1);
        //     if (spell.SpellCastAnimation) { animator.SetFloat("SpellCast", 1); }
        //else { animator.SetFloat("Attack", 1); }
        //else { animator.SetFloat("Attack", 1); }
        yield return new WaitForSeconds(0.18f);
        List<GameObject> spellObjects = new List<GameObject> { };
        int cnt = 0;
        if (SpellTransform !=  null)
        {
            for (int i = 0; cnt < spell.Count; i++)
            {
                if (!SpellTransform.GetChild(i).gameObject.activeInHierarchy)
                {
                    spellObjects.Add(SpellTransform.GetChild(i).gameObject);
                    Debug.Log(SpellTransform.GetChild(i).name);
                    cnt++;
                }
            }
        }
        if (spell.CastAtPoint)
        {
            foreach (GameObject obj in spellObjects)
            {
                obj.transform.position = CursorPosition;
                obj.SetActive(true);
            }
        }
        else if (spell.CastFromPlayer)
        {
            foreach (GameObject obj in spellObjects)
            {
                Vector3 direction = ((CursorPosition - transform.position).normalized);
                Quaternion Rotation = Quaternion.LookRotation(direction);
                Rotation.x = 0;
                Rotation.z = 0;
                obj.transform.position = transform.position;
                obj.transform.rotation = Rotation;
                obj.SetActive(true);
            }
        }
        else if (spell.CastDirected)
        {
            if (SpellTransform != null)
            {
                foreach (GameObject obj in spellObjects)
                {
                    DirectedSpell dirSpell = obj.GetComponent<DirectedSpell>();
                    if (dirSpell == null) { Debug.Log("DIRRR"); }
                    dirSpell.targetCreature = cr;
                    dirSpell.targerPlayer = pl;
                    dirSpell.playerInt = Intelligence + buffIntelligence;
                    dirSpell.playerStr = Strength + buffStrength;
                    dirSpell.gameObject.transform.position = transform.position;
                    dirSpell.gameObject.SetActive(true);
                }
            }
        }
        yield return new WaitForSeconds(1.05f-0.18f);
        currentStamina = maxStamina;
        CastingSpellCour = null;
        animator.SetFloat("SpellCast", 0);
        currentActivityImage.sprite = baseActivityImage;
        CastingSpellCour = null;
        agent.updateRotation = true;
    }
    //mele
    public void TryMele(GameObject SpellObj, float KastTimer, float KdAfterSpell, bool CastAtPoint, Vector3 CursorPosition)
    {
        currentActivityImage.sprite = attackSprite;
        if (CastingSpellCour != null)
        {
            StopCoroutine(CastingSpellCour);
            CastingSpellCour = null;
            animator.SetFloat("SpellCast", 0);
        }
        if (AttackingCour != null)
        {
            StopCoroutine(AttackingCour);
        }
        //change damage
        AttackingCour = StartCoroutine(Attack(Damage));
    }



    private IEnumerator Attack(float damage)
    {
        animator.SetInteger("Random", UnityEngine.Random.Range(0, 2));
        animator.SetFloat("Attack", 1);

        Debug.Log("Attack");
        IsAttacking = true;// dealDamage = false;
        //yield return new WaitUntil(()=> dealDamage);
        if (Weapon)
        { yield return new WaitForSeconds(Weapon.AttackDurationBefore); }
        else { yield return new WaitForSeconds(HandAttackDurationBefore); }
        EnemyTarget.DamageTake(damage + (Strength + Dexterity + buffStrength + buffDexterity) * 0.5f, true, this);
        if (Weapon)
        { yield return new WaitForSeconds(Weapon.AttackDurationAfter); }
        else { yield return new WaitForSeconds(HandAttackDurationAfter); }
        IsAttacking = false;

        currentStamina = maxStamina;
        AttackingCour = null;

        animator.SetFloat("Attack", 0);
    }

    private void OnMouseEnter()
    {
        if (!cursorController.PrepearingCastSpell)
            { 
            SelectCircle.SetActive(true);
            if (!InFight)
            {
                hpBarController.gameObject.SetActive(true);
            }
        }
    }
    private void OnMouseExit()
    {
        if (!cursorController.PrepearingCastSpell)
        { 
            if (!InFight)
            {
                hpBarController.gameObject.SetActive(false);
            }
        }
    }

    private void GroundRaycast()
    {
        Ray GroundRay = new Ray(transform.position, Vector3.down);
        RaycastHit groundHit;
        bool didHit = Physics.Raycast(GroundRay, out groundHit, 20f, GroundLayer);
        if (didHit && groundHit.collider != null)
        {
            Vector3 direction = groundHit.point + Vector3.up * groundOffset;
            transform.position = Vector3.Lerp(transform.position, direction, snapSpeed * Time.deltaTime);
        }
    }

    public void SetTargetPosition(Vector3 pos, bool IsEnemy, Vector3 ViewPos)
    {
        Vector3 FinalTarget = pos;
        RaycastHit hit;

        agent.SetDestination(pos);
        ViewPointAfterMove = ViewPos;
        if (!IsEnemy)
        {
            _TargetPointCircle.transform.position = new Vector3(pos.x, pos.y + 0.05f, pos.z);
            _TargetPointCircle.SetActive(true);
        }
    }
    public void DamageTake(float damage, bool makeAnim, Creature cr = null)
    {
        if (makeAnim)
        {
            animator.SetBool("Hit", true);
        }
        _Hp -= Mathf.Clamp(damage-defence/2, 1, Mathf.Infinity);
        _Hp = Mathf.Clamp(_Hp, 0, _MaxHp);
        hpBarController.UpdateHp(_Hp, _MaxHp);

        if (_Hp <= 0)
        {
            StartCoroutine(Die());
            if (cr != null)
            {
                cr.target = null;
            }
        }
    }
    private IEnumerator Die()
    {
        animator.SetTrigger("Death");
        HitBox.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("Nothing");
        agent.ResetPath(); EnemyTarget = null; LootTarget = null; TalkTargetControl = null;
        Debug.Log("Player died!");
        cursorController.RemoveFromMainTeam(this);
        if (cursorController.selectedTeam.Contains(this))
            { cursorController.selectedTeam.Remove(this); }
        if (cursorController.CurrentPlayerContr == this)
        { cursorController.CurrentPlayerContr = null; }

        yield return new WaitForSeconds(3.25f);
        Color newColor = bodyBagRender.material.color;
        newColor.a = 0f;
        bodyBagRender.material.color = newColor;
        bodyBagRender.material.DOFade(1f, 2f);
        bodyBag.SetActive(true);
    }

    public void GetHeal(float heal)
    {
        _Hp += heal;
        hpBarController.UpdateHp(_Hp, _MaxHp);
    }

    //aniamtion attack

    private void SetAttackAnimator(float sc)
    {
        animator.SetFloat("Attack", sc);
    }
    private void SetCastSpellAnimator(float sc)
    {
        animator.SetFloat("CastSpell", sc);
    }
}



