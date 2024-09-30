/**
 * MsgContent: Basic Message class
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
using System.Runtime.CompilerServices;       //AggressivInlining (.Net 4.5)

using UnityEngine;

public delegate void MessageManager(MsgContent MC);

//Se crea una estructura que en realidad es un union en el que todos los posibles tipos de valores que se pueden almacenar en un mensaje
public class MsgContent
{
	public const int NO_TYPE = -1;
	public enum STATE
	{
		FREE,         ///< Msg is not stored in any queue and is waiting for its use o being reused
		STORED,       ///< The msg is stored in the MsgPool waiting for a new use. Only the pointer "Next" is used
		QUEUED,       ///< The msg is stored into the event queue.
		MAX_STATE	  ///< For accounting purposes only. This is not a real state
	};

	public int					Type;			///< Kind of message to store. Defines the structure of the data stored inside the msg pointer
	public STATE				State;			///< Different situations where the msg can be inserted in
	public HRT_Time				AbsoluteTime;	///< It contains the Absolute message time to be dispatched

	public GameObject			Sender;		///<GameObject that sends the message
	public MessageManager		Receiver;	///<Script that receives the message
	public MsgContent			NextMsg;	///<For enqueuing the msg into the messages pool. FIFO/LIFO queue	

	public MsgContent(int T) { Free(); Type = T;}
	public MsgContent()		 { Reset(); }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Reset() { Type = NO_TYPE; Free(); }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Free () { AbsoluteTime = HRTimer.HRT_TIME_INVALID; UnQueue(); }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void UnQueue() { State = STATE.FREE; NextMsg = null; }
};