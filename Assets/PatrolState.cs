using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Optionally, set the "EnemyWalk" animation (though your Animator State itself 
        // might already be tied to that animation clip)
        // animator.Play("EnemyWalk");

        // If you want to be sure velocity is reset:
        Enemy enemy = animator.GetComponent<Enemy>();
        enemy.body.velocity = Vector2.zero;
    }

    // Called every frame while we are in the Patrol state
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Enemy enemy = animator.GetComponent<Enemy>();

        // 1) Perform movement
        enemy.PatrolMovement();

        // 2) Transition checks
        // If the user starts dragging (isDragging = true), we set "toControl" 
        // so the Animator transitions to ControlState
        if (enemy.isDragging)
        {
            animator.SetTrigger("toControl");
        }
        // Or if the enemy is falling (no ground), go to FallingState
        else if (enemy.IsFalling())
        {
            animator.SetTrigger("toFalling");
        }
    }

    // Called once when we EXIT this Patrol state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Clear triggers if needed or do state cleanup
        // animator.ResetTrigger("toControl");
        // animator.ResetTrigger("toFalling");
    }
}

