using UnityEngine;
using UnityEngine.SceneManagement;
using static NPCStateMachine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCStateMachine : MonoBehaviour
{
    public enum NPCState
    {
        Idle,
        Patrol,
        MoveTowardsPlayer
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private Transform sightVisualRoot;
    [SerializeField] private NPCStateUI stateUI;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float reachDistance = 0.1f;

    [Header("Vision")]
    [SerializeField] private float sightDistance = 5f;
    [SerializeField] private float sightAngle = 60f;

    [Header("Decision Timing")]
    [SerializeField] private float stateDecisionTime = 3f;

    [Header("Start Setup")]
    [SerializeField] private NPCState startingState = NPCState.Idle;
    [SerializeField] private Vector2 initialFacingDirection = Vector2.up;

    [Header("Scenes")]
    [SerializeField] private string victorySceneName = "VictoryScene";
    [SerializeField] private string defeatSceneName = "DefeatScene";

    [Header("Audio")]
    [SerializeField] private AudioClip stateChangeSFX;
    [SerializeField] private AudioClip collisionSFX;

    private Rigidbody2D rb;
    private NPCState currentState;

    private float timer;
    private int failedStayAttempts = 0;

    private Transform currentPatrolTarget;
    private bool patrolArrived;
    private bool gameEnded = false;

    private Vector2 facingDirection;

    public NPCState CurrentState => currentState;
    public float Timer => timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        facingDirection = initialFacingDirection.normalized;
    }

    private void Start()
    {
        currentPatrolTarget = patrolPointA;
        SetFacingDirection(facingDirection);
        EnterState(startingState);
    }

    private void Update()
    {
        if (gameEnded || player == null)
            return;

        if (CanSeePlayer())
        {
            if (currentState != NPCState.MoveTowardsPlayer)
            {
                EnterState(NPCState.MoveTowardsPlayer);
            }
        }

        HandleCurrentState();
        UpdateUI();
        UpdateSightVisual();
    }

    private void FixedUpdate()
    {
        if (gameEnded) return;

        if (currentState == NPCState.Patrol)
        {
            if (!patrolArrived && currentPatrolTarget != null)
            {
                MoveTowards(currentPatrolTarget.position);
            }
        }
        else if (currentState == NPCState.MoveTowardsPlayer && player != null)
        {
            MoveTowards(player.position);
        }
    }

    private void HandleCurrentState()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                HandleIdle();
                break;

            case NPCState.Patrol:
                HandlePatrol();
                break;

            case NPCState.MoveTowardsPlayer:
                HandleMoveTowardsPlayer();
                break;
        }
    }

    private void HandleIdle()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            DecideIdleOrPatrol(fromState: NPCState.Idle);
        }
    }

    private void HandlePatrol()
    {
        if (!patrolArrived && currentPatrolTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentPatrolTarget.position);

            if (distance <= reachDistance)
            {
                rb.MovePosition(currentPatrolTarget.position);
                patrolArrived = true;
                timer = stateDecisionTime;
            }
        }
        else
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                DecideIdleOrPatrol(fromState: NPCState.Patrol);
            }
        }
    }

    private void HandleMoveTowardsPlayer()
    {
        timer = 0f;
    }

    private void DecideIdleOrPatrol(NPCState fromState)
    {
        NPCState otherState = (fromState == NPCState.Idle) ? NPCState.Patrol : NPCState.Idle;

        // Failsafe: force switch on the 3rd decision
        if (failedStayAttempts >= 2)
        {
            EnterState(otherState);
            return;
        }

        bool stayInCurrentState = Random.value < 0.5f;

        if (stayInCurrentState)
        {
            failedStayAttempts++;

            if (fromState == NPCState.Idle)
            {
                timer = stateDecisionTime;
            }
            else if (fromState == NPCState.Patrol)
            {
                SetNextPatrolPoint();
                patrolArrived = false;
            }
        }
        else
        {
            EnterState(otherState);
        }
    }

    private void EnterState(NPCState newState)
    {
        currentState = newState;
        failedStayAttempts = 0;

        switch (currentState)
        {
            case NPCState.Idle:
                timer = stateDecisionTime;
                break;

            case NPCState.Patrol:
                patrolArrived = false;
                if (currentPatrolTarget == null)
                    currentPatrolTarget = patrolPointA;
                break;

            case NPCState.MoveTowardsPlayer:
                timer = 0f;
                break;
        }

        if (AudioManager.Instance != null && stateChangeSFX != null)
        {
            AudioManager.Instance.PlaySFX(stateChangeSFX);
        }

        UpdateUI();
    }

    private void MoveTowards(Vector2 targetPosition)
    {
        Vector2 currentPos = rb.position;
        Vector2 direction = (targetPosition - currentPos).normalized;

        if (direction.sqrMagnitude > 0.001f)
        {
            SetFacingDirection(direction);
        }

        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    private void SetFacingDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude <= 0.001f) return;

        facingDirection = dir.normalized;

        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void SetNextPatrolPoint()
    {
        if (currentPatrolTarget == patrolPointA)
            currentPatrolTarget = patrolPointB;
        else
            currentPatrolTarget = patrolPointA;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 origin = transform.position;
        Vector2 toPlayer = (player.position - transform.position);
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > sightDistance)
            return false;

        float angleToPlayer = Vector2.Angle(facingDirection, toPlayer.normalized);
        if (angleToPlayer > sightAngle * 0.5f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer.normalized, distanceToPlayer, obstacleMask | playerMask);

        if (hit.collider == null)
            return false;

        return hit.collider.CompareTag("Player");
    }

    private void UpdateSightVisual()
    {
        if (sightVisualRoot == null) return;

        sightVisualRoot.localRotation = Quaternion.identity;
    }

    private void UpdateUI()
    {
        if (stateUI == null) return;

        string transitions = "";

        switch (currentState)
        {
            case NPCState.Idle:
                transitions =
                    "Player seen -> Move Towards Player\n" +
                    "Timer ends -> Randomly stay Idle or switch to Patrol\n" +
                    "Failsafe -> Force switch on 3rd decision";
                break;

            case NPCState.Patrol:
                if (!patrolArrived)
                {
                    transitions =
                        "Moving to patrol point\n" +
                        "Player seen -> Move Towards Player\n" +
                        "Decision happens only after patrol point is reached";
                }
                else
                {
                    transitions =
                        "Player seen -> Move Towards Player\n" +
                        "Timer ends -> Randomly stay Patrol or switch to Idle\n" +
                        "Failsafe -> Force switch on 3rd decision";
                }
                break;

            case NPCState.MoveTowardsPlayer:
                transitions =
                    "Touch player -> Defeat\n" +
                    "Player touched NPC in Idle/Patrol -> Victory";
                break;
        }

        stateUI.UpdateUI(currentState.ToString(), timer, transitions);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameEnded) return;

        if (!other.CompareTag("Player"))
            return;

        gameEnded = true;

        if (AudioManager.Instance != null && collisionSFX != null)
        {
            AudioManager.Instance.PlaySFX(collisionSFX);
        }

        if (currentState == NPCState.MoveTowardsPlayer)
        {
            SceneManager.LoadScene(defeatSceneName);
        }
        else
        {
            SceneManager.LoadScene(victorySceneName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightDistance);

        Vector3 leftDir = Quaternion.Euler(0, 0, sightAngle * 0.5f) * (Application.isPlaying ? (Vector3)facingDirection : transform.up);
        Vector3 rightDir = Quaternion.Euler(0, 0, -sightAngle * 0.5f) * (Application.isPlaying ? (Vector3)facingDirection : transform.up);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * sightDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * sightDistance);
    }
}