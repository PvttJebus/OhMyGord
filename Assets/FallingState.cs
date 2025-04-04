using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingState : StateMachineBehaviour
{
    // Called once when we enter the "Falling" state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Enemy enemy = animator.GetComponent<Enemy>();
        // Possibly set velocity or a "fall" animation
        // e.g. animator.Play("EnemyFall");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Enemy enemy = animator.GetComponent<Enemy>();

        // If we landed back on ground, go to Patrol
        if (!enemy.IsFalling())
        {
            animator.SetTrigger("toPatrol");
        }

        // If you want a condition to destroy the enemy (like falling out of bounds),
        // you can detect that here. For example:
        // if (enemy.transform.position.y < -10f)
        // {
        //     // transition to a "Dead" or "Destroy" state, or just Destroy(enemy.gameObject)
        // }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cleanup or reset triggers if needed
    }
}

