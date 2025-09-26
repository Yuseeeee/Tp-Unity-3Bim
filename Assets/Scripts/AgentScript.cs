using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AgentScript : MonoBehaviour
{
    public Transform[] waypoints;
    public int currentWaypoint = 0;
    public float capacidadVision = 12f;
    public NavMeshAgent agent;
    public Animator anim;
    public Transform player;
    public float anguloVision = 60f;
    public float distanciaPerseguir = 1.5f;
    public Transform vistaNPC;
    public float tiempoPerdidaVista = 2f;
    public float tiempoSinVer = 0f;
    public bool perseguir = false;
    public Transform puntoCercano;
    public float minDistancia;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypoint].position);
        }
    }

    void Update()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);

        if (perseguir)
        {
            PerseguirPlayer();
        }
        else
        {
            Patrullar();
            DetectPlayer();
        }
    }

    void Patrullar()
    {
        if (waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypoint].position);
        }
    }

    void DetectPlayer()
    {
        Vector3 direccion = (player.position - vistaNPC.position).normalized;
        float distancia = Vector3.Distance(player.position, vistaNPC.position);

        if (distancia <= capacidadVision)
        {
            float angulo = Vector3.Angle(vistaNPC.forward, direccion);
            if (angulo < anguloVision)
            {
                if (Physics.Raycast(vistaNPC.position, direccion, out RaycastHit hit, capacidadVision))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        perseguir = true;
                        tiempoSinVer = 0f;
                    }
                }
            }
        }
    }

    void PerseguirPlayer()
    {
        agent.SetDestination(player.position);

        float distancia = Vector3.Distance(transform.position, player.position);
        if (distancia <= distanciaPerseguir)
        {
            SceneManager.LoadScene("Perdiste");
        }

        Vector3 direccion = (player.position - vistaNPC.position).normalized;
        if (Physics.Raycast(vistaNPC.position, direccion, out RaycastHit hit, capacidadVision) && hit.collider.CompareTag("Player"))
        {
            tiempoSinVer = 0f;
        }
        else
        {
            tiempoSinVer += Time.deltaTime;
            if (tiempoSinVer >= tiempoPerdidaVista)
            {
                perseguir = false;
                tiempoSinVer = 0f;

                if (waypoints.Length > 0)
                {
                    puntoCercano = waypoints[0];
                    minDistancia = Vector3.Distance(transform.position, waypoints[0].position);

                    foreach (Transform p in waypoints)
                    {
                        float d = Vector3.Distance(transform.position, p.position);
                        if (d < minDistancia)
                        {
                            minDistancia = d;
                            puntoCercano = p;
                        }
                    }
                    agent.SetDestination(puntoCercano.position);
                }
            }
        }
    }
}
