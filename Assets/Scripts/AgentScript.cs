using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AgentScript : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentWaypoint = 0;

    [Header("Detección")]
    public float viewDistance = 12f;
    public float viewAngle = 60f;
    public float losePlayerTime = 2f;
    public float catchDistance = 1.5f;

    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    private float lastSeenTime;
    private bool chasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        GoToNextPoint();
    }

    void Update()
    {
        if (chasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        DetectPlayer();
    }

    // --- PATRULLA ---
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPoint();

        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f);
    }

    void GoToNextPoint()
    {
        if (waypoints.Length == 0) return;

        // Reinicia en un punto aleatorio
        currentWaypoint = Random.Range(0, waypoints.Length);
        agent.destination = waypoints[currentWaypoint].position;
    }

    // --- DETECCIÓN ---
    void DetectPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < viewDistance)
        {
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle < viewAngle)
            {
                if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, viewDistance))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        chasing = true;
                        lastSeenTime = Time.time;
                        return;
                    }
                }
            }
        }

        // Si estaba persiguiendo pero ya no ve al player por más de 2s
        if (chasing && Time.time - lastSeenTime > losePlayerTime)
        {
            chasing = false;
            GoToNextPoint();
        }
    }

    // --- PERSEGUIR ---
    void ChasePlayer()
    {
        agent.destination = player.position;
        anim.SetBool("isWalking", true);

        if (Vector3.Distance(transform.position, player.position) < catchDistance)
        {
            Debug.Log("Jugador atrapado");
            SceneManager.LoadScene("GameOver"); 
        }
    }
}
