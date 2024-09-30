using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
using System;

enum LivesActions { DecLife }

public class LivesReceiveMessage : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, centMillis;

    RTDESKEngine Engine;

    string gameObjectName;
    private void Awake()
    {
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }
    void Start()
    {
        gameObjectName = gameObject.name;

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)UserMsgTypes.Action:
                Action act = (Action)Msg;
                if (act.action == (int)PacmanActions.Die)
                {
                    string a = GetComponent<UnityEngine.UI.Text>().text;
                    int b = int.Parse(a.Substring(1));
                    b -= 1;
                    if (b == 0)
                    {
                        // Enviar mensaje a GameManager para que cambie a GoGameOver
                        Action GameOverMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                        GameOverMsg.action = (int)GameManagerActions.GoGameOver;

                        Engine.SendMsg(GameOverMsg, gameObject, RTDESKEntity.getMailBox("GameManager"), centMillis);
                    }
                    GetComponent<UnityEngine.UI.Text>().text = "x" + b.ToString();
                }
                Engine.PushMsg(Msg);
                break;
        }
    }
}
