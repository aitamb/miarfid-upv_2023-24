using System.Collections.Generic;
using UnityEngine;
using HRT_Time = System.Int64;
using System;

public enum GameManagerActions { GoWonGame, GoGameOver, StartGame }
enum GameManagerStates { Initial, InGame, GameOver }

[RequireComponent(typeof(RTDESKEntity))]
public class GameManager : MonoBehaviour
{
    HRT_Time userTime;
    HRT_Time oneSecond, halfSecond, tenMillis, centMillis;

    RTDESKEngine Engine;

    int gameManagerState;
    string gameObjectName;

    public GameObject initScreen, gameScreen, overScreen, wonScreen;

    private void Awake()
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

        gameManagerState = (int)GameManagerStates.Initial;

        RTDESKInputManager InputManager = engine.GetComponent<RTDESKInputManager>();
        InputManager.RegisterKeyCode(ReceiveMessage, KeyCode.Return);
    }

    void ReceiveMessage(MsgContent Msg)
    {
        switch (Msg.Type)
        {
            case (int)RTDESKMsgTypes.Input:
                // Si esta en la pantalla inicial
                if (gameManagerState == (int)GameManagerStates.Initial)
                {
                    if (gameManagerState == (int)GameManagerStates.Initial)
                    {
                        RTDESKInputMsg IMsg = (RTDESKInputMsg)Msg;
                        switch (IMsg.c)
                        {
                            case KeyCode.Return:
                                if (KeyState.DOWN == IMsg.s)
                                {
                                    // Apagar pantalla de inicio
                                    initScreen.SetActive(false);
                                    gameManagerState = (int)GameManagerStates.InGame;
                                    // Encender pantalla de juego
                                    gameScreen.SetActive(true);
                                }
                                break;
                        }
                    }
                }
                Engine.PushMsg(Msg);
                break;

            case (int)UserMsgTypes.Action:
                if (gameManagerState == (int)GameManagerStates.InGame)
                {
                    Action act = (Action)Msg;
                    gameManagerState = (int)GameManagerStates.GameOver;
                    // Apagar pantalla de juego
                    gameScreen.SetActive(false);
                    switch ((int)act.action)
                    {
                        case (int)GameManagerActions.GoWonGame:
                            // Encender pantalla de ganar
                            wonScreen.SetActive(true);
                            break;

                        case (int)GameManagerActions.GoGameOver:
                            // Encender pantalla de ganar
                            overScreen.SetActive(true);
                            break;
                    }
                }
                Engine.PushMsg(Msg);
                break;
        }
    }
}