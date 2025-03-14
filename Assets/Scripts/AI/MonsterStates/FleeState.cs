using UnityEngine;
using System.Collections;
//
//This is the FleeState class. 
//It handles the decisions and actions for the AI character when it is in the flee state.
//It also handles all the possible the transtions OUT of the flee state
//
public class FleeState : FSMState
{
    private FleeAIProperties fleeAIproperties;

    public FleeState(FleeAIProperties fleeAIproperties)
    {
        stateID = FSMStateID.Fleeing;
        this.fleeAIproperties = fleeAIproperties;
        waypoints = fleeAIproperties.fleeLocations;
        curSpeed = fleeAIproperties.speed;
        curRotSpeed = fleeAIproperties.rotSpeed;
    }

    //
    //EnterStateInit - If you need to initialize variables before you enter the state do it in this function
    //
    public override void EnterStateInit(Transform player, Transform npc)
    {
        destPos = GetFurthestWayPoint(npc).position;
    }

    //
    //Reason - make any decisions required
    //
    public override void Reason(Transform player, Transform npc)
    {
        //Check if NPC has died
        if (npc.GetComponent<MonsterControllerAI>().Health == 0)
        {
            npc.GetComponent<MonsterControllerAI>().PerformTransition(Transition.NoHealth);
            return;
        }

        Vector3 playerPos = player.position;

        //check to see if we are near the player position and we are in good health, go to ATTACK state
        if (IsInCurrentRange(npc, playerPos, fleeAIproperties.chaseDistance) && (npc.GetComponent<MonsterControllerAI>().Health > 50))
        {
            Debug.Log("Switch to the Attack state");
            npc.GetComponent<MonsterControllerAI>().PerformTransition(Transition.SawPlayer);
        }
        //eles if there are no enmies around go to HIDE state
        else if (!IsInCurrentRange(npc, playerPos, fleeAIproperties.lostDistance)) // if we are farther than Lost Distance
        {
            Debug.Log("Switch to the Hide state");
            //lost the player so go to hiding state (it is mapped to Lost player transition)
            npc.GetComponent<MonsterControllerAI>().PerformTransition(Transition.LostPlayer);
        }
        else
        {
            //enemy is in sight but our health is low so continue to flee OR the player is not in sight so continue to flee
            //
            //check to see if we have reached our last destination point. If we have then get the next one (so we don't stand still)
            if (IsInCurrentRange(npc, destPos, 0))
            {
                destPos = GetFurthestWayPoint(npc).position;
            }
        }
    }


    //
    //Act - perform any actions required
    //
    public override void Act(Transform player, Transform npc)
    {
        //if we are fleeing we are going to the current flee position
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //go forward
        Vector3 newPos = Vector3.MoveTowards(npc.position, destPos, Time.deltaTime * curSpeed);
        npc.position = newPos;


        if (fleeAIproperties.healthRegenRate > 0)
        {
            npc.GetComponent<MonsterControllerAI>().AddHealth(fleeAIproperties.healthRegenRate * Time.deltaTime);
        }
    }

}
