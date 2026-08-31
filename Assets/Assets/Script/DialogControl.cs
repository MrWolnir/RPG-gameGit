using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class DialogControl : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Serializable]
    public class StringArray
    {
        public string[] array;
    }
    [Serializable]
    public class EventsArray
    {
        public UnityEvent[] events;
    }

    
    public StringArray[] Answers; //what player answer
    [Header("0-continue 1-leave 2-fight 3-buy 4-take quest")]
    public EventsArray[] Actions; //0-continue 1-leave 2-fight 3-buy 4-take quest
    public String[] Texts; //what man sayed


}
