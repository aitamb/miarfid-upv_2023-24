using UnityEngine;
using HRT_Time = System.Int64;

enum PelletActions { Alive, Dead }

[RequireComponent(typeof(RTDESKEntity))]

public class PelletReceiveMessage : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, centMillis;

    RTDESKEngine Engine;

    public MessageManager ScoreMailBox;
    string gameObjectName;

    // Variables para el movimiento
    public LayerMask pacmanLayer;

    private void Awake()
    {
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    void Start()
    {
        Action AliveMsg;

        gameObjectName = gameObject.name;

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);
        centMillis = Engine.ms2Ticks(100);

        // Obtener el MailBox del ScoreText
        ScoreMailBox = RTDESKEntity.getMailBox("ScoreText");

        // Mensajes Action
        AliveMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        AliveMsg.action = (int)PelletActions.Alive;

        // Enviar los mensajes
        Engine.SendMsg(AliveMsg, gameObject, ReceiveMessage, tenMillis);
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
                        case (int)PelletActions.Alive:
                            // Comprobar si Pacman ha colisionado con el pellet
                            if (DetectPacmanCollision())
                            {
                                // Score de aumentar puntuacion
                                Score ScoreMsg = (Score)Engine.PopMsg((int)UserMsgTypes.Score);
                                ScoreMsg.score = 10;

                                // Action de morir
                                Action DieMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                                DieMsg.action = (int)PelletActions.Dead;

                                Engine.SendMsg(ScoreMsg, gameObject, ScoreMailBox, tenMillis);
                                Engine.SendMsg(DieMsg, gameObject, ReceiveMessage, tenMillis);
                                Engine.PushMsg(Msg);
                            }
                            else 
                            { // Enviar el mensaje de Alive
                                Engine.SendMsg(Msg, tenMillis);
                            }
                            break; 
                        
                        case (int)PelletActions.Dead:
                            this.gameObject.SetActive(false);
                            break;
                    }
                break;
        }
    }
    bool DetectPacmanCollision()
    {
        bool hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, Vector2.right, 0f, pacmanLayer);
        return hit;
    }
}