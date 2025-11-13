using UnityEngine;
using StarterAssets;

public class FSMController_YN : MonoBehaviour
{
    public GameObject goPlayer;
   
    public Transform[] patrolWaypoints;
    public float waypointReachDistance = 0.5f;
    public float patrolSpeed = 3f;
    [Range(0f, 1f)]
    public float chanceToStopPatrolling = 0.01f;
    
    [Header("Wander Settings")]
    public float wanderSpeed = 2f;
    public float minWanderTime = 3f;
    public float maxWanderTime = 8f;
    public float wanderDirectionChangeInterval = 2f;

    private Renderer enemyRenderer;
    private Color originalColor;
    private int currentWaypointIndex = 0;
    private ThirdPersonController playerController;
  
    private float wanderTimer = 0f;
    private float wanderDuration = 0f;
    private Vector3 wanderDirection;
    private float directionChangeTimer = 0f;

    public enum State
    {
        Wander,
        Chase,
        Evade,
        Patrol
    }


    public State CurrentState { get; private set; } = State.Wander;


    void Start()
    {

        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }

        // Get reference to ThirdPersonController component
        if (goPlayer != null)
        {
            playerController = goPlayer.GetComponent<ThirdPersonController>();
            if (playerController == null)
            {
                Debug.LogWarning("ThirdPersonController component not found on goPlayer!");
            }
        }

        SetState(State.Wander);
    }

    // Update is called once per frame
    void Update()
    {
        switch (CurrentState)
        {
            case State.Wander:
                HandleWander();
                break;
            case State.Chase:
                HandleChase();
                break;
            case State.Evade:
                HandleEvade();
                break;
            case State.Patrol:
                HandlePatrol();
                break;
        }
    }


    public void SetState(State newState)
    {
        if (CurrentState == newState) return;


        if ((CurrentState == State.Chase || CurrentState == State.Evade || CurrentState == State.Patrol || CurrentState == State.Wander) && enemyRenderer != null)
        {
            enemyRenderer.material.color = originalColor;
        }

        CurrentState = newState;

        if (CurrentState == State.Chase && enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
        }

        if (CurrentState == State.Evade && enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.yellow;
        }

        if (CurrentState == State.Patrol && enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.magenta;
        }
        
        if (CurrentState == State.Wander && enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.green;
            
            wanderTimer = 0f;
            wanderDuration = Random.Range(minWanderTime, maxWanderTime);
            directionChangeTimer = 0f;
            wanderDirection = GetRandomDirection();
        }
    }

    void HandleChase()
    {
        if (goPlayer == null) return;


        if (ShouldEvade())
        {
            SetState(State.Evade);
            return;
        }

        if (IsPlayerDetected())
        {
            Vector3 direction = (goPlayer.transform.position - transform.position).normalized;
            transform.position += direction * Time.deltaTime * 5f;

            transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            SetState(State.Wander);
        }
    }

    bool IsPlayerDetected()
    {
        if (goPlayer == null) return false;

        Vector3 directionToPlayer = goPlayer.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > 7f) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > 90f) return false;


        return true;
    }

    bool ShouldEvade()
    {
        if (goPlayer == null || playerController == null) return false;

        // Read HasPowerUp from ThirdPersonController
        bool playerHasPowerUp = playerController.HasPowerUp;

        if (!playerHasPowerUp) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, goPlayer.transform.position);

        return distanceToPlayer < 4f;
    }

    void HandleEvade()
    {
        if (goPlayer == null) return;


        if (ShouldEvade())
        {

            Vector3 directionAwayFromPlayer = (transform.position - goPlayer.transform.position).normalized;
            transform.position += directionAwayFromPlayer * Time.deltaTime * 6f;

            transform.rotation = Quaternion.LookRotation(directionAwayFromPlayer);
        }
        else
        {

            SetState(State.Wander);
        }
    }

    void HandlePatrol()
    {
        
        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
        {
            SetState(State.Wander);
            return;
        }

       
        if (ShouldEvade())
        {
            SetState(State.Evade);
            return;
        }

       
        if (IsPlayerDetected())
        {
            SetState(State.Chase);
            return;
        }

       
        if (Random.value < chanceToStopPatrolling)
        {
            SetState(State.Wander);
            return;
        }

        
        Transform targetWaypoint = patrolWaypoints[currentWaypointIndex];

        if (targetWaypoint == null)
        {
            
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            return;
        }

        
        Vector3 directionToWaypoint = targetWaypoint.position - transform.position;
        float distanceToWaypoint = directionToWaypoint.magnitude;

        
        if (distanceToWaypoint <= waypointReachDistance)
        {
          
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
        }
        else
        {
          
            Vector3 direction = directionToWaypoint.normalized;
            transform.position += direction * patrolSpeed * Time.deltaTime;

            
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void HandleWander()
    {
        if (ShouldEvade())
        {
            SetState(State.Evade);
            return;
        }

      
        if (IsPlayerDetected())
        {
            SetState(State.Chase);
            return;
        }

      
        wanderTimer += Time.deltaTime;

        
        if (wanderTimer >= wanderDuration)
        {
            
            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                SetState(State.Patrol);
            }
            else
            {
              
                wanderTimer = 0f;
                wanderDuration = Random.Range(minWanderTime, maxWanderTime);
            }
            return;
        }

        
        directionChangeTimer += Time.deltaTime;

        
        if (directionChangeTimer >= wanderDirectionChangeInterval)
        {
            wanderDirection = GetRandomDirection();
            directionChangeTimer = 0f;
        }

        
        transform.position += wanderDirection * wanderSpeed * Time.deltaTime;

       
        if (wanderDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(wanderDirection);
        }
    }

    Vector3 GetRandomDirection()
    {
       
        float randomAngle = Random.Range(0f, 360f);
        float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad);
        float z = Mathf.Sin(randomAngle * Mathf.Deg2Rad);
        
        return new Vector3(x, 0f, z).normalized;
    }
}