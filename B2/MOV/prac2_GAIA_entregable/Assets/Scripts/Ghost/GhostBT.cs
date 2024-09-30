#define PANDA

using GAIA;
using Panda;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhostBT : MonoBehaviour
{
    private Rigidbody2D ghost_rigidbody;
    private GameObject pacman;

    // IA atributos
    private GAIA_Manager manager;      // Referencia al manager
    public string BTFileName;          // Choose the BT to load by txt file name.

    // Variables de scripts
    public GameManager gm;             // script GameManager
    public Ghost ghost;                // script Ghost
    public GhostScatter scatter;       // script GhostScatter
    public GhostChase chase;           // script GhostChase
    public GhostFrightened frightened; // script GhostFrightened
    public float distance;             // Distancia entre pacman y fantasma

    // UNITY FUNCTIONS ////////////////////////////////////////////////////////
    // Awake
    void Awake()
    {
        pacman = GameObject.Find("Pacman");
        scatter = GetComponent<GhostScatter>();
        chase = GetComponent<GhostChase>();
        frightened = GetComponent<GhostFrightened>();
        ghost = GetComponent<Ghost>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        ghost_rigidbody = GetComponent<Rigidbody2D>();
    }

    // Start
    void Start()
    {
        manager = GAIA_Controller.INSTANCE.m_manager;
#if (PANDA)
            manager.createBT(gameObject, BTFileName);
#endif

        // Inicializacion del BT
        scatter.Enable();
        frightened.Disable();
    }

    // Update 
    void Update()
    {
        Vector3 pacman_position = pacman.transform.position;
        distance = Vector3.Distance(ghost_rigidbody.transform.position, pacman_position);
    }

    // TASKS PARA EL BT ///////////////////////////////////////////////////////
    // Funciones asociadas a los nombres del BT en el .xml

    [Task]
    bool isPPeaten()
    {
        return gm.pp_eaten;
    }

    [Task]
    bool Frighten()
    {
        frightened.Enable(3);
        return true;
    }

    [Task]
    bool ghostDead()
    {
        return ghost.ghost_dead;
    }

    [Task]
    bool Die()
    {
        frightened.Eaten();
        return true;
    }

    [Task]
    bool Scatter()
    {
        chase.Disable();
        return true;
    }

    [Task]
    bool pacmanVisto()
    {
        return distance < 9.5;
    }

    [Task]
    bool pacmanNoVisto()
    {
        return distance >= 9.5;
    }

    [Task]
    bool Chase()
    {
        scatter.Disable();
        return true;
    }
}