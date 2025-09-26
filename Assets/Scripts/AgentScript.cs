using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AgentScript : MonoBehaviour
{
    public Transform[] waypoints;
    public int indiceRuta = 0;

    public Transform objetivo;
    public float rangoVision = 10f;
    public float rangoAngulo = 60f;
    public float rangoCaptura = 1.5f;
    public Transform sensorVista;
    public float tiempoPerdidaMax = 2f;

    public float tiempoFueraVista = 0f;
    public bool enPersecucion = false;

    public Transform puntoMasCercano;
    public float distanciaMinima;

    public NavMeshAgent navAgente;
    public Animator animador;

    private void Awake()
    {
        navAgente = GetComponent<NavMeshAgent>();
        animador = GetComponent<Animator>();
    }

    private void Start()
    {
        if (waypoints.Length > 0)
        {
            navAgente.SetDestination(waypoints[indiceRuta].position);
        }
    }

    private void Update()
    {
        animador.SetFloat("Speed", navAgente.velocity.magnitude);

        if (enPersecucion)
        {
            SeguirObjetivo();
        }
        else
        {
            PatrullarRuta();
            VerificarObjetivo();
        }
    }

    void PatrullarRuta()
    {
        if (waypoints.Length == 0) return;

        if (!navAgente.pathPending && navAgente.remainingDistance < 0.5f)
        {
            indiceRuta = (indiceRuta + 1) % waypoints.Length;
            navAgente.SetDestination(waypoints[indiceRuta].position);
        }
    }

    void VerificarObjetivo()
    {
        Vector3 direccion = (objetivo.position - sensorVista.position).normalized;
        float distancia = Vector3.Distance(objetivo.position, sensorVista.position);

        if (distancia <= rangoVision)
        {
            float angulo = Vector3.Angle(sensorVista.forward, direccion);
            if (angulo < rangoAngulo)
            {
                if (Physics.Raycast(sensorVista.position, direccion, out RaycastHit hit, rangoVision))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        enPersecucion = true;
                        tiempoFueraVista = 0f;
                    }
                }
            }
        }
    }

    void SeguirObjetivo()
    {
        navAgente.SetDestination(objetivo.position);

        float distancia = Vector3.Distance(transform.position, objetivo.position);
        if (distancia <= rangoCaptura)
        {
            SceneManager.LoadScene("Perdiste");
        }

        Vector3 direccion = (objetivo.position - sensorVista.position).normalized;
        if (Physics.Raycast(sensorVista.position, direccion, out RaycastHit hit, rangoVision) && hit.collider.CompareTag("Player"))
        {
            tiempoFueraVista = 0f;
        }
        else
        {
            tiempoFueraVista += Time.deltaTime;
            if (tiempoFueraVista >= tiempoPerdidaMax)
            {
                enPersecucion = false;
                tiempoFueraVista = 0f;

                if (waypoints.Length > 0)
                {
                    puntoMasCercano = waypoints[0];
                    distanciaMinima = Vector3.Distance(transform.position, waypoints[0].position);

                    foreach (Transform p in waypoints)
                    {
                        float d = Vector3.Distance(transform.position, p.position);
                        if (d < distanciaMinima)
                        {
                            distanciaMinima = d;
                            puntoMasCercano = p;
                        }
                    }
                    navAgente.SetDestination(puntoMasCercano.position);
                }
            }
        }
    }
}
