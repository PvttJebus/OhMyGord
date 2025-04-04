using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlState : StateMachineBehaviour
{
    // OnStateEnter: do anything you need right as we switch to “Control” 
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Possibly set an “EnemyDragged” animation or freeze velocity
        Enemy enemy = animator.GetComponent<Enemy>();
        enemy.body.velocity = Vector2.zero;
        // If you have a “grab” animation, you could do: animator.Play("EnemyGrab");
    }

    // Called every frame while in "Control" state
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Enemy enemy = animator.GetComponent<Enemy>();

        // If still dragging, follow the mouse
        if (enemy.isDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            enemy.body.MovePosition(mousePos);
        }
        else
        {
            // If we are no longer dragging, presumably the user let go => transition
            // We'll set "toFalling" so it can drop
            animator.SetTrigger("toFalling");
        }
    }

    // OnStateExit: optionally do cleanup
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // e.g. reset triggers or revert any special state
    }
}
