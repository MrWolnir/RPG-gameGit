using NUnit.Framework;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class SpellPrepare : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is createdz

    public int mask;

    public List<PlayerControl> pl;
    public List<Enemy> en;


    private void OnTriggerEnter(Collider other)
    {
        // 1 - player; 2 - enemy; 3 - all
        if (mask == 1)
        {
            other.gameObject.TryGetComponent<PlayerControl>(out PlayerControl playerControl);
            if (playerControl != null)
            {
                playerControl.SelectCircle.SetActive(true);
                pl.Add(playerControl);
            }
        }
        else if (mask == 2)
        {
            other.gameObject.TryGetComponent<Enemy>(out Enemy enemyControl);
            if (enemyControl != null)
            {
                enemyControl.SelectCircle.SetActive(true);
                en.Add(enemyControl);
            }
            
        }
        else if (mask == 3)
        {
            other.gameObject.TryGetComponent<Enemy>(out Enemy enemyControl);
            if (enemyControl != null)
            {
                enemyControl.SelectCircle.SetActive(true);
                en.Add(enemyControl);
            }
            other.gameObject.TryGetComponent<PlayerControl>(out PlayerControl playerControl);
            if (playerControl != null)
            {
                playerControl.SelectCircle.SetActive(true);
                pl.Add(playerControl);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {

        if (mask == 1)
        {
            other.gameObject.TryGetComponent<PlayerControl>(out PlayerControl playerControl);
            if (playerControl != null)
            {
                playerControl.SelectCircle.SetActive(false);
                if (pl.Contains(playerControl))
                {
                    pl.Remove(playerControl);
                }
            }
        }
        else if (mask == 2)
        {
            other.gameObject.TryGetComponent<Enemy>(out Enemy enemyControl);
            if (enemyControl != null)
            {
                enemyControl.SelectCircle.SetActive(false);
                if (en.Contains(enemyControl))
                {
                    en.Remove(enemyControl);
                }
            }

        }
        else if (mask == 3)
        {
            other.gameObject.TryGetComponent<Enemy>(out Enemy enemyControl);
            if (enemyControl != null)
            {
                enemyControl.SelectCircle.SetActive(false);
                if (en.Contains(enemyControl))
                {
                    en.Remove(enemyControl);
                }
            }
            other.gameObject.TryGetComponent<PlayerControl>(out PlayerControl playerControl);
            if (playerControl != null)
            {
                playerControl.SelectCircle.SetActive(false);
                if (pl.Contains(playerControl))
                {
                    pl.Remove(playerControl);
                }
            }

        }

    }
    private void OnDisable()
    {
        if (en.Count > 0)
        {
            foreach (Enemy i in en)
            {
                i.SelectCircle.SetActive(false);
            }
        }
        if (pl.Count > 0)
        {
            foreach (PlayerControl i in pl)
            {
                i.SelectCircle.SetActive(false);
            }
        }

    }
}
