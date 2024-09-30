using UnityEngine;
using HRT_Time = System.Int64;
using System;

public enum PacmanActions { EatAnimation, Move, Die, DieAnimation }
enum PacmanStates { Alive, Dead }

[RequireComponent(typeof(RTDESKEntity))]

public class PacmanReceiveMessage : MonoBehaviour
{
    HRT_Time oneSecond, halfSecond, tenMillis, centMillis;

    RTDESKEngine Engine;

    public MessageManager LivesMailBox, ScoreMailBox;
    string gameObjectName;

    // Variables para el movimiento
    public LayerMask obstacleLayer;
    public LayerMask ghostLayer;

    // Variables para la apariencia
    public Sprite[] sprites = new Sprite[0];
    public SpriteRenderer spriteRenderer;
    public int pacmanState;

    private void Awake()
    {
        GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
    }

    void Start()
    {
        Transform PosMsg;
        Action EatAnimMsg, MoveMsg;

        pacmanState = (int)PacmanStates.Alive;

        gameObjectName = gameObject.name;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[0];

        GameObject engine = GameObject.Find(RTDESKEngine.Name);
        Engine = engine.GetComponent<RTDESKEngine>();

        oneSecond = Engine.ms2Ticks(1000);
        halfSecond = Engine.ms2Ticks(500);
        tenMillis = Engine.ms2Ticks(10);
        centMillis = Engine.ms2Ticks(100);

        // Obtener el MailBox del LivesText
        LivesMailBox = RTDESKEntity.getMailBox("LivesText");
        ScoreMailBox = RTDESKEntity.getMailBox("ScoreText");

        RTDESKInputManager InputManager = engine.GetComponent<RTDESKInputManager>();
        InputManager.RegisterKeyCode(ReceiveMessage, KeyCode.UpArrow);
        InputManager.RegisterKeyCode(ReceiveMessage, KeyCode.DownArrow);
        InputManager.RegisterKeyCode(ReceiveMessage, KeyCode.LeftArrow);
        InputManager.RegisterKeyCode(ReceiveMessage, KeyCode.RightArrow);

        // Mensajes Position
        PosMsg = (Transform)Engine.PopMsg((int)UserMsgTypes.Position);

        // Mensajes Action
        EatAnimMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        EatAnimMsg.action = (int)PacmanActions.EatAnimation;

        MoveMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
        MoveMsg.action = (int)PacmanActions.Move;

        // Enviar los mensajes
        Engine.SendMsg(PosMsg, gameObject, ReceiveMessage, halfSecond);
        Engine.SendMsg(EatAnimMsg, gameObject, ReceiveMessage, halfSecond);
        Engine.SendMsg(MoveMsg, gameObject, ReceiveMessage, 3 * oneSecond);
    }
    
    void ReceiveMessage(MsgContent Msg) 
    {
        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                // Si pacman esta vivo
                if (pacmanState == (int)PacmanStates.Alive)
                {
                    RTDESKInputMsg IMsg = (RTDESKInputMsg)Msg;
                    switch (IMsg.c)
                    {
                        case KeyCode.UpArrow:
                            if (KeyState.DOWN == IMsg.s)
                                transform.rotation = Quaternion.Euler(0, 0, 90);
                            break;

                        case KeyCode.DownArrow:
                            if (KeyState.DOWN == IMsg.s)
                                transform.rotation = Quaternion.Euler(0, 0, 270);
                            break;

                        case KeyCode.LeftArrow:
                            if (KeyState.DOWN == IMsg.s)
                                transform.rotation = Quaternion.Euler(0, 0, 180);
                            break;

                        case KeyCode.RightArrow:
                            if (KeyState.DOWN == IMsg.s)
                                transform.rotation = Quaternion.Euler(0, 0, 0);
                            break;
                    }
                }
                Engine.PushMsg(Msg);
                break;

            case (int)UserMsgTypes.Action:
                Action act = (Action)Msg;
                if(gameObjectName == Msg.Sender.name)
                    switch((int)act.action)
                    {
                        case (int)PacmanActions.EatAnimation:
                            // Si pacman esta vivo
                            if (pacmanState == (int)PacmanStates.Alive)
                            {
                                int id = Array.IndexOf(sprites, spriteRenderer.sprite) + 1;
                                if (id > 3) id = 0;
                                spriteRenderer.sprite = sprites[id];                                
                            }
                            // Envia el mensaje a si mismo
                            Engine.SendMsg(Msg, 2 * centMillis);
                            break;

                        case (int)PacmanActions.Move:
                            // Si pacman esta vivo
                            if (pacmanState == (int)PacmanStates.Alive)
                            {
                                RaycastHit2D ghostCollision = DetectGhostCollision();
                                // Detectar si hay colisiones
                                if ((bool)DetectGhostCollision())
                                {
                                    // Si modo asustado
                                    if (ghostCollision.transform.GetComponent<GhostReceiveMessage>().ghostState == 1)
                                    {
                                        // Se come al fantasma
                                        Action DieMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                                        DieMsg.action = (int)GhostActions.Die;

                                        Engine.SendMsg(DieMsg, gameObject, RTDESKEntity.getMailBox(ghostCollision.transform.name), centMillis);
                                        // Aumenta puntuacion
                                        Score AddScoreMsg = (Score) Engine.PopMsg((int)UserMsgTypes.Score);
                                        AddScoreMsg.score = 200;

                                        Engine.SendMsg(AddScoreMsg, gameObject, ScoreMailBox, tenMillis);

                                        // Se mueve
                                        transform.Translate(Vector3.right * 0.5f);
                                    }
                                    else
                                    {
                                        // Enviar mensaje de accion de morir
                                        Action DieMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                                        DieMsg.action = (int)PacmanActions.Die;

                                        Engine.SendMsg(DieMsg, gameObject, ReceiveMessage, tenMillis);
                                    }
                                }
                                else if (!DetectWallCollision(transform.right))
                                {
                                    // Si no hay colision, se mueve
                                    transform.Translate(Vector3.right * 0.5f);
                                }
                            }
                            // Envia el mensaje a si mismo
                            Engine.SendMsg(Msg, centMillis);
                            break;

                        case (int)PacmanActions.Die:
                            // Cambiar el estado a muerto y el sprite al primer frame de muerte
                            pacmanState = (int)PacmanStates.Dead;
                            spriteRenderer.sprite = sprites[4];
                            // Enviar el mensaje al contador de vidas
                            Engine.SendMsg(Msg, gameObject, LivesMailBox, centMillis);
                            // Enviar mensaje de animacion de muerte a si mismo
                            Action DieAnimMsg = (Action)Engine.PopMsg((int)UserMsgTypes.Action);
                            DieAnimMsg.action = (int)PacmanActions.DieAnimation;
                            
                            Engine.SendMsg(DieAnimMsg, gameObject, ReceiveMessage, centMillis);
                            break;

                        case (int)PacmanActions.DieAnimation:
                            // Si pacman esta muerto
                            if (pacmanState == (int)PacmanStates.Dead)
                            {
                                int idx = Array.IndexOf(sprites, spriteRenderer.sprite) + 1;
                                if (idx > 14)
                                {
                                    // Finalizar la animacion de muerte
                                    restartPacman();
                                    Engine.PushMsg(Msg);
                                }
                                else
                                {
                                    spriteRenderer.sprite = sprites[idx];
                                    // Enviar el mensaje de animacion a si mismo
                                    Engine.SendMsg(Msg, centMillis);
                                }
                            }
                            break;
                    }
                break;

            case (int)UserMsgTypes.Teleport:
                Teleport tp = (Teleport)Msg;
                if (Msg.Sender.name == "Passage_Left")
                { 
                    // Teletransportar a la derecha
                    transform.position = tp.pos;
                }
                else if (Msg.Sender.name == "Passage_Right")
                {
                    // Teletransportar a la izquierda
                    transform.position = tp.pos;
                }
                Engine.PushMsg(Msg);
                break;
        }    
    }

    bool DetectWallCollision(Vector3 direction)
    {
        bool hit = Physics2D.Raycast(transform.position, direction, 0.7f, obstacleLayer);
        return hit;
    }

    RaycastHit2D DetectGhostCollision()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.7f, 0f, transform.right, 1f, ghostLayer);
        return hit;
    }

    void restartPacman() 
    { 
        transform.position = new Vector3(0, -9.5f, 0);
        spriteRenderer.sprite = sprites[0];
        pacmanState = (int)PacmanStates.Alive;
    }
}