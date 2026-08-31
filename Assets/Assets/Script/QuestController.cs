//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Unity.VisualScripting;
//using UnityEngine;

//public class QuestController : MonoBehaviour
//{
//    [Serializable]
//    public class intValue
//    {
//        public int[] value;
//    }
//    //initialize
//    [SerializeField] private UiController uiController;

//    public Quest[] quests;

//    public string[] titles;
//    [SerializeField] public intValue[] values;

//    public Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>();




//    private void OnEnable()
//    {
//        EventBus.setQuestPar += SetQuestPar;
//    }
//    private void OnDisable()
//    {
//        EventBus.setQuestPar -= SetQuestPar;
//    }

//    private void Start()
//    {
//        for (int i = 0; i < titles.Length; i ++)
//        {
//            dictionary.Add(titles[i], values[i].value);
//        }
//    }
//    private void SetQuestPar(string name, int col, bool add)
//    {
//        if (add)
//        {
//            dictionary[name][0] = Math.Clamp(dictionary[name][0] + col, 0, dictionary[name][1]);
//        }
//        else
//        {
//            dictionary[name][0] = Math.Clamp(col, 0, dictionary[name][1]);
//        }
//        Debug.Log($"{name}, {dictionary[name]}");
//    }
//}
