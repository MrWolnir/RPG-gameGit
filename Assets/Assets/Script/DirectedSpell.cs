using System;
using UnityEngine;

namespace Assets.Script
{
    internal class DirectedSpell: MonoBehaviour
    {
        [SerializeField] Rigidbody rb;

        [SerializeField] private int Damage;
        [SerializeField] private int Heal;

        [SerializeField] private int Speed;
        [SerializeField] private bool isMagicScale;
        public int playerInt;
        public int playerStr;
        public PlayerControl targerPlayer;
        public Creature targetCreature;

        private GameObject target;
        private void FixedUpdate()
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * Speed;
            transform.LookAt(target.transform.position);
        }
        private void OnEnable()
        {
            if (targerPlayer != null) { target = targerPlayer.gameObject; }
            else {target = targetCreature.gameObject; }
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
                    else if (Heal>0) { cr.GetHeal(Heal); }
                    gameObject.SetActive(false);
                }
            }
            else if (other.TryGetComponent<PlayerControl>(out PlayerControl pl))
            {
                if (pl == targerPlayer)
                {
                    if (Damage > 0)
                    {
                        if (isMagicScale) { pl.DamageTake(Damage + playerInt, false); }
                        else { pl.DamageTake(Damage + playerStr, false); }
                    }
                    else if (Heal > 0) { pl.GetHeal(Heal); }
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
