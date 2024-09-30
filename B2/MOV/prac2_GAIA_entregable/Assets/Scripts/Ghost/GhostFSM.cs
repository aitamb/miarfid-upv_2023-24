using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GAIA;
using UnityEditor.VersionControl;
using System.Linq.Expressions;
using UnityEditor.PackageManager.Requests;

public class GhostFSM : MonoBehaviour
{
    private Rigidbody2D ghost_rigidbody;
    private GameObject pacman;

    // IA atributos
    private FSM_Machine FSM; // Contiene a la FSM
    private GAIA_Manager manager; // Referencia al manager
    private List<int> FSMactions; // Acciones a realizar
    private List<int> FSMevents = new List<int>(); //Eventos

    // Variables acciones y eventos
    public GameManager gm;             // script GameManager
    public Ghost ghost;                // script Ghost
    public GhostScatter scatter;       // script GhostScatter
    public GhostChase chase;           // script GhostChase
    public GhostFrightened frightened; // script GhostFrightened
    public float distance;             // Distancia entre pacman y fantasma

    // FSM FUNCTIONS //////////////////////////////////////////////////////////
    // Buscar eventos
    public List<int> BuscarEventos()
    {
        FSMevents.Clear();
        if(ghost.resurrect)
        {
            FSMevents.Add((int)Tags.EventTags.RESURRECT);
        }
        if (FSM.getCurrentState().getTag() != (int)Tags.StateTags.FRIGHTENING)
        {
            if (gm.pp_eaten)
            {
                FSMevents.Add((int)Tags.EventTags.POWER_PELLET_ON);
            }
            if (distance < 9.5)
            {
                FSMevents.Add((int)Tags.EventTags.PACMAN_VISTO);
            }
            else
            {
                FSMevents.Add((int)Tags.EventTags.PACMAN_NO_VISTO);
            }
        }
        else {
            if (ghost.ghost_dead)
            {
                FSMevents.Add((int)Tags.EventTags.GHOST_DEAD);
            }
            if (!gm.pp_eaten)
            {
                FSMevents.Add((int)Tags.EventTags.POWER_PELLET_OFF);
            }
            else
            {
                FSMevents.Add((int)Tags.EventTags.POWER_PELLET_ON);
            }
        }
        if (FSMevents.Count == 0) addNoEvent();

        return FSMevents;
    }

    // Ejecutar acciones
    public void ExecuteAction(int actionTag)
    {
        switch (actionTag)
        {
            case (int)Tags.ActionTags.SCATTER:
                Scatter();
                break;

            case (int)Tags.ActionTags.CHASE:
                Chase();
                break;

            case (int)Tags.ActionTags.FRIGHTEN:
                Frighten();
                break;

            case (int)Tags.ActionTags.DIE:
                Die();
                break;

            case (int)Tags.ActionTags.RESET:
                Reset();
                break;
        }
    }

    // Otras funciones
    private void addNoEvent()
    {
        FSMevents.Add((int)Tags.EventTags.NULL);
    }

    // ACCIONES ///////////////////////////////////////////////////////////////
    private void Scatter()
    {
        chase.Disable();
    }

    private void Chase()
    {
        scatter.Disable();
    }

    private void Frighten()
    {
        frightened.Enable(12);
    }
    private void Die()
    {
        frightened.Eaten();
    }

    public void Reset()
    {
        ghost.ResetState();
    }

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
        FSM = manager.createMachine(this,
                (int)FA_Classic.FAType.CLASSIC,
                "GhostRedFSM");
        addNoEvent();

        // Inicializacion de la FSM
        scatter.Enable();
        frightened.Disable();
    }

    // Update 
    void Update()
    {
        Vector3 pacman_position = pacman.transform.position;
        distance = Vector3.Distance(ghost_rigidbody.transform.position, pacman_position);

        FSMactions = FSM.Update();
        for (int i = 0; i < FSMactions.Count; i++)
        {
            if (FSMactions[i] != (int)Tags.ActionTags.NULL)
            {
                ExecuteAction(FSMactions[i]);
            }
        }
    }
}
