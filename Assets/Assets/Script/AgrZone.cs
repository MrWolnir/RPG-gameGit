using UnityEngine;

public class AgrZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private CursorController cursorControl;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Creature"))
        {
            Creature control = other.GetComponent<Creature>();
            if (control.isEnemy)
            {
                if (control.team.Count == 0)
                {
                    control.team.Add(control);

                }
                cursorControl.StartFight(); //for heroes
                if (cursorControl.enemyTeam.Count == 0)
                {
                    cursorControl.enemyTeam = control.team;
                }

                foreach (Creature cntr in control.team)
                {
                    cntr.InFight = true;
                    cntr.hpBarController.gameObject.SetActive(true);
                    cntr.ChangeAnimator("InFight", true);
                    cntr.SelectCircle.SetActive(true);
                    cntr.hitBoxContr.tempBlockCircle = true;
                }

            }
        }
    }
}
