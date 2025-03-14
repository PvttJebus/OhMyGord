using UnityEngine;
using System.Collections;
//
//This is the AttackState class. 
//It handles the decisions and actions for the AI character when it is in the attack state.
//It also handles all the possible the transtions OUT of the attack state
//

public class AttackState : FSMState
{
    private AttackAIProperties attackAIProperties;
    private bool moveCloser;

    //Constructor
    public AttackState(AttackAIProperties attackAIProperties)
    {
        stateID = FSMStateID.Attacking;
        this.attackAIProperties = attackAIProperties;
        curSpeed = attackAIProperties.speed;
        curRotSpeed = attackAIProperties.rotSpeed;
              
        moveCloser = false;
    }

    public override void EnterStateInit(Transform player, Transform npc)
    {
        destPos = player.position;
    }

    //
    //Reason - make any decisions required
    //
    public override void Reason(Transform player, Transform npc)
    {
        destPos = player.position;        //Check if NPC has died
        if (npc.GetComponent<MonsterControllerAI>().Health == 0)
        {
            npc.GetComponent<MonsterControllerAI>().PerformTransition(Transition.NoHealth);
            return;
        }

        if (npc.GetComponent<MonsterControllerAI>().Health < 50)
        {
            //Transition to Flee state (it is mapped to the low health transition).
            npc.GetComponent<MonsterControllerAI>().PerformTransition(Transition.LowHealth);
        }
        //if the AI character is close enough to attack
        else if (IsInCurrentRange(npc, destPos, attackAIProperties.attackDistance))
        {
            moveCloser = false;
            // do nothing -- unless health is low (Act function should perform attacks)
            // if the AI character's health is low then FLEE           
        }
        //if the character is not close enough to attack
        else if (IsInCurrentRange(npc, destPos, attackAIProperties.chaseDistance))
        {
            //if we are not close enough to attack and we have enough health, move closer to the "enemy" (which is the player)
            moveCloser = true;
        }
        else if (!IsInCurrentRange(npc, destPos, attackAIProperties.lostDistance))
        {
            //Transition to Hiding state (it is mapped to the lost player transition).
            npc.GetComponent<MonsterControllerAI>().PerformTransition(Transition.LostPlayer);
        }
    }

    //
    //Act - perform any actions required
    //
    public override void Act(Transform player, Transform npc)
    {
        destPos = player.position;
        Quaternion newRot = Quaternion.LookRotation(destPos - npc.position);
        npc.rotation = Quaternion.RotateTowards(npc.rotation, newRot, Time.deltaTime * curRotSpeed);
        if (moveCloser)
        {
            Vector3 newPos = Vector3.MoveTowards(npc.position, destPos, Time.deltaTime * curSpeed);
            npc.position = newPos;
        }

        //Attack code goes here -- create and shoot projectile in direction of player, perform melee attacks etc...
    }



}
