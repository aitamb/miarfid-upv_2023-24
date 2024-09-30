using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
using System;

enum ScoreActions { AddScore }

[RequireComponent(typeof(RTDESKEntity))]

public class ScoreReceiveMessage : MonoBehaviour
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
            case (int)UserMsgTypes.Score:
                Score act = (Score)Msg;
                if (Msg.Sender.name == "Pellet(Clone)" ||
                    Msg.Sender.name == "PowerPellet(Clone)") {
                    string a = GetComponent<UnityEngine.UI.Text>().text;
                    int b = int.Parse(a);
                    b += act.score;
                    GetComponent<UnityEngine.UI.Text>().text = b.ToString();
                }
                Engine.PushMsg(Msg);
                break;
        }
    }
}
