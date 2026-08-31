using GDS.Core;
using GDS.Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;
using DG.Tweening;

public class Creature : MonoBehaviour
{
    //Stats
    [Header("Serialize")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;//speed(float) InFight(bool) MeleAttack(float) CastSpell(float) BowAttack(float) Death(bool)
    [SerializeField] private CursorController cursorController;
    public HitBoxContr hitBoxContr;
    public DialogControl dialogControl;
    [SerializeField] private GameObject HitBox;

    public HpBarControlelr hpBarController;

    public UnityEngine.UI.Image currentActivityImage;
    [SerializeField] private Sprite attackSprite;

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
    [Header("Active")]

    public bool isEnemy;
    public bool isFriend;
    public bool canTalk;
    public bool talkBeforeFight;

    public bool canAttackPlayer;
    private bool walkWait = false;

    private bool isMove = false;

    public bool InFight = false;
    [SerializeField] private bool IsAttacking;


    [SerializeField] private float chanceToChangeTarget;

    [Header("Attack")]
    [SerializeField] private int damage;
    public PlayerControl target = null;
    [SerializeField] private float AttackDistance;


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
        //here
        animator.SetBool("Sword", true);
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null && WalkPoints.Count == 0 && InFight == false)
        {
            rb.linearVelocity = Vector3.zero;
        }
        if (hp > 0 && !isDead)
        {
            Vector3 horisontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            animator.SetFloat("speed", horisontalVelocity.magnitude);


            KdControl();
            Actions();

            hpBarController.UpdateStamina(currentStamina, maxStamina);
            hpBarController.UpdateHp(hp, maxHp);
        }
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
                    Vector3 rawDirection = (WalkPoints[index].transform.position - gameObject.transform.position).normalized;
                    rawDirection.y = 0f;
                    Vector3 direction = rawDirection;
                    rb.linearVelocity = direction * walkSpeed;

                    Vector3 lookDirection = (WalkPoints[index].transform.position - transform.position);
                    lookDirection = new Vector3(lookDirection.x, 0, lookDirection.z).normalized;
                    transform.rotation = Quaternion.LookRotation(-lookDirection);
                    if (Vector3.Distance(gameObject.transform.position, new Vector3(WalkPoints[index].transform.position.x, gameObject.transform.position.y, WalkPoints[index].transform.position.z)) < walkStopDistance)
                    {
                        rb.linearVelocity = Vector3.zero;
                        StartCoroutine(walkWaitCour(WalkWaitTime[index]));
                    }
                }
            }
        }
        else if (InFight)
        {
            if (target != null)
            {
                isMove = true;
                Vector3 lookDirection = (target.transform.position - transform.position);
                lookDirection = new Vector3(lookDirection.x, 0, lookDirection.z).normalized;
                transform.rotation = Quaternion.LookRotation(-lookDirection);
                if (AttackDistance + dop < Vector3.Distance(gameObject.transform.position, target.transform.position))
                {
                    if (isDuel) { StartCoroutine(MeleAttack(damage)); isDuel = false; 
                        PlayerControl pl = null;
                        float dst = 1000f;
                        foreach (PlayerControl play in cursorController.mainTeam)
                        {
                            if (play != target)
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
                            target = pl;
                        }
                        
                    }


                    dop = 0f;
                    Vector3 direction = (target.transform.position - transform.position).normalized;
                    direction.y = 0;
                    rb.linearVelocity = direction * speed;
                }
                else if (AttackDistance + dop > Vector3.Distance(gameObject.transform.position, target.transform.position))
                {
                    isDuel = true;
                    dop = 0.2f;
                    isMove = false;
                    rb.angularVelocity = Vector3.zero;
                    rb.linearVelocity = Vector3.zero;
                    if (currentStamina <= 0.01f && !IsAttacking)
                    {
                        StartCoroutine(MeleAttack(damage));
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
                    target = pl;
                }
            }
        }
    }
    private IEnumerator MeleAttack(float damage)
    {
        currentActivityImage.sprite = attackSprite;
        animator.SetFloat("Attack", 1);

        Debug.Log("Attack");
        IsAttacking = true;
        target.DamageTake(damage- defence/2, true, this);
        yield return new WaitForSeconds(1.16f);
        IsAttacking = false;
        animator.SetFloat("Attack", 0);
        currentStamina = maxStamina;

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
                                cr.target = contr;
                            }
                        }
                        
                    }
                }
                if (UnityEngine.Random.Range(0f, 1f) < chanceToChangeTarget)
                {
                    target = control;
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
        WalkPoints = null; target = null;

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
