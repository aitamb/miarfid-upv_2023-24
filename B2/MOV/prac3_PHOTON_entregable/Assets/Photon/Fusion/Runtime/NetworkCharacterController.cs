namespace Fusion {
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using UnityEngine;

    [StructLayout(LayoutKind.Explicit)]
    [NetworkStructWeaved(WORDS + 4)]
    public unsafe struct NetworkCCData : INetworkStruct {
        public const int WORDS = NetworkTRSPData.WORDS + 4;
        public const int SIZE  = WORDS * 4;

        [FieldOffset(0)]
        public NetworkTRSPData TRSPData;

        [FieldOffset((NetworkTRSPData.WORDS + 0) * Allocator.REPLICATE_WORD_SIZE)]
        int _grounded;

        [FieldOffset((NetworkTRSPData.WORDS + 1) * Allocator.REPLICATE_WORD_SIZE)]
        Vector3Compressed _velocityData;

        public bool Grounded {
            get => _grounded == 1;
            set => _grounded = (value ? 1 : 0);
        }

        public Vector3 Velocity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _velocityData;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _velocityData = value;
        }
    }

    [DisallowMultipleComponent]
    [NetworkBehaviourWeaved(NetworkCCData.WORDS)]
    // ReSharper disable once CheckNamespace
    public sealed unsafe class NetworkCharacterController : NetworkTRSP, IBeforeAllTicks, IAfterAllTicks, IBeforeCopyPreviousState {
    new ref NetworkCCData Data => ref ReinterpretState<NetworkCCData>();

    [Header("Character Controller Settings")]
    public float gravity = -20.0f;
    public float jumpImpulse   = 8.0f;
    public float acceleration  = 10.0f;
    public float braking       = 10.0f;
    public float maxSpeed      = 2.0f;
    public float rotationSpeed = 15.0f;

    Tick _initial;

    public LayerMask obstacleLayer;

    public void Move(Vector3 direction) { // IMPLEMENTAR EL NUESTRO
        var deltaTime    = Runner.DeltaTime;
        var previousPos  = transform.position;
        var moveVelocity = Data.Velocity;

        Debug.Log("Direction: " + direction);
        Debug.Log("Position: " + transform.position);
        // Mover pacman si no hay obstaculo
        if (!Occupied(direction))
            transform.position += direction * deltaTime * 5;

        // Rotate pacman to face the movement direction
        float angle = Mathf.Atan2(direction.y, direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward); 
    }

    public bool Occupied(Vector2 direction)
    {
        Debug.Log("Occupied direction: " + direction);
        // If no collider is hit then there is no obstacle in that direction
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, direction, 0.2f, obstacleLayer);
        return hit.collider != null;
    }

    public override void Spawned() {
        _initial = default;
        CopyToBuffer();
    }

    public override void Render() {
        NetworkTRSP.Render(this, transform, false, false, false, ref _initial);
    }

    void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount) {
        CopyToEngine();
    }

    void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount) {
        CopyToBuffer();
    }

    void IBeforeCopyPreviousState.BeforeCopyPreviousState() {
        CopyToBuffer();
    }

    void CopyToBuffer() {
        Data.TRSPData.Position = transform.position;
        Data.TRSPData.Rotation = transform.rotation;
    }

    void CopyToEngine() {
        // set position and rotation
        transform.SetPositionAndRotation(Data.TRSPData.Position, Data.TRSPData.Rotation);
    }
    }
}