using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class DamageDealTrig : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool All;
    public bool Enemy;
    public bool Player;

    [SerializeField] private bool makeAnim;

    public float Damage;

    public bool InstantHeal;

    public bool IntervalHeal;
    public float Heal;

    public bool IntervalDamage;

    public bool InstantDamage;

    public float Interval;
    private float DamageTimer;
    private float HealTimer;

    [SerializeField] private List<Creature> enemies = new List<Creature>();
    [SerializeField] private List<PlayerControl> players = new List<PlayerControl>();

    // Update is called once per frame
    void Update()
    {
        if (IntervalDamage && (enemies.Count > 0 || players.Count > 0))
        {
            DamageTimer += Time.deltaTime;
            if (DamageTimer > Interval)
            {
                DamageTimer = 0f;
                DealEverybodyDamage();
            }
        }
        if (IntervalHeal && (enemies.Count > 0 || players.Count > 0))
        {
            HealTimer += Time.deltaTime;
            if (HealTimer > Interval)
            {
                HealTimer = 0f;
                DealEverybodyHeal();
            }
        }
    }
    private void DealEverybodyDamage()
    {
        if (enemies != null)
        {
            foreach (Creature enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.DamageTake(Damage, false);
                }
            }
        }
        if (players != null)
        {
            foreach (PlayerControl player in players)
            {
                if (player != null)
                {
                    player.DamageTake(Damage, makeAnim);
                }
            }
        }
    }
    private void DealEverybodyHeal()
    {
        if (enemies != null)
        {
            foreach (Creature enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.GetHeal(Heal);
                }
            }
        }
        if (players != null)
        {
            foreach (PlayerControl player in players)
            {
                if (player != null)
                {
                    player.GetHeal(Heal);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Enemy || All)
        {
            if (other.TryGetComponent<Creature>(out Creature control))
            {
                if (IntervalDamage)
                {
                    if (!enemies.Contains(control))
                    {
                        enemies.Add(control);
                    }
                }
                if (InstantHeal)
                {
                    control.GetHeal(Heal);
                }
                if (InstantDamage)
                {
                    control.DamageTake(Damage, false);
                }
            }
        }
        if (Player || All)
        {
            if (other.TryGetComponent<PlayerControl>(out PlayerControl control))
            {
                if (IntervalDamage || IntervalHeal)
                {
                    if (!players.Contains(control))
                    {
                        players.Add(control);
                    }
                }
                if (InstantHeal)
                {
                    control.GetHeal(Heal);
                }
                if (InstantDamage)
                {
                    control.DamageTake(Damage, makeAnim);
                }
            }
                

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (IntervalDamage)
        {
            if (other.TryGetComponent<Creature>(out Creature enemy))
            {
                if (enemies.Contains(enemy))
                {
                    enemies.Remove(enemy);
                }
            }
            if (other.TryGetComponent<PlayerControl>(out PlayerControl pl))
            {
                if (players.Contains(pl))
                {
                    players.Remove(pl);
                }
            }
        }
    }

}
