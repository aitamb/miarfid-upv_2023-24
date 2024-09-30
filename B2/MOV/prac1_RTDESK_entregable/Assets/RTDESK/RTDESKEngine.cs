/**
 * RTDeskEngine: The core engine for RTDESK
 *
 * Copyright(C) 2022
 *
 * Prefix: RTDESK_

 * @Author:	Dr. Ramón Mollá Vayá
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
using System.Runtime.CompilerServices;
using UnityEngine;

//The component that creates an internal RTDESK Engine
// CubeReceiveMessage requires the GameObject to have a RTDESKEntity component
[RequireComponent(typeof(RTDESKEntity))]
[RequireComponent(typeof(RTDESKInputManager))]
public class RTDESKEngine : MonoBehaviour
{
	private const int RTDESK_VERSION	= 2;
	private const int RTDESK_SUBVERSION = 0;
	private const int RTDESK_REVISION	= 0;

	public const string Name = "RTDESK Engine";

	private RTDESKMsgPool		MsgPool;
	private RTDESKMsgDispatcher MsgDispatcher;
	private RTDESKInputManager  InputManager;

	public enum Actions
    {
		SynchSim2RealTime
	}

	string GetVersion()
	{
		string aux = Name + " ";

		aux += RTDESK_VERSION;
		aux += ".";
		aux += RTDESK_SUBVERSION;
		aux += ".";
		aux += RTDESK_REVISION;

		return aux;
	}

	/**
		* @fn Awake()
		* Insert comment
	*/
	void Awake()
	{
		Reset();

		//Now set the timer manager with the default clocks set up. Now the MsgDispatcher can select the one it needs
		MsgDispatcher.ResetClocks();
		//Assign the "listener" to the normalized component RTDESKEntity. Every gameObject that wants to receive a message must have a public mailbox
		GetComponent<RTDESKEntity>().MailBox = ReceiveMessage;
		InputManager = GetComponent<RTDESKInputManager>();
	}

	void Start()
    {
		//This is the first message in the time stamped queue to manage
		//The aim is to synchronize every event to the real time just before any event management
		//to avoid start acceleration to match the simulation time to the real time if this synchronization is not done
		SendMsg(MsgPool.PopMsg((int)Actions.SynchSim2RealTime), gameObject, ReceiveMessage);
	}

    // Update is called once per frame
    void Update()
	{
		MsgContent Msg;
		
		while (GetRealTime() >= GetNextMsgTime())
		{
			Msg = MsgDispatcher.ReceiveMsg();
			//Debug.Log("Objeto " + Msg.Sender.name + " envía mensaje " + Msg.Type + " a objeto " + Msg.Sender.gameObject.name);

			try
            {
				Msg.Receiver(Msg);
			}
			catch (Exception e)
            {
				//Possibly, the object receiving this message has been destroyed and do not exist. Go on
				Debug.Log("Possibly, the receiver object does not exist");
				Debug.Log(e.Message);
				if (null != Msg)
				{
					Debug.Log("Pushing back the message to the pool");
					Debug.Log("Sender: " + Msg.Sender + " Type: " + Msg.Type);
                    PushMsg(Msg);
                }
				else Debug.Log("Null message");
            }			
		}
	}

	void OnDestroy(){Debug.Log("Destroying RTDESK Engine");}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool ObjectAdmitsMessages(GameObject o){return null != o.GetComponent<RTDESKEntity>();}

	void ReceiveMessage(MsgContent Msg)
	{
		switch (Msg.Type)
		{
			case (int)Actions.SynchSim2RealTime:
				//Sincronizar la cola de eventos pendientes de ejecución al instante actual en tiempo real antes de proceder a la simulación
				SynchSim2RealTime(0.1D);  //Se asigna una décima de segundo adicional por seguridad

				//Ya no se va a volver a gastar este tipo de mensaje. Devolver al pool
				PushMsg(Msg);
				break;
			default:
				break;
		}
	}

	/**
		* @fn Reset()
		* Insert comment
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void Reset()
	{
		//Do not change the order or deleting. Msg Dispatcher has to get empty to send all not propietary messages to the msg Pool before deleting the Pool
		MsgDispatcher	= new RTDESKMsgDispatcher();
		MsgPool			= new RTDESKMsgPool();

		//Update the reference to the MsgPool into the Dispatcher
		MsgDispatcher.MsgPool = MsgPool;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#nullable enable
	public static string? GetName<TEnum>(TEnum value) where TEnum : struct	{ return Enum.GetName(typeof(TEnum), value); }
#nullable disable

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RTDESKInputManager GetInputManager(){ return InputManager; }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int AmountOf<TEnum>() where TEnum : struct { return Enum.GetNames(typeof(TEnum)).Length; }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public MsgContent	PopMsg	(int type)		 {return MsgPool.PopMsg(type);}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void			PushMsg	(MsgContent msg) { MsgPool.PushMsg(msg); }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SynchSim2RealTime(double ms)			{ SynchSim2RealTime(ms2Ticks(ms)); }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SynchSim2RealTime(HRT_Time Ticks)		{ MsgDispatcher.SynchSim2RealTime(Ticks); }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double   Ticks2ms		(HRT_Time ticks){ return MsgDispatcher.Ticks2ms(ticks); }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public HRT_Time ms2Ticks		(double ms)		{ return MsgDispatcher.ms2Ticks(ms); }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public HRT_Time GetRealTime		()				{ return MsgDispatcher.GetRealTime(); }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public HRT_Time GetNextMsgTime	()				{ return MsgDispatcher.GetNextMsgTime(); }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public HRT_Time GetSimulTime	()				{ return MsgDispatcher.GetSimulTime(); }

	void EndSimulation()
    {

    }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	MsgContent ReceiveMsg() { return MsgDispatcher.ReceiveMsg(); }

	/**
	* @fn SendMsg()
	* Stores a user message into the time ordered data structure
	* @param Msg The data structure that holds the user data properly
	* @param Sender The gameObject that sends the message. The original gameObject of the message
	* @param Receiver The delegate method to manage the message sent
	* @param DeltaTime Amount of simulation time clock ticks to wait until the data is sent to the destination object
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SendMsg(MsgContent Msg, GameObject Sender, MessageManager Receiver, HRT_Time DeltaTime = HRTimer.HRT_ALMOST_INMEDIATELY)
	{
		Msg.Sender		= Sender;
		Msg.Receiver	= Receiver;
		MsgDispatcher.SendMsg(Msg, DeltaTime);
	}

	/**
	* @fn SendMsg()
	* Stores a user message into the time ordered data structure
	* @param Msg The data structure that holds the user data properly
	* @param Sender The gameObject that sends the message. The original gameObject of the message
	* @param Receiver The gameObject to manage the message sent
	* @param DeltaTime Amount of simulation time clock ticks to wait until the data is sent to the destination object
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SendMsg(MsgContent Msg, GameObject Sender, GameObject Receiver, HRT_Time DeltaTime = HRTimer.HRT_ALMOST_INMEDIATELY)
	{
		Msg.Sender		= Sender;
		Msg.Receiver	= Receiver.GetComponent<RTDESKEntity>().MailBox;
		MsgDispatcher.SendMsg(Msg, DeltaTime);
	}

	/**
	* @fn SendMsg()
	* Stores a user message into the time ordered data structure
	* @param Msg The data structure that holds the user data properly
	* @param Sender The gameObject that sends the message. The original gameObject of the message
	* @param DeltaTime Amount of simulation time clock ticks to wait until the data is sent to the destination object
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SendMsg(MsgContent Msg, GameObject Sender, HRT_Time DeltaTime = HRTimer.HRT_ALMOST_INMEDIATELY)
	{
		Msg.Sender		= Sender;
		Msg.Receiver	= Sender.GetComponent<RTDESKEntity>().MailBox;
		MsgDispatcher.SendMsg(Msg, DeltaTime);
	}

	/**
	* @fn SendMsg()
	* Stores a user message into the time ordered data structure
	* @param Msg The data structure that holds the user data properly
	* @param DeltaTime Amount of simulation time clock ticks to wait until the data is sent to the destination object
	* * It is assumed that the sender and the receiver remains equal. Tipically for self messages
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SendMsg(MsgContent Msg, HRT_Time DeltaTime = HRTimer.HRT_ALMOST_INMEDIATELY){MsgDispatcher.SendMsg(Msg, DeltaTime);}

	/**
	* @fn SendMsg()
	* Stores a user message into the time ordered data structure
	* @param Msg The data structure that holds the user data properly
	* @param DeltaTime Amount of simulation time clock ticks to wait until the data is sent to the destination object
	* * It is assumed that the sender and the receiver remains equal. Tipically for self messages
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SendMsg(MsgContent Msg) { MsgDispatcher.SendMsg(Msg, Msg.AbsoluteTime); }
}

