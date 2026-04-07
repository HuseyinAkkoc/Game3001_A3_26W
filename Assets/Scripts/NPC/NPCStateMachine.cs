using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
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
    [SerializeField] private float stoppingDistance = 0.2f;

    [Header("Vision")]
    [SerializeField] private float sightDistance = 6f;
    [SerializeField] private float sightAngle = 60f;
    [SerializeField] private Vector3 initialFacingDirection = Vector3.forward;

    [Header("Decision Timing")]
    [SerializeField] private float stateDecisionTime = 3f;

    [Header("State Setup")]
    [SerializeField] private NPCState startingState = NPCState.Idle;

    [Header("Scenes")]
    [SerializeField] private string victorySceneName = "VictoryScene";
    [SerializeField] private string defeatSceneName = "DefeatScene";

    [Header("Audio")]
    [SerializeField] private AudioClip stateChangeSFX;
    [SerializeField] private AudioClip collisionSFX;

    private Rigidbody rb;
    private NPCState currentState;

    private float timer;
    private int sameStateCount;
    private bool gameEnded;

    private Transform currentPatrolTarget;
    private Vector3 facingDirection = Vector3.forward;

    public NPCState CurrentState => currentState;
    public float Timer => timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        facingDirection = initialFacingDirection.normalized;
    }

    private void Start()
    {
        transform.position = patrolPointA.position;
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
            StopMovement();
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
        StopMovement();

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

        Vector3 npcFlatPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 targetFlatPos = new Vector3(currentPatrolTarget.position.x, 0f, currentPatrolTarget.position.z);

        float distance = Vector3.Distance(npcFlatPos, targetFlatPos);

        if (distance <= stoppingDistance)
        {
            Vector3 fixedPos = transform.position;
            fixedPos.x = currentPatrolTarget.position.x;
            fixedPos.z = currentPatrolTarget.position.z;
            transform.position = fixedPos;

            StopMovement();

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
            StopMovement();
            return;
        }

        Vector3 targetPosition = currentPatrolTarget.position;
        Vector3 currentPosition = transform.position;

        Vector3 direction = targetPosition - currentPosition;
        direction.y = 0f;

        direction = direction.normalized;

        Vector3 velocity = direction * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (direction.sqrMagnitude > 0.001f)
        {
            SetFacingDirection(direction);
        }
    }

    private void ChasePlayer()
    {
        if (player == null)
        {
            StopMovement();
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        direction = direction.normalized;

        Vector3 velocity = direction * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (direction.sqrMagnitude > 0.001f)
        {
            SetFacingDirection(direction);
        }
    }

    private void StopMovement()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
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
                StopMovement();
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

    private void SetFacingDirection(Vector3 direction)
    {
        facingDirection = direction.normalized;

        Vector3 flatDirection = new Vector3(facingDirection.x, 0f, facingDirection.z);

        if (flatDirection.sqrMagnitude > 0.001f)
        {
            transform.forward = flatDirection;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;

        Vector3 toPlayer = target - origin;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > sightDistance)
            return false;

        float angleToPlayer = Vector3.Angle(facingDirection, toPlayer.normalized);
        if (angleToPlayer > sightAngle * 0.5f)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(origin, toPlayer.normalized, out hit, distanceToPlayer, obstacleMask | playerMask))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
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

    private void OnTriggerEnter(Collider other)
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
        Vector3 drawDir = Application.isPlaying ? facingDirection : transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightDistance);

        Vector3 leftDir = Quaternion.Euler(0f, sightAngle * 0.5f, 0f) * drawDir;
        Vector3 rightDir = Quaternion.Euler(0f, -sightAngle * 0.5f, 0f) * drawDir;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * sightDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * sightDistance);
    }
}