using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask detectionLayers;
    
    [Header("Patrol Settings")]
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float patrolSpeed = 3.5f;
    
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float losePlayerTime = 3f; // Time before returning to patrol after losing sight
    
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 10;
    
    private NavMeshAgent agent;
    private Transform player;
    private List<GameObject> patrolNodes = new List<GameObject>();
    private int currentPatrolIndex = 0;
    private float lastAttackTime;
    private float lostPlayerTimer;
    
    private enum AIState
    {
        Patrolling,
        Chasing,
        Attacking
    }
    
    private AIState currentState = AIState.Patrolling;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No GameObject with 'Player' tag found!");
        }
        
        // Get patrol nodes from the maze generator
        ProceduralMaze maze = FindObjectOfType<ProceduralMaze>();
        if (maze != null)
        {
            patrolNodes = maze.GetPatrolNodes();
            if (patrolNodes.Count > 0)
            {
                // Start patrolling to nearest node
                FindNearestPatrolNode();
                StartCoroutine(PatrolRoutine());
            }
        }
        
        agent.speed = patrolSpeed;
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case AIState.Patrolling:
                // Check if player is in detection range
                if (distanceToPlayer <= detectionRange && CanSeePlayer())
                {
                    EnterChaseState();
                }
                break;
                
            case AIState.Chasing:
                // Check if player is in attack range
                if (distanceToPlayer <= attackRange)
                {
                    EnterAttackState();
                }
                // Check if lost sight of player
                else if (!CanSeePlayer() || distanceToPlayer > detectionRange * 1.5f)
                {
                    lostPlayerTimer += Time.deltaTime;
                    if (lostPlayerTimer >= losePlayerTime)
                    {
                        EnterPatrolState();
                    }
                }
                else
                {
                    lostPlayerTimer = 0f;
                    agent.SetDestination(player.position);
                }
                break;
                
            case AIState.Attacking:
                // Look at player
                Vector3 lookDirection = (player.position - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
                
                // Attack if cooldown is ready
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }
                
                // Check if player moved out of attack range
                if (distanceToPlayer > attackRange)
                {
                    if (distanceToPlayer <= detectionRange)
                    {
                        EnterChaseState();
                    }
                    else
                    {
                        EnterPatrolState();
                    }
                }
                break;
        }
    }
    
    bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Raycast to check for obstacles
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, distance))
        {
            return hit.transform.CompareTag("Player");
        }
        
        return false;
    }
    
    void EnterPatrolState()
    {
        currentState = AIState.Patrolling;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        lostPlayerTimer = 0f;
        
        if (patrolNodes.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(PatrolRoutine());
        }
    }
    
    void EnterChaseState()
    {
        currentState = AIState.Chasing;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        lostPlayerTimer = 0f;
        StopAllCoroutines();
    }
    
    void EnterAttackState()
    {
        currentState = AIState.Attacking;
        agent.isStopped = true;
    }
    
    void Attack()
    {
        Debug.Log($"{gameObject.name} attacks player for {attackDamage} damage!");
        
        // Try to find a health component on the player
        if (player != null)
        {
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }
    
    IEnumerator PatrolRoutine()
    {
        while (currentState == AIState.Patrolling && patrolNodes.Count > 0)
        {
            // Move to current patrol node
            GameObject targetNode = patrolNodes[currentPatrolIndex];
            if (targetNode != null)
            {
                agent.SetDestination(targetNode.transform.position);
                
                // Wait until we reach the node
                while (Vector3.Distance(transform.position, targetNode.transform.position) > agent.stoppingDistance + 0.5f)
                {
                    if (currentState != AIState.Patrolling) yield break;
                    yield return null;
                }
                
                // Wait at the node
                yield return new WaitForSeconds(patrolWaitTime);
                
                // Move to next node
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolNodes.Count;
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    
    void FindNearestPatrolNode()
    {
        if (patrolNodes.Count == 0) return;
        
        float minDistance = float.MaxValue;
        int nearestIndex = 0;
        
        for (int i = 0; i < patrolNodes.Count; i++)
        {
            if (patrolNodes[i] != null)
            {
                float distance = Vector3.Distance(transform.position, patrolNodes[i].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }
        }
        
        currentPatrolIndex = nearestIndex;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
