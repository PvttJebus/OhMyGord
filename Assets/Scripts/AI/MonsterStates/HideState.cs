using UnityEngine;
using System.Collections;
//
//This is the HideState class. 
//It handles the decisions and actions for the AI character when it is in the hide state.
//It also handles all the possible the transtions OUT of the hide state
//
public class HideState : FSMState
{
    private HideAIProperties hideAIProperties;
    bool moving;

    //Constructor
    public HideState(HideAIProperties hideAIProperties, Transform trans)
    {
        this.hideAIProperties = hideAIProperties;
        waypoints = hideAIProperties.hideLocations;
        stateID = FSMStateID.Hiding;
        curSpeed = hideAIProperties.speed;
        curRotSpeed = hideAIProperties.rotSpeed;
        destPos = GetClosestWaypoint(trans).position;
        moving = true;
    }

    //
    //Reason - make any decisions required
    //
    public override void Reason(Transform player, Transform npc)
    {
        moving = true;
        destPos = GetClosestWaypoint(npc).position;

        //Check if NPC has died
        if (npc.GetComponent<MonsterControllerAI>().Health == 0)
        {
            npc.GetComponent<MonsterControllerAI>().PerformTransition(Transition.NoHealth);
            return;
        }
        //NOTE: ORDER OF IF STATEMENTS IS IMPORTANT

        //If we are in chasing distance of the player, then go to ATTACK state
        if (IsInCurrentRange(npc, player.position, hideAIProperties.chaseDistance) && (npc.GetComponent<MonsterControllerAI>().Health > 50)) //NOTE can add a health check here to ensure you only come out of hiding if your health is high
        {
            npc.GetComponent<MonsterControllerAI>().PerformTransition(Transition.SawPlayer);
        }
        //check to see if we reached our hiding spot
        else if (IsInCurrentRange(npc, destPos, 0))
        {
            //do nothing, we have reached our hiding spot
            moving = false;
        }
    }

    //
    //Act - perform any actions required
    //
    public override void Act(Transform player, Transform npc)
    {


        if (moving)
        {
            Quaternion newRot = Quaternion.LookRotation(destPos - npc.position);
            npc.rotation = Quaternion.RotateTowards(npc.rotation, newRot, Time.deltaTime * curRotSpeed);

            Vector3 newPos = Vector3.MoveTowards(npc.position, destPos, Time.deltaTime * curSpeed);
            npc.position = newPos;
        }

        if (hideAIProperties.healthRegenRate > 0)
        {
            npc.GetComponent<MonsterControllerAI>().AddHealth(hideAIProperties.healthRegenRate * Time.deltaTime);
        }

    }
}
