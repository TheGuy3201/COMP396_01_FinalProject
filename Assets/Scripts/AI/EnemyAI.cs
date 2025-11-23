using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 15f; // Range within which the enemy can detect the player
    [SerializeField] private float attackRange = 4f; // Range within which the enemy can attack the player

    [Header("Patrol Settings")]
    [SerializeField] private float patrolWaitTime = 2f; // Time to wait at each patrol node
    [SerializeField] private float patrolSpeed = 3.5f; // Speed while patrolling
    
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 6f; // Speed while chasing the player
    [SerializeField] private float losePlayerTime = 3f; // Time before returning to patrol after losing sight
    
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.5f; // Time between attacks
    [SerializeField] private int attackDamage = 10; // Damage dealt per attack
    
    private NavMeshAgent agent;
    private Transform player;
    private List<GameObject> patrolNodes = new List<GameObject>();
    private int currentPatrolIndex = 0;
    private float lastAttackTime;
    private float lostPlayerTimer;
    
    private enum AIState // FSM state of the AI
    {
        Patrolling,
        Chasing,
        Attacking
    }
    
    private AIState currentState = AIState.Patrolling; // Initial state of the AI
    
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
        
        // Get patrol nodes from the maze generator script
        ProceduralMaze maze = FindObjectOfType<ProceduralMaze>();
        if (maze != null)
        {
            patrolNodes = maze.GetPatrolNodes(); //Calls a method to get all patrol Nodes
            if (patrolNodes.Count > 0)
            {
                // Start patrolling to nearest node
                FindNearestPatrolNode();
                StartCoroutine(PatrolRoutine());
            }
        }
        
        agent.speed = patrolSpeed; //Changes speed to patrol speed (much faster!)
    }
    
    void Update()
    {
        // Check if player reference is properly assigned
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Calculate distance to player
        
        // State machine logic in a switch statement
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
                    // Reset lost player timer if player is visible
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
    
    bool CanSeePlayer() // Simple line-of-sight check
    {
        if (player == null) return false; //Again, check if player reference is valid
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Raycast to check for obstacles or if the player is visible
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, distance))
        {
            return hit.transform.CompareTag("Player"); // Returns true if the raycast hits the player
        }
        
        return false; // Default to false if nothing is hit
    }
    
    void EnterPatrolState() // Switch to patrol state
    {
        currentState = AIState.Patrolling;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        lostPlayerTimer = 0f;
        
        if (patrolNodes.Count > 0)
        {
            //Stops previous state coroutine and starts patrolling state coroutine
            StopAllCoroutines();
            StartCoroutine(PatrolRoutine());
        }
    }
    
    void EnterChaseState() // Switch to chase state
    {
        currentState = AIState.Chasing;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        lostPlayerTimer = 0f;
        StopAllCoroutines();
    }
    
    void EnterAttackState() // Switch to attack state
    {
        currentState = AIState.Attacking;
        agent.isStopped = true;
    }
    
    void Attack() // Actual attack logic
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
    
    IEnumerator PatrolRoutine() // Coroutine for patrolling behavior
    {
        while (currentState == AIState.Patrolling && patrolNodes.Count > 0)
        {
            // Move to current patrol node
            GameObject targetNode = patrolNodes[currentPatrolIndex];
            if (targetNode != null)
            {
                // Move towards the patrol node
                agent.SetDestination(targetNode.transform.position);
                
                // Wait until it reaches the node
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
    
    void FindNearestPatrolNode() // Finds the nearest patrol node to start patrolling from
    {
        if (patrolNodes.Count == 0) return; //Another safety check
        
        float minDistance = float.MaxValue;
        int nearestIndex = 0;
        
        for (int i = 0; i < patrolNodes.Count; i++)
        {
            if (patrolNodes[i] != null)
            {
                float distance = Vector3.Distance(transform.position, patrolNodes[i].transform.position); // Calculate distance to patrol node
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }
        }
        
        currentPatrolIndex = nearestIndex; // Set the current patrol index to the nearest node
    }
    
    private void OnDrawGizmosSelected() // Visualize detection and attack ranges in the editor
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
