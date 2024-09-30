using System.Collections.Generic;
using UnityEngine;
using HRT_Time = System.Int64;
using System;

public enum GhostActions { Move, BodyAnimation, Die, Resurect }

[RequireComponent(typeof(RTDESKEntity))]

public class GhostReceiveMessage : MonoBehaviour
{
    HRT_Time oneSecond, halfSecond, tenMillis, centMillis;

    RTDESKEngine Engine;

    string gameObjectName;

    public MessageManager GhostsManagerMailBox;

    // Variables para el movimiento
    public LayerMask obstacleLayer;
    readonly List<Vector3> directions =
        new() { Vector3.up, Vector3.down, Vector3.right, Vector3.left };
    public Vector3 ghostDirection;
    public Vector3 initPosition;

    // Variables para la apariencia
    public Sprite[] idleSprites = new Sprite[0];
    public Sprite[] frightenedSprites = new Sprite[0];
    public SpriteRenderer spriteRenderer;
    public int ghostState = 0;
    public Color initColor;

    private void Awake()
    {
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }
    void Start()
    {
        Action MoveMsg, BodyAnimationMsg;
        ObjectMsg ObjMsg;

        gameObjectName = gameObject.name;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = idleSprites[0];

        int i = UnityEngine.Random.Range(0, directions.Count);
        ghostDirection = directions[i];
        initColor = spriteRenderer.color;
        initPosition = transform.position;

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        oneSecond = Engine.ms2Ticks(1000);
        centMillis = Engine.ms2Ticks(100);

        // Obtener MailBox del GhostManager
        GhostsManagerMailBox = RTDESKEntity.getMailBox("GhostsManager");

        // Mensajes Action
        MoveMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        MoveMsg.action = (int)GhostActions.Move;

        BodyAnimationMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        BodyAnimationMsg.action = (int)GhostActions.BodyAnimation;

        // Mensajes Object
        ObjMsg = (ObjectMsg)Engine.PopMsg((int)UserMsgTypes.Object);
        ObjMsg.o = gameObject;

        // Enviar mensajes
        Engine.SendMsg(MoveMsg, gameObject, ReceiveMessage, halfSecond);
        Engine.SendMsg(BodyAnimationMsg, gameObject, ReceiveMessage, halfSecond);
        Engine.SendMsg(ObjMsg, gameObject, GhostsManagerMailBox, halfSecond);
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
                        case (int)GhostActions.Move:
                            // Detectar si hay colision 
                            string collision = DetectCollision(ghostDirection);
                            // Si no hay colision, se mueve
                            if (collision == "")
                                transform.Translate(ghostDirection * 0.5f);
                            else
                            { // Si hay colision, cambia direccion

                                List<Vector3> possibleDirections = new List<Vector3>(directions);
                                possibleDirections.Remove(ghostDirection);
                                int idx = UnityEngine.Random.Range(0, possibleDirections.Count);

                                ghostDirection = possibleDirections[idx];
                            }
                            // Envia el mensaje a si mismo
                            Engine.SendMsg(Msg, centMillis);
                            break;

                        case (int)GhostActions.BodyAnimation:
                            if (ghostState == 0)
                            {
                                int id = Array.IndexOf(idleSprites, spriteRenderer.sprite) + 1;
                                if (id > 1) id = 0;
                                spriteRenderer.sprite = idleSprites[id];
                            }
                            else {
                                int id = Array.IndexOf(frightenedSprites, spriteRenderer.sprite) + 1;
                                if (id > 1) id = 0;
                                spriteRenderer.sprite = frightenedSprites[id];
                            }
                            Engine.SendMsg(Msg, 2 * centMillis);
                            break;

                        case (int)GhostActions.Resurect:
                            // Activar el objeto y ponerlo en la posicion inicial
                            transform.position = initPosition;
                            this.GetComponent<SpriteRenderer>().enabled = true;
                            this.GetComponent<CircleCollider2D>().enabled = true;
                            this.gameObject.layer = 6;
                            Engine.PushMsg(Msg);
                            break;
                    }
                else
                    switch ((int)act.action)
                    {
                        case (int)GhostActions.Die:
                            Debug.Log("Estoy muriendo");
                            // Desactivar el objeto
                            this.GetComponent<SpriteRenderer>().enabled = false;
                            this.GetComponent<CircleCollider2D>().enabled = false;
                            this.gameObject.layer = 2;
                            //// Enviar mensaje de resureccion en 5 segundos
                            Action ResuMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            ResuMsg.action = (int)GhostActions.Resurect;

                            Engine.SendMsg(ResuMsg, gameObject, ReceiveMessage, oneSecond);
                            Engine.PushMsg(Msg);
                            break;
                    }
                break;

            case (int)UserMsgTypes.State:
                State sta = (State)Msg;
                switch ((int)sta.state)
                {
                    case (int)GhostStates.Frightened:
                        ghostState = (int)GhostStates.Frightened;
                        // Para que pueda recuperar el valor del indice del sprite
                        spriteRenderer.sprite = frightenedSprites[0];
                        // Cambiar ojos y color
                        transform.GetChild(0).gameObject.SetActive(false);
                        spriteRenderer.color = Color.white;                        
                        break;

                    case (int)GhostStates.Idle:
                        ghostState = (int)GhostStates.Idle;
                        // Para que pueda recuperar el valor del indice del sprite
                        spriteRenderer.sprite = idleSprites[0];
                        // Cambiar ojos y color
                        transform.GetChild(0).gameObject.SetActive(true);
                        spriteRenderer.color = initColor;
                        break;
                }
                Engine.PushMsg(Msg);
                break;
        }
    }
    string DetectCollision(Vector3 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.7f, obstacleLayer);
        if (!(bool)hit)
            return "";
        else
            return hit.transform.tag;
    }
}
