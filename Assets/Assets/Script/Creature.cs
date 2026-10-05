using Assets.Script;
using DG.Tweening;
using GDS.Core;
using GDS.Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class Creature : MonoBehaviour
{
    [SerializeField] private bool Dummy;
    //Stats
    [Header("Serialize")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;//speed(float) InFight(bool) MeleAttack(float) CastSpell(float) BowAttack(float) Death(bool)
    [SerializeField] private CursorController cursorController;
    public HitBoxContr hitBoxContr;
    public DialogControl dialogControl;
    [SerializeField] private GameObject HitBox;
    public NavMeshAgent agent;

    [Header("1 - mele; 2 - cast")]
    [SerializeField] private int action; 

    public HpBarControlelr hpBarController;

    [Header("Stats")]
    public string name;

    public float hp;
    public float maxHp;

    public float currentStamina;
    public float maxStamina;

    [SerializeField] private float speed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float walkStopDistance;
    [Header("Quest")]
    public string questName;
    public int questCol;

    [SerializeField] private int exp;

    public int defence;
    public int Strength; //damage and hp
    public int Dexterity; // Damage and attackspeed
    public int Constitution; // Hp
    public int Intelligence;
    public int level;

    //current activity On health bar
    public UnityEngine.UI.Image currentActivityImage;
    [SerializeField] private Sprite baseActivityImage;
    [SerializeField] private Sprite attackActivityImage;


    [SerializeField] private float castChange;
    private Spell currentSpell;
    [SerializeField] private int[] spellsLimit;
    [SerializeField] private Spell[] allSpells;
    [SerializeField] private Spell[] avableSpells;

    private Coroutine CastingSpellCour;
    private Coroutine attackCour;
    [SerializeField] private Transform[] spellCastPos;
    [Header("Active")]

    public bool isEnemy;
    public bool isFriend;
    public bool canTalk;
    public bool talkBeforeFight;

    public bool canAttackPlayer;
    private bool walkWait = false;

    private bool isMove = false;
    private bool moveBlock = false;

    public bool InFight = false;
    [SerializeField] private bool IsAttacking;


    [SerializeField] private float chanceToChangeTarget;

    [Header("Attack")]
    [SerializeField] private int damage;
    public PlayerControl playerTarget = null;
    public Creature creatureTarget = null;
    [SerializeField] private float AttackDistance;

    private float timer = 0;
    private float maxTimer = 0.3f;
    //Set who is it
    [Header("Equip")]

    [SerializeField] private bool Sword;
    [SerializeField] private bool Bow;
    [SerializeField] private bool Staff;

    public bool isDead = false;


    [Header("Another")]
    private bool isDuel;

    private float dop = 0f;
    [SerializeField] private List<GameObject> WalkPoints;
    [SerializeField] private List<float> WalkWaitTime;
    private int index;

    [SerializeField] private GameObject bodyBag;
    [SerializeField] private Renderer bodyBagRender;
    public LayerMask onlyGroundLayer;
    public GameObject SelectCircle;
    public List<Item> Items;
    public List<Creature> team;
    void Start()
    {
        animator.SetBool("Sword", true);
        updateSpells();



    }

    // Update is called once per frame
    void Update()
    {
        if (!Dummy)
        {
            if (playerTarget == null && WalkPoints.Count == 0 && InFight == false)
            {
                rb.linearVelocity = Vector3.zero;
            }
            if (hp > 0 && !isDead)
            {
                animator.SetFloat("speed", agent.velocity.magnitude);


                KdControl();
                Actions();
                }
        }
        hpBarController.UpdateStamina(currentStamina, maxStamina);
        hpBarController.UpdateHp(hp, maxHp);
            
    }
    //attack
    private void Actions()
    {
        if (!InFight)
        {
            if (WalkPoints != null && WalkPoints.Count > 0)
            {
                if (!walkWait)
                {
                    //Vector3 rawDirection = (WalkPoints[index].transform.position - gameObject.transform.position).normalized;
                    //rawDirection.y = 0f;
                    //Vector3 direction = rawDirection;
                    //rb.linearVelocity = direction * walkSpeed;
                    if (!agent.hasPath && !agent.pathPending)
                    { agent.SetDestination(WalkPoints[index].transform.position); }
                    
                    //Vector3 lookDirection = (WalkPoints[index].transform.position - transform.position);
                    //lookDirection = new Vector3(lookDirection.x, 0, lookDirection.z).normalized;
                    //transform.rotation = Quaternion.LookRotation(-lookDirection);
                    if (Vector3.Distance(gameObject.transform.position, new Vector3(WalkPoints[index].transform.position.x, gameObject.transform.position.y, WalkPoints[index].transform.position.z)) < walkStopDistance)
                    {
                        //rb.linearVelocity = Vector3.zero;
                        agent.ResetPath();
                        rb.linearVelocity = new Vector3(0, 0, 0);
                        StartCoroutine(walkWaitCour(WalkWaitTime[index]));
                    }
                }
            }
        }
        else if (InFight)
        {
            if (action == 1)
            {
                meleAttack();
            }
            else if (action == 2) //spell
            {
                magicAttack();
            }
            else
            {//выбор цели
                if (avableSpells.Length > 0)
                {
                    if (UnityEngine.Random.Range(0f, 1f) > castChange)
                    { action = 1; }
                    else 
                    { 
                        currentSpell = null;
                        int attends = 0;
                        while (currentSpell == null && attends<10)
                        {
                            attends++;
                            Spell chosenSpell = avableSpells[UnityEngine.Random.Range(0, avableSpells.Length)];
                            if (chosenSpell.isHeal)
                            {
                                float temp = 1f;
                                foreach (Creature cr in cursorController.enemyTeam)
                                {
                                    if (cr.maxHp - cr.hp > temp)
                                    {
                                        temp = cr.maxHp - cr.hp;
                                        creatureTarget = cr;
                                    }
                                }
                                if (temp != 1f)
                                {
                                    currentSpell = chosenSpell;
                                }

                            }
                            else if (chosenSpell.isBuff)
                            {
                                creatureTarget = cursorController.enemyTeam[UnityEngine.Random.Range(0, cursorController.enemyTeam.Count)];
                                currentSpell = chosenSpell;
                            }
                            else
                            {
                                if (playerTarget == null || playerTarget._Hp < 0)
                                {
                                    PlayerControl pl = null;
                                    float dst = 1000f;
                                    foreach (PlayerControl play in cursorController.mainTeam)
                                    {
                                        if (play != playerTarget && play._Hp > 0)
                                        {
                                            float newDists = Vector3.Distance(transform.position, play.transform.position);
                                            if (newDists < dst)
                                            {
                                                pl = play;
                                                dst = newDists;
                                            }
                                        }
                                    }
                                    if (pl != null)
                                    { playerTarget = pl; }
                                }
                                currentSpell = chosenSpell;
                            }
                        }
                        if (currentSpell != null)
                        { action = 2; }
                        else { action = 1; }
                        

                    }
                }
                else
                {
                    action = 1;
                }
            }
        }
    }

    private void meleAttack()
    {
        if (playerTarget != null)
        {
            currentActivityImage.sprite = attackActivityImage;
            isMove = true;
            if (AttackDistance < Vector3.Distance(gameObject.transform.position, playerTarget.transform.position) - dop)
            {
                if (isDuel)//удар и выбор цели при выходе из боя
                {
                    StartCoroutine(Attack(damage)); isDuel = false;
                    PlayerControl pl = null;
                    float dst = 1000f;
                    foreach (PlayerControl play in cursorController.mainTeam)
                    {
                        if (play != playerTarget && play._Hp > 0 && Vector3.Distance(transform.position, play.transform.position) < 5f)
                        {
                            float newDists = Vector3.Distance(transform.position, play.transform.position);
                            if (newDists < dst)
                            {
                                pl = play;
                                dst = newDists;
                            }
                        }
                    }
                    if (pl != null)
                    {
                        playerTarget = pl;
                    }
                }
                if (!moveBlock)
                {
                    dop = 0f;
                    timer += Time.deltaTime;

                    if (timer > maxTimer)
                    {
                        agent.SetDestination(playerTarget.transform.position);
                        timer = 0f;
                    }
                }
            }
            else if (AttackDistance + dop > Vector3.Distance(gameObject.transform.position, playerTarget.transform.position) - dop)
            {
                dop = 0.2f;
                isDuel = true;
                isMove = false;
                Vector3 lookDirection = (playerTarget.transform.position - transform.position);
                lookDirection = new Vector3(lookDirection.x, 0, lookDirection.z).normalized;
                transform.rotation = Quaternion.LookRotation(lookDirection);
                agent.ResetPath();
                if (currentStamina <= 0.01f && !IsAttacking)
                {
                    attackCour = StartCoroutine(Attack(damage));
                }
            }
        }
        else
        {
            PlayerControl pl = null;
            float dst = 1000f;
            foreach (PlayerControl play in cursorController.mainTeam)
            {
                float newDists = Vector3.Distance(transform.position, play.transform.position);
                if (newDists < dst)
                {
                    pl = play;
                    dst = newDists;
                }
            }
            if (pl != null)
            {
                playerTarget = pl;
            }
        }
    }

    private void magicAttack()
    {
        if (CastingSpellCour == null)
        {
            currentActivityImage.sprite = currentSpell.Icon;
            if (currentSpell.CastRadius < Vector3.Distance(gameObject.transform.position, playerTarget.transform.position) - dop)
            {
                timer += Time.deltaTime;
                if (timer > maxTimer)
                {
                    agent.SetDestination(playerTarget.transform.position);
                    timer = 0f;
                }
            }
            else if (currentSpell.CastRadius > Vector3.Distance(gameObject.transform.position, playerTarget.transform.position) - dop)
            {
                isMove = false;
                Vector3 lookDirection = (playerTarget.transform.position - transform.position);
                lookDirection = new Vector3(lookDirection.x, 0, lookDirection.z).normalized;
                transform.rotation = Quaternion.LookRotation(lookDirection);
                agent.ResetPath();
                if (currentStamina <= 0.01f && !IsAttacking)
                {
                    if (currentSpell.isBuff || currentSpell.isHeal)
                    {
                        CastingSpellCour = StartCoroutine(CastSpell(currentSpell, null, creatureTarget, cursorController.SpellTransforms[currentSpell.SpellTransformIndex]));
                    }
                    else
                    {
                        CastingSpellCour = StartCoroutine(CastSpell(currentSpell, playerTarget, null, cursorController.SpellTransforms[currentSpell.SpellTransformIndex]));
                    }
                        
                }
            }
        }
    }
    private IEnumerator Attack(float damage)
    {
        moveBlock = true;
        currentActivityImage.sprite = attackActivityImage;
        animator.SetFloat("Attack", 1);

        Debug.Log("Attack");
        IsAttacking = true;
        playerTarget.DamageTake(damage - defence / 2, true, this);
        yield return new WaitForSeconds(1.16f);
        IsAttacking = false;
        animator.SetFloat("Attack", 0);
        currentStamina = maxStamina;

        action = -1;
        moveBlock = false;
    }
    public IEnumerator CastSpell(Spell spell, PlayerControl pl = null, Creature cr = null, Transform SpellTransform = null)
    {   
        Transform targ = null;
        
        if (pl != null) { targ = pl.transform; }
        else if (cr != null) { targ = cr.transform; }
        agent.SetDestination(targ.transform.position);

        if (targ != null)
        { yield return new WaitUntil(() => (currentStamina < 0.01f && Vector3.Distance(transform.position, targ.transform.position) < spell.CastRadius + 0.2f)); }
        else { yield return new WaitUntil(() => (currentStamina < 0.01f)); }
        agent.ResetPath();

        animator.SetBool("SpellPrepare", true);
        yield return new WaitForSeconds(spell.СastTimer);
        animator.SetBool("SpellPrepare", false);
        animator.SetFloat("SpellCast", 1);
        //     if (spell.SpellCastAnimation) { animator.SetFloat("SpellCast", 1); }
        //else { animator.SetFloat("Attack", 1); }
        yield return new WaitForSeconds(0.18f);
        List<GameObject> spellObjects = new List<GameObject> { };
        int cnt = 0;
        spellsLimit[System.Array.IndexOf(allSpells, currentSpell)] -= 1;
        updateSpells();
        if (SpellTransform != null)
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
                obj.transform.position = targ.position;
                obj.SetActive(true);
            }
        }
        //else if (spell.CastFromPlayer)
        //{
        //    foreach (GameObject obj in spellObjects)
        //    {
        //        Vector3 direction = ((CursorPosition - transform.position).normalized);
        //        Quaternion Rotation = Quaternion.LookRotation(direction);
        //        Rotation.x = 0;
        //        Rotation.z = 0;
        //        obj.transform.position = transform.position;
        //        obj.transform.rotation = Rotation;
        //        obj.SetActive(true);
        //    }
        //}
        else if (spell.CastDirected)
        {
            Debug.Log("SpellCasted");
            if (SpellTransform != null)
            {
                int ind = 0;
                foreach (GameObject obj in spellObjects)
                {
                    DirectedSpell dirSpell = obj.GetComponent<DirectedSpell>();
                    dirSpell.targetCreature = cr;
                    dirSpell.targerPlayer = pl;
                    dirSpell.playerInt = Intelligence;
                    dirSpell.playerStr = Strength;
                    dirSpell.gameObject.transform.position = spellCastPos[ind].position;
                    dirSpell.gameObject.SetActive(true);
                    ind++;
                }
            }
        }
        yield return new WaitForSeconds(1.05f - 0.18f);
        currentStamina = maxStamina;
        animator.SetFloat("SpellCast", 0);
        currentActivityImage.sprite = baseActivityImage;
        CastingSpellCour = null;
        agent.updateRotation = true;

        action = -1;
    }
    private void updateSpells()
    {
        for (int i = 0; i<allSpells.Length; i++)
        {
            avableSpells = new Spell[0];
            if (spellsLimit[i] != 0)
            {
                avableSpells.Append(allSpells[i]);
            }
        }
    }
    //to stop while point walking
    private IEnumerator walkWaitCour(float waitTime)
    {
        walkWait = true;
        yield return new WaitForSeconds(waitTime);

        if (index == WalkPoints.Count - 1)
        {
            index = 0;
        }
        else
        {
            index += 1;
        }
        walkWait = false;
    }
    //change animator stat
    public void ChangeAnimator(string name, float valueFloat)
    {
        animator.SetFloat(name, valueFloat);
    }
    public void ChangeAnimator(string name, bool valueBool)
    {
        animator.SetBool(name, valueBool);
    }

    private IEnumerator UnsetAnimatorValue(string name, float waitTime, float floatValue)
    {
        yield return new WaitForSeconds(waitTime);
        animator.SetFloat(name, floatValue);
    }
    private IEnumerator UnsetAnimatorValue(string name, float waitTime, bool boolValue)
    {
        yield return new WaitForSeconds(waitTime);
        animator.SetBool(name, boolValue);

    }
    public void DamageTake(float damage, bool makeAnim, PlayerControl control = null)
    {
        hp -= damage;
        hp = Mathf.Clamp(0, hp, maxHp);


        hpBarController.UpdateHp(hp, maxHp);
        if (canAttackPlayer)
        {
            if (control != null)
            {
                if (!InFight)
                {//if not in fight all team attack nearest plyer
                    foreach (Creature cr in team)
                    {
                        float dst = 0f;
                        foreach (PlayerControl contr in cursorController.mainTeam)
                        {
                            if (Vector3.Distance(contr.transform.position, cr.transform.position) < dst)
                            {
                                dst = Vector3.Distance(contr.transform.position, cr.transform.position);
                                cr.playerTarget = contr;
                            }
                        }
                        
                    }
                }
                if (UnityEngine.Random.Range(0f, 1f) < chanceToChangeTarget)
                {
                    playerTarget = control;
                }
            }
        }
        if (hp <= 0)
        {//Dead
            if (cursorController.enemyTeam.Contains(this))
            {
                cursorController.enemyTeam.Remove(this);
            }
            foreach (PlayerControl pl in cursorController.mainTeam)
            {
                pl.getExp(exp);
                if (pl.EnemyTarget == this)
                {
                    if (cursorController.enemyTeam.Count > 0)
                    {
                        float dst = 10000;
                        Creature creat = null;
                        foreach (Creature cr in cursorController.enemyTeam)
                        {
                            if (math.distance(pl.transform.position, cr.transform.position) < dst)
                            {
                                creat = cr;
                            }
                        }
                        pl.EnemyTarget = creat;
                    }
                }
            }
            if (cursorController.enemyTeam.Count == 0)
            {
                //All Dead
                foreach (PlayerControl contr in cursorController.mainTeam)
                {
                    contr.EnemyTarget = null;
                    contr.InFight = false;
                }
                cursorController.EndFight();
            }
            if (!isDead)
            {
                EventBus.setQuestPar?.Invoke(questName, questCol, true);
                animator.SetBool("Death", true);
            }
            if (control != null)
            {
                control.enemyDied();
            }
            StartCoroutine(Die());
        }
        else
        {
            animator.SetBool("hit", true);
            StartCoroutine(UnsetAnimatorValue("hit", 1, false));
        }

    }
    private IEnumerator Die()
    {
        animator.SetTrigger("Death");
        HitBox.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("Nothing");
        WalkPoints = null; playerTarget = null;

        yield return new WaitForSeconds(3.25f);
        Color newColor = bodyBagRender.material.color;
        newColor.a = 0f;
        bodyBagRender.material.color = newColor;
        bodyBagRender.material.DOFade(1f, 2f);
        bodyBag.SetActive(true);
        isDead = true;
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
    //LootBag
    public void SpawnDeadLootBag()
    {
        Debug.Log("Spawn DeadBag");
        gameObject.SetActive(false);
    }

    public void GetHeal(float heal)
    {
        hp += heal;
        hpBarController.UpdateHp(hp, maxHp);
    }

}
