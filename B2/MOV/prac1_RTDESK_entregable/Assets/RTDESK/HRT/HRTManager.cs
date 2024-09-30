/**
 * HRTManager: High Resolution Timers Manager class
 * 
 * Copyright(C) 2022
 *
 * Prefix: HRTM_

 * @Author:	Dr. Ramón Mollá Vayá
 * @Date:	11/2022
 * @Version: 2.0
 *
 */

//----conversion de tipos de C++ a C#----
/*
INT64_MAX            Int64.Maxvalue   8 Bytes
long long            Int64            8 Bytes
unsigned char        Byte             1 Byte
unsigned short int   UInt16           2 Bytes
unsigned int         uint, UInt32     4 bytes
double               Double           8 Bytes
*/
//---------------------------------------

#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

#if !RTTIMER_H
#define RTTIMER_H
#define RTT_MM_COUNTERS         //QPC-Stopwatch
//#define RTT_TIME_STAMP_ASM    //HRTdll
#endif

#if !HRTIMER_H
#define HRTIMER_H
#endif

#if !HRTIMERM_H
#define HRTIMERM_H
#endif

//----bibliotecas Unity3d----
using UnityEngine;
using System.Collections;
//------------------------

//----bibliotecas HRT----
using System;                                //Int64
using System.Diagnostics;                    //Stopwatch
using System.Collections.Generic;            //List
//using System.Runtime.InteropServices;        //DllImport
//using System.Runtime.CompilerServices;       //AggressivInlining (.Net 4.5)
//-----------------------


//----constantes y tipos-----
#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

public enum RTDESK_TIMERS
{
	//Insert here as many timers as needed
	RTDT_DEFAULT,
	RTDT_GLOBAL,
	RTDT_SYSTEM,
	RTDT_MAX_TIMERS
}

public enum RTDT_PREDEFINED_TIMES
{
	RTDT_INMEDIATELY,   //When delivering events just right now. Maximun priority and Store Slack Time is allowed
	RTDT_1MS,           //1 ms
	RTDT_120FPS,        //120 frames per second
	RTDT_100FPS,        //100 frames per second
	RTDT_60FPS,         //60 frames per second
	RTDT_30FPS,         //30 frames per second
	RTDT_25FPS,         //25 frames per second
	RTDT_12FPS,         //12 frames per second
	RTDT_10FPS,         //10 frames per second
	RTDT_1HZ,           //Once per second
	RTDT_MAX_PREDEFINED_TIMES
}

#if HRTIMERM_H
public class HRTManager : HRTimer
{
	HRT_Time[] PredefinedPeriods;

	public List<HRTimer> Timer = new List<HRTimer>();

	//----constantes----
	//const en C# es const y static
	public const int HRTM_NO_SAMPLING_FREQUENCY = -1;
	public const int HRTM_NO_TIMER_CREATED		= -1;
	//------------------

	public enum HRTM_ERRORS {
		HRTM_NO_ERROR,
		HRTM_NO_TIMERS_CREATED,
		HRTM_MAX_ERRORS
	}

	private List<string> HRTM_ErrorMsg = new List<string>(){
		"No error", //Corresponds to HRTM_NO_ERROR error
		"No timer has been possible to be created"	//Corresponds to HRTM_NO_TIMERS_CREATED error
	};

	private List<string> TimersNames = new List<string>(){
		"Por defecto",	//Corresponds to RTDT_DEFAULT timer
		"Global",		//Corresponds to RTDT_GLOBAL timer
		"Sistema"		//Corresponds to RTDT_SYSTEM timer
	};

	//Methods
	//constructor
	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public HRTManager(){
		DestroyTimers();
		CreateTimers((int)RTDESK_TIMERS.RTDT_MAX_TIMERS) ;
		SetTimersName(TimersNames);
		InitPredefinedTimes();
	}

	//destructor
	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	~HRTManager(){
		DestroyTimers();
	}

	/*
	public void listResize (List<HRTimer> list, int size){
		if (list != null) {
			if (size > list.Count)
				for (int i = 0; i <= size - list.Count; i++)
					list.Add (new HRTimer());
			else if (size < list.Count)
				for (int i = 0; i <= list.Count - size; i++)
					list.RemoveAt (list.Count - 1);
		}
	}
	*/

	void InitPredefinedTimes()
	{
		double FPS;

		PredefinedPeriods = new HRT_Time[(int)RTDT_PREDEFINED_TIMES.RTDT_MAX_PREDEFINED_TIMES];

		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_INMEDIATELY] = 1;   //When delivering events just right now. Maximun priority and Store Slack Time is allowed
		FPS = 0.001D;
		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_1MS]		= (HRT_Time)(((double)GetSF())*FPS);            //1 ms
		FPS = 1.0D/120.001D;
		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_120FPS]	= (HRT_Time)(((double)GetSF()) * FPS);          //120 frames per second
		FPS = 1.0D / 100.001D;
		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_100FPS]	= (HRT_Time)(((double)GetSF()) * FPS);          //100 frames per second
		FPS = 1.0D / 60.001D;
		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_60FPS]	= (HRT_Time)(((double)GetSF()) * FPS);           //60 frames per second
		FPS = 1.0D / 30.001D;
		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_30FPS]	= (HRT_Time)(((double)GetSF()) * FPS);           //30 frames per second
		FPS = 1.0D / 25.001D;
		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_25FPS]	= (HRT_Time)(((double)GetSF()) * FPS);           //25 frames per second
		FPS = 1.0D / 12.001D;
		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_12FPS]	= (HRT_Time)(((double)GetSF()) * FPS);           //12 frames per second
		FPS = 1.0D / 10.001D;
		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_10FPS]	= (HRT_Time)(((double)GetSF()) * FPS);			//10 frames per second
		FPS = 1.0D;
		PredefinedPeriods[(int)RTDT_PREDEFINED_TIMES.RTDT_1HZ]		= (HRT_Time)(((double)GetSF()) * FPS);			//1 frames per second
	}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CreateTimers(int T) {
		//listResize(Timers, T);

		for (int i = 0; i < T; i++)
			Timer.Add(new HRTimer());
		ResetSamplingFrequency();
	}

	public void SetTimersName (List<string> TimerNames){
		for (int i = 0; i < Timer.Count; i++)
			Timer[i].SetName(TimerNames[i]);
	}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ErrorMsg(HRTM_ERRORS E){
		return HRTM_ErrorMsg[(int)E];
	}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void DestroyTimers(){
		if(Timer != null) Timer.Clear();
		Name = "";
	}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool SamplingFrequencyAvailable(){
		return HRTM_NO_SAMPLING_FREQUENCY != GetSF();
	}

	public void ResetSamplingFrequency(){
		//UnityEngine.Debug.Log("Frecuencia de reloj del sistema " +  Stopwatch.Frequency);
		SetSF(Stopwatch.Frequency);
		int i, size;
		size = Timer.Count;
		if (size > 0){
			for (i = 0; i < size; i++)
				Timer[i].ResetSamplingFrequency(GetSF());
		}
	}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ResetAllTimers(){
		for(int i = 0; i < Timer.Count; i++)
			Timer[i].Reset();
	}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string GetName(RTDESK_TIMERS t) {return Timer[(int)t].Name;}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetSamplingFrequency(RTT_Time SF){SamplingFrequency = SF;}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void UpdateWindow(RTT_Time SF){
		int i, size = Timer.Count;
		for(i = 0; i < size; i++)
			Timer[i].SetSF(SF);
	}
}
#endif
