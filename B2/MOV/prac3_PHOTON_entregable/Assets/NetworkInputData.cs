using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput { 
    public Vector3 direction; 
    public const byte MOUSEBUTTON0 = 1;
    public NetworkButtons botones;
}