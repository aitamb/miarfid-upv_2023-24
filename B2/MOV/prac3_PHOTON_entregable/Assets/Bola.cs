using Fusion;
using UnityEngine;

public class Bola : NetworkBehaviour
{
    [Networked] private TickTimer vida { get; set; }

    public void InitVida() {
        vida = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (vida.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += 5 * transform.forward * Runner.DeltaTime;
    }
}
