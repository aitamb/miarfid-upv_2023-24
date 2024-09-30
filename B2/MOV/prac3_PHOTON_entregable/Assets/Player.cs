using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Bola _prefabPellets;

    [SerializeField] private TickTimer delay { get; set; }
    private NetworkCharacterController _cc;
    private Vector3 _forward;

    private void Awake()
    {
        GameObject _player = transform.GetChild(0).gameObject;
        _player.GetComponent<Renderer>().material.color = new Color(Random.Range(0.6f, 1), Random.Range(0.6f, 1), Random.Range(0.6f, 1));
        _cc = GetComponent<NetworkCharacterController>(); 
        _forward = transform.forward;
    }
    public override void FixedUpdateNetwork() { // Se llama e cada instante de simulacion (puuede suceder varias veces por ciclo de Unity)
        // GetInput() obtiene la entrada desde la red en cada ciclo de red
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            // Una vez se tenga la ultima entrada, se llama al NetworkCharacterController para aplicar el movimiento
            // real de transfomracion del avatar local
            _cc.Move(data.direction);

            if (data.direction.sqrMagnitude > 0) _forward = data.direction;

            //if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner)) { 
            //    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
            //    Runner.Spawn(_prefabBola,
            //        transform.position + _forward, Quaternion.LookRotation(_forward),
            //        Object.InputAuthority, (runner, o) => { 
            //            // Inicializar la bola antes de sincronizar
            //            o.GetComponent<Bola>().InitVida();
            //        });
                
            //}

        }
    }
}
