/**
 * ObjectTemplate: Basic Message Management for any generic game object that can send or receive RTDESK messages
 *
 * Copyright(C) 2022
 *
 * Prefix: OT_

 * @Author: Dr. Ramón Mollá Vayá
 * @Date:	12/2022
 * @Version: 2.0
 *
 * Update:
 * Date:	
 * Version: 
 * Changes:
 *
 */

#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

//----constantes y tipos-----
#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

using UnityEngine;

enum ObjectActions { Idle, Start, Sleep, WakeUp, End }

// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
public class ObjectTemplate : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis;

    [SerializeField]
     RTDESKEngine       Engine;   //Shortcut

    private void Awake()
    {
        //Assign the "listener" to the normalized component RTDESKEntity. Every gameObject that wants to receive a message must have a public mailbox
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    // Start is called before the first frame update
    void Start()
    {
        Action          ActMsg;
        
        Engine = GetComponent<RTDESKEntity>().RTDESKEngineScript;
        RTDESKInputManager IM = Engine.GetInputManager();

        //Register keys that we want to be signaled in case the user press them
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.UpArrow);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.DownArrow);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.LeftArrow);
        IM.RegisterKeyCode(ReceiveMessage, KeyCode.RightArrow);

        //Debug.Log("Solicitud de Acción por cubo");
        //Get a new message to activate a new action in the object
        ActMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        //Update the content of the message sending and activation 
        ActMsg.action = (int)ObjectActions.Start;

        halfSecond = Engine.ms2Ticks(500);
        tenMillis  = Engine.ms2Ticks(10);
        oneSecond  = Engine.ms2Ticks(1000);

        Engine.SendMsg(ActMsg, gameObject, ReceiveMessage, tenMillis);
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                RTDESKInputMsg IMsg = (RTDESKInputMsg)Msg;
                switch (IMsg.c)
                {
                    case KeyCode.UpArrow:
                        //if (KeyState.DOWN == IMsg.s)
                         //Debug.Log("Flecha arriba");
                        break;
                    case KeyCode.DownArrow:
                        //if (KeyState.DOWN == IMsg.s)
                        //Debug.Log("Flecha abajo");
                        break;
                    case KeyCode.LeftArrow:
                        //if (KeyState.DOWN == IMsg.s)
                        //Debug.Log("Flecha a la izquierda");
                        break;
                    case KeyCode.RightArrow:
                        //if (KeyState.DOWN == IMsg.s)
                        //Debug.Log("Flecha a la derecha");
                        break;
                    }
                Engine.PushMsg(Msg);
                break;
            case (int)UserMsgTypes.Position:
                break;
            case (int)UserMsgTypes.Rotation:
                break;
            case (int)UserMsgTypes.Scale:
                break;
            case (int)UserMsgTypes.TRE:
                break;
            case (int)UserMsgTypes.Action:
                Action a;
                a = (Action)Msg;
                //Sending automessage
                if (name == Msg.Sender.name)
                    switch ((int)a.action)
                    {
                        case (int)ObjectActions.Idle:                              
                            break;
                        case (int)ObjectActions.Sleep:
                            break;
                        case (int)ObjectActions.WakeUp:
                            break;
                        case (int)ObjectActions.End:
                            break;
                        case (int)ObjectActions.Start:
                            //We have to start the Object behaviour

                            break;
                        default:
                            break;
                    }
                else
                {
                    switch ((int)a.action)
                    {
                        case (int)UserActions.GetSteady: //Stop the movement of the object
                            break;
                        case (int)UserActions.Move:
                            break;
                    }
                    Engine.PushMsg(Msg);
                }
        break;
        }
    }
}
