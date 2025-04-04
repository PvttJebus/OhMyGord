using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlState : StateMachineBehaviour
{
    //I thought I could set the variable here and use the on-state enter to forget it, but sadly this didn't seem to work
    //public Enemy enemy;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        Enemy enemy = animator.GetComponent<Enemy>();
        enemy.body.velocity = Vector2.zero;
        
    }

    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
     //so my simple solution is to just have it confirm the animator every update. 
        Enemy enemy = animator.GetComponent<Enemy>();
        if (enemy.isDragging == true)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            enemy.body.MovePosition(mousePos);
        }
        else if (enemy.isDragging == false)
        {
            
            animator.SetTrigger("toFalling");
        }
    }

  
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
