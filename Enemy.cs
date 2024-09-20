using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private List<AudioClip> attackClips;

    public Survival _survival;
    public AudioSource audioSource;
    public AudioClip run;
    public AudioClip attack1;
    public AudioClip attack2;
    public Transform player;

    // Attacking
    private float timeBetweenAttacks = 5f;
    private bool alreadyAttacked;

    // States
    private float sightRange = 75f, attackRange = 25f;
    public bool playerInSightRange, playerInAttackRange;

    // Start is called before the first frame update
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        attackClips = new()
        {
            attack1,
            attack2
        };
    }

    // Update is called once per frame
    private void Update()
    {
        playerInSightRange = Vector3.Distance(transform.position, player.position) <= sightRange;
        playerInAttackRange = Vector3.Distance(transform.position, player.position) <= attackRange;

        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        transform.LookAt(player);
        if (!alreadyAttacked)
        {
            audioSource.loop = false;
            audioSource.Stop();
            audioSource.clip = attackClips[Random.Range(0, 2)];
            audioSource.Play();
            StartCoroutine(WaitForSound());

            if (Random.Range(0f, 100f) <= 50f)
                _survival.LowerHealth(15f);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    IEnumerator WaitForSound()
    {
        while (audioSource.isPlaying)
            yield return null;

        audioSource.clip = run;
        audioSource.loop = true;
        audioSource.Play();
    }
}
