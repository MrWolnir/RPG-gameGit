using System;
using System.Collections;
using UnityEngine;

namespace Assets.Script
{
    [Serializable]
    public class stats
    {
        public int buffDefence;
        public int buffStrength; //damage and block
        public int buffDexterity; // Damage and dodge
        public int buffConstitution; // Hp
        public int buffIntelligence; // Mage Damage

    }
    internal class DirectedSpell: MonoBehaviour
    {
        [SerializeField] Rigidbody rb;

        [Header("Heal")]
        [SerializeField] private float heal;
        [Header("For contant heal")]
        [SerializeField] private bool isPeriodicHeal;
        [SerializeField] private float healInterval;
        [SerializeField] private float healTimer;

        [Header("Damage")]
        [SerializeField] private int Damage;
        [Header("For 'potion'")]
        [SerializeField] private bool isPeriodicDamage;
        [SerializeField] private float damageInterval;
        [SerializeField] private float damageTimer;

        [Header("true - smth fly to target // false - stay on target")]
        [SerializeField] private bool directedAttack;
        [Header("Buff/Debuff")]
        [SerializeField] private bool statusEffect;
        [SerializeField] private float timer;

        [SerializeField] private stats stat;

        [SerializeField] private int Speed;
        [SerializeField] private bool isMagicScale;
        public int playerInt;
        public int playerStr;
        public PlayerControl targerPlayer;
        public Creature targetCreature;

        private GameObject target;

        private Coroutine buffCour = null;
        private Coroutine damageCour = null;
        private Coroutine healCour = null;

        [Header("Not necessarily")]
        [SerializeField] private ParticleSystem particle;
        private void FixedUpdate()
        {
            if (target != null)
            {
                if (directedAttack)
                {
                    Vector3 direction = (target.transform.position - transform.position).normalized;
                    rb.linearVelocity = direction * Speed;
                    transform.LookAt(target.transform.position);
                }
                else
                {
                    gameObject.transform.position = target.transform.position;
                }

                if (targerPlayer)
                {
                    if (targerPlayer._Hp <= 0)
                    {
                        gameObject.SetActive(false);
                    }
                }
                else if (targetCreature)
                {
                    if (targetCreature.hp <= 0)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
            
        }
        private void OnEnable()
        {
            if (targerPlayer != null) {
                target = targerPlayer.gameObject;
            }
            else if (targetCreature != null)
            {
                target = targetCreature.gameObject;
            }
            if (statusEffect && !directedAttack)
            {
                StartCoroutine(useСonsequence(targerPlayer, targetCreature));
            }
            if (isPeriodicDamage && !directedAttack)
            {
                StartCoroutine(periodicDamage(targerPlayer, targetCreature));
            }
            if (heal > 0 && !directedAttack)
            {
                StartCoroutine(periodicHeal(targerPlayer, targetCreature));
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Creature>(out Creature cr))
            {
                if (cr == targetCreature)
                {
                    if (Damage > 0)
                    {
                        if (isMagicScale) { cr.DamageTake(Damage + playerInt, false); }
                        else { cr.DamageTake(Damage + playerStr, false); }
                    }
                    if (heal > 0) { StartCoroutine(periodicHeal(null, cr)); }
                    if (statusEffect)
                    {
                        StartCoroutine(useСonsequence(targerPlayer, targetCreature));
                    }
                    if (particle != null)
                    {
                        particle.gameObject.transform.position = transform.position;
                        particle.Play();
                    }
                    gameObject.SetActive(false);
                }
            }
            if (other.TryGetComponent<PlayerControl>(out PlayerControl pl))
            {
                if (pl == targetCreature)
                {
                    if (Damage > 0)
                    {
                        if (isMagicScale) { pl.DamageTake(Damage + playerInt, false); }
                        else { pl.DamageTake(Damage + playerStr, false); }
                    }
                    if (heal > 0) { StartCoroutine(periodicHeal(pl, null)); }
                    if (statusEffect)
                    {
                        StartCoroutine(useСonsequence(targerPlayer, targetCreature));
                    }
                    if (particle != null)
                    {
                        particle.gameObject.transform.position = transform.position;
                        particle.Play();
                    }
                    gameObject.SetActive(false);
                }
            }
        }
        private IEnumerator useСonsequence(PlayerControl pl, Creature cr) //последствия
        {
            if (pl != null)
            {
                pl.buffDefence += stat.buffDefence;
                pl.buffStrength += stat.buffStrength;
                pl.buffDexterity += stat.buffDexterity;
                pl.buffConstitution += stat.buffConstitution;
                pl.buffIntelligence += stat.buffIntelligence;
            }
            else if (cr != null)
            {
                cr.defence += stat.buffDefence;
                cr.Strength += stat.buffStrength;
                cr.Dexterity += stat.buffDexterity;
                cr.Constitution += stat.buffConstitution;
                cr.Intelligence += stat.buffIntelligence;

            }
            yield return new WaitForSeconds(timer);
            if (pl != null)
            {
                pl.buffDefence -= stat.buffDefence;
                pl.buffStrength -= stat.buffStrength;
                pl.buffDexterity -= stat.buffDexterity;
                pl.buffConstitution -= stat.buffConstitution;
                pl.buffIntelligence -= stat.buffIntelligence;
            }
            else if (cr != null)
            {
                cr.defence -= stat.buffDefence;
                cr.Strength -= stat.buffStrength;
                cr.Dexterity -= stat.buffDexterity;
                cr.Constitution -= stat.buffConstitution;
                cr.Intelligence -= stat.buffIntelligence;

            }
            buffCour = null;
            if (damageCour == null && healCour == null) { gameObject.SetActive(false); }
        }
        private IEnumerator periodicDamage(PlayerControl pl, Creature cr) //последствия
        {
            float glTm = 0f;
            float tm = 0f;
            while (glTm < damageTimer)
            {
                tm += Time.deltaTime;
                glTm += Time.deltaTime;
                if (tm > damageInterval)
                {
                    tm = 0f;
                    if (pl != null)
                    { pl.DamageTake(Damage, false); }
                    else if (cr != null)
                    { cr.DamageTake(Damage, false); }
                }
            }
            yield return new WaitUntil(()=>(glTm> damageTimer));
            damageCour = null;
            if (healCour == null && buffCour == null) { }
        }
        private IEnumerator periodicHeal(PlayerControl pl, Creature cr) //последствия
        {
            float glTm = 0f;
            float tm = 10f;
            if (isPeriodicHeal)
            {
                while (glTm < healTimer)
                {
                    if (tm > healInterval)
                    {
                        tm = 0f;
                        if (pl != null)
                        { pl.GetHeal(heal); }
                        else if (cr != null)
                        { cr.GetHeal(heal); }
                    }
                    tm += Time.deltaTime;
                    glTm += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                if (pl != null)
                { pl.GetHeal(heal); }
                else if (cr != null)
                { cr.GetHeal(heal); }
                yield return new WaitForSeconds(4f);
            }

            healCour = null;
            if (damageCour == null && buffCour == null) { gameObject.SetActive(false); }

        }

    }
    
}
