/**
 * RTDeskMsgDispatcher: RTDESKMsgDispatcher class.
 *
 * Copyright(C) 2022
 *
 * Prefix: RTDMD_

 * @Author:	Dr. Ramón Mollá Vayá
 * @Date:	11/2022
 * @Version: 1.0
 *
 */

#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

#define RTDMD_ORDERED_LIST

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;       //AggressivInlining (.Net 4.5)
using UnityEngine;

//----constantes y tipos-----
#if OS_MSWINDOWS
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

public class RTDESKMsgDispatcher
{
#if RTDMD_ORDERED_LIST
    SortedList MsgTimeOrderedBuffer = new SortedList();
#endif

	//Clocks
	HRTManager	HRTimerManager = new HRTManager();
	HRTimer		SystemClock;    //Shortcut for HRTimerManager.Timer[(int)RTDESK_TIMERS.RTDT_SYSTEM]

    //Reference to the Message Pool 
    public RTDESKMsgPool MsgPool;  //When a message has no destination available, send it back to the pool

    ///The current simulation time instant. Corresponds to the current msg
    HRT_Time SimulationTime,
             ///The very next simulation time. It belongs to the first event in the time ordered buffer
             NextMsgTime;

    // Start is called before the first frame update
    public RTDESKMsgDispatcher()
    {
        SystemClock = HRTimerManager.Timer[(int)RTDESK_TIMERS.RTDT_SYSTEM];
        resetTime();
        MsgTimeOrderedBuffer.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double   Ticks2ms        (HRT_Time ticks){ return SystemClock.Ticks2ms(ticks); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRT_Time ms2Ticks        (double ms)     { return SystemClock.ms2Ticks(ms); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRT_Time GetRealTime     ()              { return SystemClock.GetRealTime(); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRT_Time GetNextMsgTime  ()              { return NextMsgTime; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRT_Time GetSimulTime    ()              { return SimulationTime; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void     ResetClocks     ()              { SimulationTime = GetRealTime(); HRTimerManager.ResetSamplingFrequency(); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void resetTime       ()              { SimulationTime = GetRealTime(); NextMsgTime = HRTimer.HRT_TIME_INVALID; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRT_Time GetSamplingFreq     ()  { return SystemClock.GetSF(); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRT_Time GetSimulationTime   ()  { return SimulationTime; }

	///Inserts the msg into the msgbuffer and updates NextMsgTime if proceeds
	void InsertMessage(MsgContent Msg)
    {

    }

	bool DeleteMessage(MsgContent Msg)
    {
		return true;
    }

    ///Synchronizes all events time to the real time
    public void SynchSim2RealTime(double ms) { SynchSim2RealTime(ms2Ticks(ms)); }
    public void SynchSim2RealTime(HRT_Time ms)
    {
        MsgContent msg;
        HRT_Time DeltaTime, currentTime;

        if (0 != MsgTimeOrderedBuffer.Count)
        {
            //There is al least one MsgContent even waiting for scheduling
            DeltaTime = GetRealTime() - NextMsgTime + ms;

            for (int index = 0; index < MsgTimeOrderedBuffer.Count;index ++)
            {
                msg = (MsgContent)MsgTimeOrderedBuffer.GetByIndex(index);
            
                if (HRTimer.HRT_TIME_INVALID == msg.AbsoluteTime)
                    Debug.Log("Origen " + msg.Sender.name + " tipo de mensaje " + msg.Type  );
                msg.AbsoluteTime += DeltaTime;

                //Si cuelgan más mensajes debajo, hay que actualizarlos al mismo valor 
                currentTime = msg.AbsoluteTime;
                while (null != msg.NextMsg)
                {
                    msg = msg.NextMsg;
                    msg.AbsoluteTime = currentTime;
                }
            }
        }
    }


    ///Validates integrity of the queues content
    void Validate()
    {

    }

	///Sends all not propietary events to the Msgs Pool and return every propietary msg to its owner
	void Empty()
    { 

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool isEmpty() { return 0 == MsgTimeOrderedBuffer.Count; }

    public MsgContent ReceiveMsg()
    {
        MsgContent msg;

        if (isEmpty())
        {
            NextMsgTime = HRTimer.HRT_TIME_INVALID;
            return null;
        }

        msg = (MsgContent)MsgTimeOrderedBuffer.GetByIndex(0);
        SimulationTime = msg.AbsoluteTime;
        if (null == msg.NextMsg)
        {
            MsgTimeOrderedBuffer.RemoveAt(0);
            // Update NextMsgTime
            if (isEmpty())
                NextMsgTime = HRTimer.HRT_TIME_INVALID;
            else
                NextMsgTime = ((MsgContent)MsgTimeOrderedBuffer.GetByIndex(0)).AbsoluteTime;
        }
        else // NextMsgTime does not change
            MsgTimeOrderedBuffer.SetByIndex(0, msg.NextMsg);

        //Debug.Log("Simulation time after receiving message = " + SimulationTime + " Next Message Time = " + NextMsgTime);
        //Debug.Log("Sender = " + msg.Sender.name + " Message Type = " + msg.Type);

        msg.UnQueue();  //State free and no reference to other enqueued messages. Absolute time and message type is OK
        return msg;
    }

    public void SendMsg(MsgContent Msg, HRT_Time DeltaTime)
    {
        Msg.AbsoluteTime = SimulationTime + DeltaTime;

        try
        {
            if (HRTimer.HRT_INMEDIATELY == DeltaTime)
            {
                Msg.AbsoluteTime = GetSimulationTime();
                Msg.Receiver(Msg);
                return;
            }
        }
        //In case the method Receiver in the Msg.Receiver(Msg); instruction does not exist.
        //For instance, the object that has the receiver method has been destroyed
        catch (InvalidOperationException notImp)
        {
            Debug.Log("Error. Destinatario no existe");
            Debug.Log(notImp.Message);
            MsgPool.PushMsg(Msg);
        }

        try
        {
            if (MsgContent.STATE.FREE != Msg.State)
                Debug.Log("ERROR. El mensaje recibido para encolar en la lista de mensajes temporizados está en un estado : " + Msg.State);
            Msg.State = MsgContent.STATE.QUEUED;

            MsgTimeOrderedBuffer.Add(Msg.AbsoluteTime, Msg);
            if (Msg.AbsoluteTime < NextMsgTime)
                NextMsgTime = Msg.AbsoluteTime;
        }        
        //The ordering key is repeated. Sorted list does not admit duplicated keys
        //Store the msg as a queue. For instance, a LIFO
        catch (ArgumentException)
        {
            //Debug.Log(e.Message);
            int index = MsgTimeOrderedBuffer.IndexOfKey(Msg.AbsoluteTime);
            Msg.NextMsg = (MsgContent)MsgTimeOrderedBuffer.GetByIndex(index);
            MsgTimeOrderedBuffer.SetByIndex(index, Msg);
        }
    }
}
