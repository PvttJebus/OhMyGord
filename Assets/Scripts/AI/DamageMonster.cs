using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageMonster : MonoBehaviour
{
    [SerializeField]
    private float attackTime = 0.3f;

    [SerializeField]
    private int attackPower = 25;

    private void OnEnable()
    {
        StartCoroutine(StartCoolDown());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnTriggerEnter(Collider other)
    {
        MonsterControllerAI monster = other.GetComponent<MonsterControllerAI>();
        if (monster)
        {
            monster.DecHealth(attackPower);
        }
    }

    private IEnumerator StartCoolDown()
    {
        yield return new WaitForSeconds(attackTime);

        gameObject.SetActive(false);
    }
}
