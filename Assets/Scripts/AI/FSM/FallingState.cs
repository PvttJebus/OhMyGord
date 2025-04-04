using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingState : StateMachineBehaviour
{
    //Thought this would work, but it didn't (explained in ControlState)
    //public Enemy enemy;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Enemy enemy = animator.GetComponent<Enemy>();

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Enemy enemy = animator.GetComponent<Enemy>();

        //I kept facing an issue where the enemy would temporarily exit the control state, go to falling, then patrol, and back
        // all in the course of six or so seconds. I thought adding the second if check would fix it... it did not, but I kept it anyway
        if (enemy.isDragging == false)
        {
            if (enemy.IsFalling() == false)
            {
                animator.SetTrigger("toPatrol");
            }
        }

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}

