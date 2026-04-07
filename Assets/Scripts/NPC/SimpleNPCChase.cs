using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleNPCChase : MonoBehaviour
{
    public NPCStateUI npcStateUI;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float pointReachDistance = 0.2f;
    [SerializeField] private float waitTime = 1f;

    [Header("Detection / Chase")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float catchRange = 1.2f;

    [Header("Scene")]
    [SerializeField] private string loseSceneName = "Defeat";

    private Transform currentTarget;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isChasing = false;

    private void Start()
    {
        currentTarget = pointA;
    }

    private void Update()
    {
        if (player == null || pointA == null || pointB == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;
        }

        if (distanceToPlayer <= catchRange)
        {
            SceneManager.LoadScene(loseSceneName);
            return;
        }

        if (isChasing)
        {
            ChasePlayer();
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                SwitchTarget();
            }

            return;
        }

        Patrol();
    }

    private void Patrol()
    {
      //  npcStateUI.UpdateStateUI("Patrolling");
        Vector3 targetPos = new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z);
        Vector3 direction = (targetPos - transform.position).normalized;

        transform.position += direction * patrolSpeed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction;
        }

        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= pointReachDistance)
        {
            transform.position = targetPos;
            isWaiting = true;
            waitTimer = waitTime;
        }
    }

    private void ChasePlayer()
    {
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 direction = (targetPos - transform.position).normalized;

        transform.position += direction * chaseSpeed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction;
        }
    }

    private void SwitchTarget()
    {
        currentTarget = (currentTarget == pointA) ? pointB : pointA;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchRange);
    }
}