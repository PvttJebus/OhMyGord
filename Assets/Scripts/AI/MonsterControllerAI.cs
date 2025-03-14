using UnityEngine;
using UnityEngine.UI;
using System.Collections;


[System.Serializable]
public abstract class AIProperties
{
    public float speed = 3.0f;
    public float rotSpeed = 2.0f;
    public float chaseDistance = 20;
    public float healthRegenRate = 0;
}

[System.Serializable]
public class AttackAIProperties : AIProperties
{
    public float attackDistance = 2;
    public float lostDistance = 45;
}

[System.Serializable]
public class HideAIProperties : AIProperties
{
    public Transform[] hideLocations;
}

[System.Serializable]
public class FleeAIProperties : AIProperties
{
    public float lostDistance = 45;
    public Transform[] fleeLocations;
}

public class MonsterControllerAI : AdvancedFSM
{
    [SerializeField]
    private bool debugDraw;
    [SerializeField]
    private Text StateText;
    [SerializeField]
    private Text HealthText;

    [SerializeField]
    private AttackAIProperties attackAIProperties;

    [SerializeField]
    private HideAIProperties hideAIProperties;

    [SerializeField]
    private FleeAIProperties fleeAIProperties;

    [SerializeField]
    private GameObject deathGO;
    public GameObject DeathGO
    {
        get { return deathGO; }
    }



    private float health;
    public float Health
    {
        get { return health; }
    }
    public void DecHealth(float amount) { health = Mathf.Max(0, health - amount); }
    public void AddHealth(float amount) { health = Mathf.Min(100, health + amount); }

    private string GetStateString()
    {

        string state = "NONE";
        if (CurrentState.ID == FSMStateID.Dead)
        {
            state = "DEAD";
        }
        else if (CurrentState.ID == FSMStateID.Attacking)
        {
            state = "ATTACK";
        }
        else if (CurrentState.ID == FSMStateID.Fleeing)
        {
            state = "FLEE";
        }
        else if (CurrentState.ID == FSMStateID.Hiding)
        {
            state = "HIDE";
        }

        return state;
    }

    protected override void Initialize()
    {
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;
        health = 100;
        ConstructFSM();
    }

    protected override void FSMUpdate()
    {
        elapsedTime += Time.deltaTime;

        if (CurrentState != null)
        {
            CurrentState.Reason(playerTransform, transform);
            CurrentState.Act(playerTransform, transform);
        }
        StateText.text = "MONSTER STATE IS: " + GetStateString();
        HealthText.text = "MONSTER HEALTH IS: " + Health;

        if (debugDraw)
        {
            Debug.DrawRay(transform.position, transform.forward * 5.0f, Color.red);
        }
    }


    private void ConstructFSM()
    {
        pointList = GameObject.FindGameObjectsWithTag("WayPoint");
        //
        //Creating a waypoint transform array for each state
        //
        Transform[] waypoints = new Transform[pointList.Length];

        for (int i = 0; i < pointList.Length; i++)
        {
            waypoints[i] = pointList[i].transform;
        }


        //
        //Create States
        //
        //Create Hide state
        HideState hideState = new HideState(hideAIProperties, transform);
        //add transitions OUT of the hide state
        hideState.AddTransition(Transition.SawPlayer, FSMStateID.Attacking);
        hideState.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        //Create 

        AttackState attackState = new AttackState(attackAIProperties);
        //add transitions OUT of the attack state
        attackState.AddTransition(Transition.LostPlayer, FSMStateID.Hiding);
        attackState.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
        attackState.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        //Create Flee state
        FleeState fleeState = new FleeState(fleeAIProperties);
        //add transitions OUT of the flee state
        fleeState.AddTransition(Transition.LostPlayer, FSMStateID.Hiding);
        fleeState.AddTransition(Transition.SawPlayer, FSMStateID.Attacking);
        fleeState.AddTransition(Transition.NoHealth, FSMStateID.Dead);


        //Create the Dead state
        DeadState deadState = new DeadState();
        //there are no transitions out of the dead state


        //Add all states to the state list
        AddFSMState(hideState);
        AddFSMState(attackState);
        AddFSMState(fleeState);
        AddFSMState(deadState);
    }

    public void StartDeath()
    {
        StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        Renderer r = GetComponent<Renderer>();
        r.enabled = false;

        deathGO.SetActive(true);

        yield return new WaitForSeconds(2.2f);

        Destroy(gameObject);
    }


}