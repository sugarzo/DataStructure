using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonEvent : MonoBehaviour
{
    //private Button btn;
    private EventTrigger eventTrigger;
    public UnityEvent onEnter;
    public UnityEvent onExit;
    

    public void Enter()
    {
        onEnter?.Invoke();
    }
    public void Exit()
    {
        onExit?.Invoke();
    }
}
