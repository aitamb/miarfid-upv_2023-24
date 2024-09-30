using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HRT_Time = System.Int64;

public enum GhostStates { Idle, Frightened };

[RequireComponent(typeof(RTDESKEntity))]

public class GhostManager : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, centMillis, frightenedTime;

    // Lista de los Ghosts que gestiona
    List<MessageManager> Ghosts = new List<MessageManager>();

    RTDESKEngine Engine;

    string gameObjectName;

    public int ghostState = (int)GhostStates.Idle;

    public int frightenedCount = 0;

    void Awake()
    {
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    void Start()
    {
        gameObjectName = gameObject.name;

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);
        centMillis = Engine.ms2Ticks(100);
        frightenedTime = Engine.ms2Ticks(10000); // 10 segundos
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)UserMsgTypes.Object: // Guardar MailBox de cada Ghost
                Ghosts.Add(RTDESKEntity.getMailBox(((ObjectMsg)Msg).o));
                Engine.PushMsg(Msg);
                break;  

            case (int)UserMsgTypes.State:
                State sta = (State)Msg;
                if (sta.state == (int)GhostStates.Frightened) {
                    ghostState = (int)GhostStates.Frightened;
                    frightenedCount = 0;
                    // Enviar mensaje a todos los Ghosts para que cambien a asustado
                    for (int i = 0; i < Ghosts.Count; i++)
                    {
                        State FrighStateMsg = (State)Engine.PopMsg((int)UserMsgTypes.State);
                        FrighStateMsg.state = (int)GhostStates.Frightened;
                        Engine.SendMsg(FrighStateMsg, gameObject, Ghosts[i], centMillis);
                    }
                    // Enviar el primer mensaje para que vuelvan a su estado normal
                    State NewStateMsg = (State)Engine.PopMsg((int)UserMsgTypes.State);
                    NewStateMsg.state = (int)GhostStates.Idle;
                    Engine.SendMsg(NewStateMsg, gameObject, ReceiveMessage, oneSecond);
                    Engine.PushMsg(Msg);
                }
                else if (sta.state == (int)GhostStates.Idle)
                {
                    if (frightenedCount == 10)
                    {
                        // Mensaje a todos los Ghosts para que vuelvan a su estado normal
                        ghostState = (int)GhostStates.Idle;
                        for (int i = 0; i < Ghosts.Count; i++)
                        {
                            State IdleStateMsg = (State)Engine.PopMsg((int)UserMsgTypes.State);
                            IdleStateMsg.state = (int)GhostStates.Idle;
                            Engine.SendMsg(IdleStateMsg, gameObject, Ghosts[i], centMillis);
                        }
                        Engine.PushMsg(Msg);
                    }
                    else
                    { 
                        frightenedCount += 1;
                        Engine.SendMsg(Msg, gameObject, ReceiveMessage, oneSecond);
                    }
                }
                break;
        }
    }
}