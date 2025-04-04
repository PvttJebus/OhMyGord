using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolState : StateMachineBehaviour
{
    //Thought this would work, but it didn't (explained in ControlState)
    //public Enemy enemy;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //reseting the enemy velocity to zero isn't totally needed, but I put it as a fall back to ensure it's no-longer falling or thinks it's falling. 
        Enemy enemy = animator.GetComponent<Enemy>();
        enemy.body.velocity = Vector2.zero;
    }

    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Enemy enemy = animator.GetComponent<Enemy>();

        enemy.PatrolMovement();

        //I didn't expect this to work as It's different from the code discussed in class, but since it's using the animator
        //It just leverages the trigger I create, so us supprisingly easy. 
        if (enemy.isDragging == true)
        {
            animator.SetTrigger("toControl");
        }
        
       
        else if (enemy.IsFalling() == true)
        {
            animator.SetTrigger("toFalling");
        }
    }

    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}

