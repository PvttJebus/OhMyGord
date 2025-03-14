using UnityEngine;
using System.Collections;

public class DeadState : FSMState
{
    private bool deathStarted = false;

    //Constructor
    public DeadState()
    {
        stateID = FSMStateID.Dead;
        curSpeed = 0.0f;
        curRotSpeed = 0.0f;
    }

    //Reason
    public override void Reason(Transform player, Transform npc)
    {
    }

    //Act
    public override void Act(Transform player, Transform npc)
    {
        if (!deathStarted)
        {
            MonsterControllerAI monster = npc.GetComponent<MonsterControllerAI>();
            if (monster)
            {
                monster.StartDeath();
            }
        }

    }
}
