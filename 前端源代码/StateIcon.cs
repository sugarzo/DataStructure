using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateIcon : MonoBehaviour
{
    private Animator animator;
    public State OriState = State.Waiting;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        SetState(OriState);
    }
    public enum State
    { 
        Waiting,Loading,Finish,Error,
    }
    public void SetState(State state)
    {
        if(animator == null)
            animator = GetComponent<Animator>();
        switch (state)
        {
            case State.Waiting:
                animator.SetTrigger("Waiting");
                break;
            case State.Finish:
                animator.SetTrigger("Finish");
                break;
            case State.Error:
                animator.SetTrigger("Error");
                break;
            case State.Loading:
                animator.SetTrigger("Loading");
                break;
        }

            
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
