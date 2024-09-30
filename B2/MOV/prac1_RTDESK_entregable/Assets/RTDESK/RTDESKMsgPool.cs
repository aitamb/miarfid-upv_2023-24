/**
 * RTDeskMSGPool: Pools of messages class
 * 
 * Copyright(C) 2022
 *
 * Prefix: RTDMPM_

 * @Author:	Dr. Ramón Mollá Vayá
 * @Date:	11/2022
 * @Version: 2.0
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;       //AggressivInlining (.Net 4.5)
using UnityEngine;

public class RTDESKMsgPool
{
	///Amount of new messages to reserve from the dynamic memory everytime the pool is exhausted
	const uint RTDESK_BUNCH_SIZE = 5;

	MsgContent[]		UserMsgPool;   ///<An array of lists where the user messages remain waiting for being used
	UserMessagesFactory MsgFactory;

    public RTDESKMsgPool()
    {
		MsgFactory = new UserMessagesFactory ();

		//deploy the Pool of user messages as indicated by the factory
		UserMsgPool = new MsgContent[MsgFactory.MsgAmount];
		DeleteAllMsg();
	}

	~RTDESKMsgPool() {DeleteAllMsg();}

	// Utilities 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool EmptyMsgPool	()			  { return null == UserMsgPool; }   //There are no pools availables
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool NotEmptyMsgPool()			  { return null != UserMsgPool; }   //There are pools availables 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool EmptyMsgPool	(int MsgType) { return null == UserMsgPool[MsgType]; }   //There are no pools availables

	//Stack management
	public MsgContent PopMsg (int MsgType)
    {
		MsgContent msg;

		//Debug.Log("Tamaño de la lista de mensajes " + MsgFactory.MsgAmount);
		if (MsgType >= MsgFactory.MsgAmount) MsgType = (int)UserMsgTypes.Action;
		if (EmptyMsgPool(MsgType)) GenerateNewMsgs(MsgType);

		msg = UserMsgPool[MsgType];
		UserMsgPool[MsgType] = msg.NextMsg;
		msg.Free();
		return msg;
	}

	//Stack management
	/**
	* @fn PushMsg
	* Stores a given MsgContent in its correspondent Pool
	* Premises: MsgType is never out of range and it is correctly updated in the message
	* @param 
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushMsg(MsgContent Msg)
	{
		if (MsgContent.STATE.FREE != Msg.State)
			Debug.Log("ERROR. El mensaje recibido para encolar en el pool de mensajes está en un estado : " + Msg.State);

		Msg.State	= MsgContent.STATE.STORED;
		Msg.NextMsg = UserMsgPool[Msg.Type];
		UserMsgPool[Msg.Type] = Msg; 
	}

	/**
	* @fn DeleteAllMsg
	* Deletes all MsgContent in a given Pool
	* Premises: MsgType is never out of range and it is correctly updated in the message
	* @param 
	*/
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DeleteAllMsg(uint id) { UserMsgPool[id] = null; }
	private void DeleteAllMsg()
    {
		int size = MsgFactory.MsgAmount;

		//deploy the Pool of user messages as indicated by the factory
		for (uint i = 0; i < size; i++)
			DeleteAllMsg(i);
	}

	private void GenerateNewMsgs(int MsgType)
	{
		for (int i = 0; i< RTDESK_BUNCH_SIZE; i++)
			PushMsg(MsgFactory.CreateMsg(MsgType));
	}
}

