using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private NPCStateUI stateUI;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.1f;

    [Header("Vision")]
    [SerializeField] private float sightDistance = 5f;
    [SerializeField] private float sightAngle = 60f;
    [SerializeField] private Vector2 initialFacingDirection = Vector2.up;

    [Header("Decision Timing")]
    [SerializeField] private float stateDecisionTime = 3f;

    [Header("State Setup")]
    [SerializeField] private NPCState startingState = NPCState.Idle;

    [Header("Scenes")]
    [SerializeField] private string victorySceneName = "Victory";
    [SerializeField] private string defeatSceneName = "Defeat";

    [Header("Audio")]
    [SerializeField] private AudioClip stateChangeSFX;
    [SerializeField] private AudioClip collisionSFX;

    private Rigidbody2D rb;
    private NPCState currentState;

    private float timer;
    private int sameStateCount;
    private bool gameEnded;

    private Transform currentPatrolTarget;
    private Vector2 facingDirection = Vector2.up;

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

        if (CanSeePlayer() && currentState != NPCState.MoveTowardsPlayer)
        {
            EnterState(NPCState.MoveTowardsPlayer);
        }

        HandleCurrentState();
        UpdateUI();
    }

    private void FixedUpdate()
    {
        if (gameEnded)
            return;

        if (currentState == NPCState.Patrol)
        {
            PatrolMove();
        }
        else if (currentState == NPCState.MoveTowardsPlayer)
        {
            ChasePlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
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
                HandleChase();
                break;
        }
    }

    private void HandleIdle()
    {
        rb.linearVelocity = Vector2.zero;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            DecideIdleOrPatrol(NPCState.Idle);
        }
    }

    private void HandlePatrol()
    {
        if (currentPatrolTarget == null)
            return;

        float distance = Vector2.Distance(transform.position, currentPatrolTarget.position);

        if (distance <= stoppingDistance)
        {
            rb.position = currentPatrolTarget.position;
            rb.linearVelocity = Vector2.zero;

            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                DecideIdleOrPatrol(NPCState.Patrol);
            }
        }
    }

    private void HandleChase()
    {
        timer = 0f;
    }

    private void PatrolMove()
    {
        if (currentPatrolTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector2)currentPatrolTarget.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        if (direction.sqrMagnitude > 0.001f)
        {
            SetFacingDirection(direction);
        }
    }

    private void ChasePlayer()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        if (direction.sqrMagnitude > 0.001f)
        {
            SetFacingDirection(direction);
        }
    }

    private void DecideIdleOrPatrol(NPCState fromState)
    {
        NPCState otherState = fromState == NPCState.Idle ? NPCState.Patrol : NPCState.Idle;

        bool stayInSameState = Random.value < 0.5f;

        if (sameStateCount >= 2)
        {
            EnterState(otherState);
            return;
        }

        if (stayInSameState)
        {
            sameStateCount++;

            if (fromState == NPCState.Idle)
            {
                timer = stateDecisionTime;
            }
            else
            {
                SetNextPatrolTarget();
                timer = stateDecisionTime;
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
        sameStateCount = 0;

        switch (currentState)
        {
            case NPCState.Idle:
                timer = stateDecisionTime;
                rb.linearVelocity = Vector2.zero;
                break;

            case NPCState.Patrol:
                if (currentPatrolTarget == null)
                    currentPatrolTarget = patrolPointA;

                timer = stateDecisionTime;
                break;

            case NPCState.MoveTowardsPlayer:
                timer = 0f;
                break;
        }

        if (AudioManager.Instance != null && stateChangeSFX != null)
        {
            AudioManager.Instance.PlaySFX(stateChangeSFX);
        }
    }

    private void SetNextPatrolTarget()
    {
        if (currentPatrolTarget == patrolPointA)
            currentPatrolTarget = patrolPointB;
        else
            currentPatrolTarget = patrolPointA;
    }

    private void SetFacingDirection(Vector2 direction)
    {
        facingDirection = direction.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector2 origin = transform.position;
        Vector2 toPlayer = (Vector2)(player.position - transform.position);

        if (toPlayer.magnitude > sightDistance)
            return false;

        float angleToPlayer = Vector2.Angle(facingDirection, toPlayer.normalized);
        if (angleToPlayer > sightAngle * 0.5f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            toPlayer.normalized,
            toPlayer.magnitude,
            obstacleMask | playerMask
        );

        if (hit.collider == null)
            return false;

        return hit.collider.CompareTag("Player");
    }

    private void UpdateUI()
    {
        if (stateUI == null)
            return;

        string transitions = "";

        switch (currentState)
        {
            case NPCState.Idle:
                transitions =
                    "Player seen -> Move Towards Player\n" +
                    "Timer ends -> Random Idle/Patrol\n" +
                    "Failsafe -> Forced switch on 3rd decision";
                break;

            case NPCState.Patrol:
                transitions =
                    "Move between point A and point B\n" +
                    "Player seen -> Move Towards Player\n" +
                    "Decision after reaching patrol point";
                break;

            case NPCState.MoveTowardsPlayer:
                transitions =
                    "Touch player -> Defeat\n" +
                    "If player touches NPC during Idle/Patrol -> Victory";
                break;
        }

        stateUI.UpdateUI(currentState.ToString(), timer, transitions);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameEnded)
            return;

        if (!other.CompareTag("Player"))
            return;

        gameEnded = true;

        if (AudioManager.Instance != null && collisionSFX != null)
        {
            AudioManager.Instance.PlaySFX(collisionSFX);
        }

        if (currentState == NPCState.MoveTowardsPlayer)
            SceneManager.LoadScene(defeatSceneName);
        else
            SceneManager.LoadScene(victorySceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 drawDir = Application.isPlaying ? (Vector3)facingDirection : transform.up;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightDistance);

        Vector3 leftDir = Quaternion.Euler(0f, 0f, sightAngle * 0.5f) * drawDir;
        Vector3 rightDir = Quaternion.Euler(0f, 0f, -sightAngle * 0.5f) * drawDir;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * sightDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * sightDistance);
    }
}