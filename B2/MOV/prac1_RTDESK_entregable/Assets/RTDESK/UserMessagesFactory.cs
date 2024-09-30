/**
 * UserMessagesFactory: Factory class for making the user's messages
 *
 * Copyright(C) 2022
 *
 * Prefix: RTDM_

 * @Author: Dr. Ramón Mollá Vayá
 * @Date:	11/2022
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

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum RTDESKMsgTypes
{
	Test,	//For testing purposes anywhere
	Input,	//Input Manager message
	RTDESK_MAX_MsgTypes
};

public enum KeyState
{
	DOWN,   //The key is pushed down 
	UP		//The key is released
}
public class RTDESKInputMsg : MsgContent
{
	public KeyCode	c;   //The code read by the input manager
	public KeyState s;	 //The state of the key. May be pushed down o released
}

/// ACHTUNG: do not touch anything above
/// Restricted for ineternal use only


//Examples of different types of messages to interchange among different GameObjects
public class Transform : MsgContent
{
	public Vector3 V3;
	public override string ToString() => $"({V3.x}, {V3.y}, {V3.z})";

	public Transform() { Type = (int)UserMsgTypes.Position;}
}

//Examples of different types of messages to interchange among different GameObjects
public class ObjectMsg : MsgContent
{
	public GameObject o;

	public ObjectMsg() { Type = (int)UserMsgTypes.Object; }
}

//Translación, Rotación y Escala
public class TRE : MsgContent
{
	public Vector3 pos, rot, esc;

	public override string ToString() => pos.ToString() + " " + rot.ToString() + " " + esc.ToString();

	public TRE() { Type = (int)UserMsgTypes.TRE; }
}

public class Action : MsgContent
{
	public int action;
	public override string ToString() => action.ToString();

	public Action() { Type = (int)UserMsgTypes.Action; }
}

public class Score : MsgContent
{ 
	public int score;
	public override string ToString() => score.ToString();

	public Score() { Type = (int)UserMsgTypes.Score; }
}

public class Teleport : MsgContent
{
	public Vector3 pos;
	public override string ToString() => pos.ToString();

	public Teleport() { Type = (int)UserMsgTypes.Teleport; }
}

public class State : MsgContent
{
    public int state;
    public override string ToString() => state.ToString();

    public State() { Type = (int)UserMsgTypes.State; }
}

public enum UserMsgTypes
{
	Position = RTDESKMsgTypes.RTDESK_MAX_MsgTypes,	//The first enumerated user message type is the last used by the RTDESK system
	Rotation, Scale, TRE, Speed, Action, Object, Score,
	Teleport, State,
	TotalAmountUserMsgTypes,
};

public enum UserActions {	Start,
							LiveState,
							GetSteady,	//Stop the movement of the object
							Move,		//Start moving the object
							End
						 };

//The component that creates an internal RTDESK Engine
public class UserMessagesFactory
{
	public const int RTDM_NO_TYPE_MSG = -1;

	public int MsgAmount = (int)UserMsgTypes.TotalAmountUserMsgTypes;

	public MsgContent CreateMsg(int type)
    {
		MsgContent msg;

		switch (type)
        {
			case (int)RTDESKMsgTypes.Input:
				msg = new RTDESKInputMsg();
				break;
			case (int)UserMsgTypes.Object:
				msg = new ObjectMsg();
				break;
			case (int)UserMsgTypes.Position:
				msg = new Transform();
				break;
			case (int)UserMsgTypes.Rotation:
				msg = new Transform(); 
				break;
			case (int)UserMsgTypes.Scale:
				msg = new Transform();
				break;
			case (int)UserMsgTypes.TRE:
				msg = new TRE();
				break;
			case (int)UserMsgTypes.Speed:
				msg = new Transform();
				break;
			case (int)UserMsgTypes.Action:
				msg = new Action();
				break;
			case (int)UserMsgTypes.Score:
                msg = new Score();
                break;
			case (int)UserMsgTypes.Teleport:
                msg = new Teleport();
                break;
			case (int)UserMsgTypes.State:
                msg = new State();
                break;
			default:
				msg = new MsgContent();
				break;
		}
		msg.Type = type;
		return msg;
	}
}