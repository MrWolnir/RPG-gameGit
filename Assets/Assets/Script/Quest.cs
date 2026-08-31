using UnityEngine;

public class Quest : MonoBehaviour
{
    public string title;
    public string questName;
    public int maxProgress;
    public int currentProgress;

    public string description;

    public bool isActive;
    public bool isCompleted;

    public bool fulfillOnlyWhenActive;

    public Item[] reward;

}
