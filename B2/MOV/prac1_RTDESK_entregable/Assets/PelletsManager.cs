using System.Collections.Generic;
using UnityEngine;
using HRT_Time = System.Int64;
using System;

enum PelletsManagerActions { Listen }

[RequireComponent(typeof(RTDESKEntity))]

public class PelletsManager : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, centMillis;

    RTDESKEngine Engine;

    string gameObjectName;

    int pelletsCount;

    private void Awake()
    {
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    void Start()
    {
        Action ListenMsg;

        gameObjectName = gameObject.name;

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);
        centMillis = Engine.ms2Ticks(100);

        // Mensajes Action
        ListenMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        ListenMsg.action = (int)PelletsManagerActions.Listen;

        // Enviar mensajes
        Engine.SendMsg(ListenMsg, gameObject, ReceiveMessage, tenMillis);
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
                        case (int)PelletsManagerActions.Listen:
                            pelletsCount = 0;
                            for (int i = 0; i < transform.childCount; i++)
                                if (transform.GetChild(i).gameObject.activeSelf)
                                    pelletsCount++;
                            if (pelletsCount == 0)
                            {
                                // Enviar mensaje a GameManager para que cambie a WonGame
                                Action WonGameMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                                WonGameMsg.action = (int)GameManagerActions.GoWonGame;

                                Engine.SendMsg(WonGameMsg, gameObject, RTDESKEntity.getMailBox("GameManager"), centMillis);
                                Engine.PushMsg(Msg);
                            }
                            else
                                Engine.SendMsg(Msg, centMillis);
                            break;
                    }
                break;
        }
    }
}