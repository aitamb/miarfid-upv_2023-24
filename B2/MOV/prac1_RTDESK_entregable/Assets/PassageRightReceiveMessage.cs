using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
using System;

enum PassageRightActions { Consult }

[RequireComponent(typeof(RTDESKEntity))]

public class PassageRightReceiveMessage : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, centMillis;

    RTDESKEngine Engine;

    public MessageManager PacmanMailBox;
    public MessageManager GhostManagerMailBox;
    string gameObjectName;

    // Variables para el movimiento
    public LayerMask pacmanLayer;
    public LayerMask ghostLayer;

    private void Awake()
    {
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    void Start()
    {
        Action ConsultMsg;

        gameObjectName = gameObject.name;

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);
        centMillis = Engine.ms2Ticks(100);

        // Obtener el MailBox del Pacman
        PacmanMailBox = RTDESKEntity.getMailBox("Pacman");
        // Obtener el Mailbox del Maganer de Ghosts
        //GhostManagerMailBox = RTDESKEntity.getMailBox("GhostManager");

        // Mensajes Action
        ConsultMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        ConsultMsg.action = (int)PassageRightActions.Consult;

        // Enviar los mensajes
        Engine.SendMsg(ConsultMsg, gameObject, ReceiveMessage, halfSecond);
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)UserMsgTypes.Action:
                Action act = (Action)Msg;
                if (gameObjectName == Msg.Sender.name)
                    switch ((int)act.action)
                    {
                        case (int)PassageRightActions.Consult:
                            // Comprobar si Pacman ha colisionado con el pasaje
                            if (DetectPacmanCollision())
                            {
                                // Mensaje de teletransporte a Pacman
                                Teleport TeleportMsg = (Teleport)Engine.PopMsg((int)UserMsgTypes.Teleport);
                                TeleportMsg.pos = new Vector3(-12f, -0.5f, 0.0f); // posicion de right connection

                                Debug.Log("Pacman ha colisionado con el pasaje");

                                Engine.SendMsg(TeleportMsg, gameObject, PacmanMailBox, tenMillis);
                            }
                            //else if (DetectGhostCollision())
                            //{
                            //    // Mensaje de teletransporte a GhostManager
                            //}
                            // Reenviarse el mensaje a si mismo
                            Engine.SendMsg(Msg, 2 * tenMillis);
                            break;
                    }
                break;
        }
    }

    bool DetectPacmanCollision()
    {
        bool hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, Vector2.left, 1f, pacmanLayer);
        return hit;
    }

    //bool DetectGhostCollision()
    //{
    //    bool hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, Vector2.right, 1f, ghostLayer);
    //    return hit;
    //}
}
