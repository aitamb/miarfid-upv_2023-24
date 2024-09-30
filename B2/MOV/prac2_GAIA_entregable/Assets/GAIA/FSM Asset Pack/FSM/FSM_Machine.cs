using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GAIA{


    // <summary>
    // This class allows to go through a Finite Automaton
    // </summary>
    // <remarks></remarks>
	public class FSM_Machine {
	
		//FSM_Machine Class Attributes
	
		//COMMON attributes to all machines

		//<summary>Current FSM</summary>
		FA_Classic FSM;
		//<summary>Current State in current FSM</summary>
		State CurrentState;
		//<summary>List of Actions to do</summary>
		List <int> DoActions;
		//<summary>Chosen Transition in selection phase</summary>
		Transition t_aux;
		//<summary>Flag that determines a change of state</summary>
		bool change;
		//<summary>Character linked to this FSM_Machine</summary>
		System.Object Character;

		//Hierarchical feature

		//<summary>List of stacks of FSMs (used to remember the previous one)</summary>
		List<Stack<FA_Classic>> FSM_Stack;
		//<summary>List of stacks of States (used to remember the previous one)</summary>
		List<Stack<State>> SuperiorStack;

		//--------------------------------
		//STACK attributes
		//<summary>Flag that determines if a situation is stackable</summary>
		bool stackable;				
		//---------------------------------
	
		//PROBABILISTIC attributes
		//<summary>Random number to determine activation</summary>
		int RandomNumber;
		//<summary>Random object to generate random numbers</summary>	
		System.Random rnd;
		//---------------------------------
	
		//INERTIAL attributes
		//<summary>Inertial FSM timer</summary>
		Stopwatch inertialTimer;
		//<summary>Stores lastEvent value (used to control the timer)</summary>
		int lastEvent;
		//---------------------------------

		//STACK attributes
		//<summary>Control Stack used in FA_Stack</summary>
		List<Stack<State>> ControlStack;
    
		//FSM_CONCURRENT_STATES
		//<summary>Enabled States in this cycle</summary>
		List<State> EnabledStates;
		//<summary>Dictionary of execution credits</summary>
		Dictionary<int, int> StatesCredits;

		//---------------------------------
		//<summary>Flag to update enabled states in current cycle</summary>
		bool UpdateEnabled;
		//<summary>Max number of concurrent states</summary>
		int MaxEnabled;

		MethodInfo EventsRoutine;

		// <summary>
		// Initializes a new instance of the <see cref="T:FSM.FSM_Machine">FSM_Machine</see> class. 
		// </summary>
		// <param name="fsm">FA object</param>
		// <param name="character">Character that demands this FSM based on fsm param</param>
		// <remarks>FSM_Manager uses this method to initialize a machine</remarks>
		public FSM_Machine(FA_Classic fsm, System.Object character){
		Character 		= character;
		FSM 			= fsm;

		CurrentState	= FSM.getInitialState();
		MaxEnabled 		= 1;

		DoActions 		= new List<int>();
		FSM_Stack 		= new List<Stack<FA_Classic>>();
		ControlStack 	= new List<Stack<State>>();
		SuperiorStack 	= new List<Stack<State>>();
		EnabledStates 	= new List<State>();
			
		//Concurrent
		if(FSM.getTag() == (int)FA_Classic.FAType.CONCURRENT) {

			MaxEnabled = (FSM as FA_Concurrent_States).getMaxConcurrent();
			for(int i = 0; i<(FSM as FA_Concurrent_States).getMaxConcurrent(); i++){
				SuperiorStack.Add(new Stack<State>());
				FSM_Stack.Add(new Stack<FA_Classic>());
				FSM_Stack[i].Push(FSM);
				ControlStack.Add(new Stack<State>());
				ControlStack[i].Push(CurrentState);
			}
			EnabledStates = (FSM as FA_Concurrent_States).getInitials().ToList();
			StatesCredits = (FSM as FA_Concurrent_States).getCreditsDic();

		}else{//The others
			MaxEnabled = 1;
			SuperiorStack.Add(new Stack<State>());
			FSM_Stack.Add(new Stack<FA_Classic>());
			FSM_Stack[0].Push(FSM);
			ControlStack.Add(new Stack<State>());
			ControlStack[0].Push(CurrentState);

			EnabledStates.Add(FSM.getInitialState());
		}
		
		rnd 			= new System.Random();
		UpdateEnabled 	= false;
		inertialTimer 	= new Stopwatch();

		EventsRoutine = Character.GetType().GetMethod(FSM.getCallback(), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		}

		#region GET methods
		// <summary>
		// Get Current FSM
		// </summary>
		// <returns>FA object</returns>
		// <remarks></remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FA_Classic getFSM() { return FSM; }

		// <summary>
		// Get Current State (not valid for Concurrent States)
		// </summary>
		// <returns>State object</returns>
		// <remarks></remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public State getCurrentState() { return CurrentState; }

		// <summary>
		// Get Current States
		// </summary>
		// <returns>List of enabled states</returns>
		// <remarks></remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public List<State> getEnabledStates() { return EnabledStates; }

	#endregion

    // <summary>
    // Updates situation of current/s FSM
    // </summary>
    // <returns>A List of integer value actions to do</returns>
    // <remarks></remarks>
	public List<int> Update() {

		//List of actions is cleared to return another filled one.
		DoActions.Clear();

		//Copy of enabled States
		List<State> EnabledStatesCopy = EnabledStates.ToList();
		
		//Executed states in currentCycle
		List<State> onCycle = new List<State>();

		//Routine for this FSM_Machine
		List<int> Events = (List<int>) EventsRoutine.Invoke(Character, null);

		//Counter in next foreach
		int es = 0;
		//Browsing enabled states
		foreach(State s in EnabledStates){

			t_aux 		= null;
			change 		= false;
			stackable 	= false;
			FSM 		= FSM_Stack[es].Peek(); //FSM in top of FSM_stack;

			//FSM calls the method that has the pertinent events to current FSM 
			if(FSM_Stack[es].Count>1){
			 	EventsRoutine = Character.GetType().GetMethod(FSM.getCallback(), BindingFlags.Instance |BindingFlags.NonPublic | BindingFlags.Public);
				Events = (List<int>) EventsRoutine.Invoke(Character, null);
			}

			//Only one event by default
			int? CEvent = Events[0];


			if(CEvent==null) return null; //if there is not an event, this method returns the null value
		
			//According to FSM TAG, FSM_Machine works in one way or another 
			switch(FSM.getTag()){
			case (int)FA_Classic.FAType.CLASSIC:		//PROBABILISTIC LOGIC
				#region Classic machine logic

				#region Classic selection

				if(FSM.isProbabilistic()){//PROBABILISTIC (NO DETERMINISTIC FSM)
										
					foreach(Transition t in CurrentState.getTransitions()){
						RandomNumber = rnd.Next(99);
						List<Event> t_events = t.getEvents();	
						foreach(Event ev in t_events){
							if(ev.getEventTag()==CEvent){
								//Test if there's 
								if(RandomNumber<t.getProbability()){
									t_aux = t;
									if(CurrentState.getTag()!=t_aux.getFinal().getTag()){
										change=true;	
									}else{
										change=false;
									}
									break;
								}
							}
						}//end foreach events
						if(t_aux!=null) break;
					}//end foreach transitions of currentstate
				
				}else{//NOT PROBABILISTIC (DETERMINISTIC FSM)
					foreach(Transition t in CurrentState.getTransitions()){
						List<Event> t_events = t.getEvents();	
						foreach(Event ev in t_events){
							if(ev.getEventTag()==CEvent && CurrentState.getTag()==t.getFinal().getTag() && t_aux==null){ //exist to myself
								t_aux = t;
							}else if(ev.getEventTag()==CEvent && CurrentState.getTag()!=t.getFinal().getTag() && t_aux==null){ //exist but to another
								t_aux = t;
								change = true;
							}
						}
					}
				}
				#endregion

				#region Classic Update
				//Exist a change
				if(change){
					//Exist submachine
					if(t_aux.getFinal().getSubFA()!=null){

						DoActions.Add(CurrentState.getOutAction()); 		//Out action of currentState
						DoActions.Add(t_aux.getAction());					//Action of transition of triggered transition
						SuperiorStack[es].Push(t_aux.getFinal()); 			//Push currentState in superiorStack
						FSM_Stack[es].Push(t_aux.getFinal().getSubFA());	//Push current FSM in FSM_Stack

						//Do something if FSM is Concurrent_States
						if(t_aux.getFinal().getSubFA().getTag()== (int)GAIA.FA_Classic.FAType.CONCURRENT)
								{
							foreach	(State sta in (t_aux.getFinal().getSubFA() as FA_Concurrent_States).getInitials()){
								DoActions.Add(sta.getInAction());
							}
						}else{//Do something if not
							CurrentState = t_aux.getFinal().getSubFA().getInitialState();			//Update currentState to subinitial state
							DoActions.Add(CurrentState.getInAction());								//in action of new currentState
						}						
					}else{

						DoActions.Add(CurrentState.getOutAction()); //Out action of currentState
						DoActions.Add(t_aux.getAction());			//Action of transition of triggered transition
						CurrentState = t_aux.getFinal();			//Update currentState
						DoActions.Add(CurrentState.getInAction());	//In action of new currentState
					}
				}else{//No change
					if(t_aux == null){ //No transition to go
						//Check if there is a submachine
						if(SuperiorStack[es].Count > 0){			
							CurrentState = SuperiorStack[es].Pop();//CurrentState is updated to inmediately following one in SuperiorStack 
							FSM_Stack[es].Pop(); 					//The FSM that is in top of FSM_Stack is unstacked (updated in next loop repetition)
						}else{
							//By default
							DoActions.Add(CurrentState.getAction());
						}
					}else{
						//Loop to myself
						DoActions.Add(CurrentState.getAction());
					}
				}
				#endregion
				//restart auxiliar transition
				t_aux = null;
				#endregion
				break;
			case (int)FA_Classic.FAType.INERTIAL:		//INERTIAL LOGIC
				#region Inertial machine logic		
	
				//Check if the last event is the same that the new one. If not, FSM has to reset its timer.
				try{//try-catch to avoid inconsistence of first cycle
					if(CEvent!=lastEvent){
						inertialTimer.Reset();
					}
				}catch(Exception){}
				
				#region Inertial selection
				if(FSM.isProbabilistic()){//(INERTIAL-PROBABILISTIC FSM)

					foreach(Transition t in CurrentState.getTransitions()){
						RandomNumber = rnd.Next(99);
						List<Event> t_events = t.getEvents();	
						foreach(Event ev in t_events){
							if(ev.getEventTag()==CEvent){
								//if there's 
								if(RandomNumber<t.getProbability()){
									t_aux = t;
									if(CurrentState.getTag()!=t_aux.getFinal().getTag()){
										change=true;
										inertialTimer.Start();
									}else{
										change=false;
									}
									break;
								}
							}
						}//end foreach events
						if(t_aux!=null) break;
					}//end foreach transitions of currentstate
					
				}else{//(INERTIAL-DETERMINISTIC FSM)
					foreach(Transition t in CurrentState.getTransitions()){
						List<Event> t_events = t.getEvents();	
						foreach(Event ev in t_events){
							if(ev.getEventTag()==CEvent && CurrentState.getTag()==t.getFinal().getTag() && t_aux==null){ //exist to myself
								t_aux = t;
							}else if(ev.getEventTag()==CEvent && CurrentState.getTag()!=t.getFinal().getTag() && t_aux==null){ //exist but to another
								t_aux = t;
								change = true;
								inertialTimer.Start(); //Start inertial timer
							}
						}
					}
				}
				#endregion

				#region Inertial Update
				//There is a change
				if(change){
					//There is a submachine
					if(t_aux.getFinal().getSubFA()!=null){

						//Check latency of current state
						if(inertialTimer.ElapsedMilliseconds > CurrentState.getLatency()){
							DoActions.Add(CurrentState.getOutAction()); 									//Out action of currentState
							DoActions.Add(t_aux.getAction());												//Action of transition of triggered transition
							SuperiorStack[es].Push(t_aux.getFinal()); 										//Push currentState in superiorStack
							FSM_Stack[es].Push(t_aux.getFinal().getSubFA());							//Push current FSM in FSM_Stack
							//For Concurrent States
							if(t_aux.getFinal().getSubFA().getTag()== (int)GAIA.FA_Classic.FAType.CONCURRENT)
									{
								foreach	(State sta in (t_aux.getFinal().getSubFA() as FA_Concurrent_States).getInitials()){
									DoActions.Add(sta.getInAction());
								}
							}else{
								CurrentState = t_aux.getFinal().getSubFA().getInitialState();				//Update currentState to subinitial state
								DoActions.Add(CurrentState.getInAction());									//In action of new currentState
							}

							inertialTimer.Stop(); inertialTimer.Reset();									//Stop and reset the timer
						}else{
							//By default
							DoActions.Add(CurrentState.getAction());
						}
					}else{ //No change

						if(inertialTimer.ElapsedMilliseconds > CurrentState.getLatency()){
							DoActions.Add(CurrentState.getOutAction()); 	//Out action of currentState
							DoActions.Add(t_aux.getAction());				//Action of transition of triggered transition
							CurrentState = t_aux.getFinal();				//Update currentState
							DoActions.Add(CurrentState.getInAction());		//In action of new currentState
							inertialTimer.Stop(); inertialTimer.Reset();	//Stop and reset the timer
						}else{
							//By default
							DoActions.Add(CurrentState.getAction());
						}
					}
				}else{
					if(t_aux == null){ //No transition to go
						//Check if there is a submachine
						if(SuperiorStack[es].Count > 0){			
							CurrentState = SuperiorStack[es].Pop();	//CurrentState is updated to inmediately following one in SuperiorStack 
							FSM_Stack[es].Pop(); 					//The FSM that is in top of FSM_Stack is unstacked (updated in next loop repetition)
						}else{
							//By default
							DoActions.Add(CurrentState.getAction());
						}
					}else{
						//By default
						DoActions.Add(CurrentState.getAction());		
					}
				}
				#endregion

				//Store last event
				lastEvent = (int)CEvent;
				//Restart auxiliar transition
				t_aux = null;			
				#endregion
				break;
			case (int)FA_Classic.FAType.STACK_BASED:	//TO THIS FSM, IT IS NECESSARY THAT EVERY STATE HAS A TRANSITION TO ITSELF****************
				#region Stack-based machine logic 

				#region Stack-based selection
				if(FSM.isProbabilistic()){//(STACK-PROBABILISTIC FSM)
										
					foreach(Transition t in CurrentState.getTransitions()){
						RandomNumber = rnd.Next(99);
						List<Event> t_events = t.getEvents();	
						foreach(Event ev in t_events){
							if(ev.getEventTag()==CEvent){
								//Test if there's 
								if(RandomNumber<t.getProbability()){
									t_aux = t;
									if(CurrentState.getTag()!=t_aux.getFinal().getTag()){
										change=true;
										if(Event.EventType.STACKABLE == ev.getEventType() && (t.getFinal().getPriority() > CurrentState.getPriority())) //if EVENT IS STACKABLE
											stackable = true;
									}else{
										change=false;
									}
									break;
								}
							}
						}//end foreach events
						if(t_aux!=null) break;
					}//end foreach transitions of currentstate
					
				}else{//(STACK-DETERMINISTIC FSM)
					foreach(Transition t in CurrentState.getTransitions()){
						List<Event> t_events = t.getEvents();	
						foreach(Event ev in t_events){
							if(ev.getEventTag()==CEvent && CurrentState.getTag()==t.getFinal().getTag() && t_aux==null){ //Exist but to myself
								t_aux = t;
							}else if(ev.getEventTag()==CEvent && CurrentState.getTag()!=t.getFinal().getTag() && t_aux==null){ //Exist but to another
								t_aux = t;
								change = true;
								if(Event.EventType.STACKABLE == ev.getEventType() && (t.getFinal().getPriority() > CurrentState.getPriority())) //if EVENT IS STACKABLE
									stackable = true;
							}
						}
					}
				}	
				#endregion
				#region Stack-based Update
				//UPDATE FSM
				if(change){ //If there is a change of State
					if(stackable){//EVENT THAT COMES CAN STACK CURRENTSTATE
						//Check if destination State has a submachine
						if(t_aux.getFinal().getSubFA()!=null){ 
							//If there is a submachine, makes Push() in SuperiorState and in FSM_Stack (Hierarchical) and Pop() and Push in ControlStack (Stack-based)
							DoActions.Add(CurrentState.getOutAction()); 								//out action of currentState
							DoActions.Add(t_aux.getAction());											//action of transition of triggered transition
							SuperiorStack[es].Push(t_aux.getFinal()); 											//Push currentState in superiorStack
							FSM_Stack[es].Push(t_aux.getFinal().getSubFA());							//Push current FSM in FSM_Stack

							if(t_aux.getFinal().getSubFA().getTag()== (int)GAIA.FA_Classic.FAType.CONCURRENT)
									{
								foreach	(State sta in (t_aux.getFinal().getSubFA() as FA_Concurrent_States).getInitials()){
									DoActions.Add(sta.getInAction());
								}		
							}else{
								CurrentState = t_aux.getFinal().getSubFA().getInitialState();				//Update currentState to subinitial state
								DoActions.Add(CurrentState.getInAction());									//in action of new currentState
							}
							//This FSM is Stack-based:
							//stackable
							ControlStack[es].Push(CurrentState);			//Push the new one
						}else{ //if not, machine updates position giving 3 actions (out/transition/in), makes Pop() and Push() in ControlStack (Stack-based)
							DoActions.Add(CurrentState.getOutAction()); 	//Out action of currentState
							DoActions.Add(t_aux.getAction());				//Action of transition of triggered transition
							CurrentState = t_aux.getFinal();				//Update currentState
							DoActions.Add(CurrentState.getInAction());		//In action of new currentState
							//This FSM is Stack-based:
							//stackable
							ControlStack[es].Push(CurrentState);			//Push the new one
						}	
					}else{
						if(t_aux.getFinal().getSubFA()!=null){ 
							//If there is a submachine, makes Push() in SuperiorState and in FSM_Stack (Hierarchical) and Pop() and Push in ControlStack (Stack-based)
							DoActions.Add(CurrentState.getOutAction()); 								//Out action of currentState
							DoActions.Add(t_aux.getAction());											//Action of transition of triggered transition
							SuperiorStack[es].Push(t_aux.getFinal()); 									//Push currentState in superiorStack
							FSM_Stack[es].Push(t_aux.getFinal().getSubFA());						//Push current FSM in FSM_Stack

							//For concurrent_states
							if(t_aux.getFinal().getSubFA().getTag()== (int)GAIA.FA_Classic.FAType.CONCURRENT)
									{
								foreach	(State sta in (t_aux.getFinal().getSubFA() as FA_Concurrent_States).getInitials()){
									DoActions.Add(sta.getInAction());
								}		
							}else{
								CurrentState = t_aux.getFinal().getSubFA().getInitialState();				//Update currentState to subinitial state
								DoActions.Add(CurrentState.getInAction());									//In action of new currentState
							}
							//This FSM is Stack-based:
							ControlStack[es].Pop ();						//Pop top state 
							ControlStack[es].Push(CurrentState);			//Push the new one
						}else{ //if not, machine updates position giving 3 actions (out/transition/in), makes Pop() and Push() in ControlStack (Stack-based)
							DoActions.Add(CurrentState.getOutAction()); //out action of currentState
							DoActions.Add(t_aux.getAction());			//action of transition of triggered transition
							CurrentState = t_aux.getFinal();			//Update currentState
							DoActions.Add(CurrentState.getInAction());	//in action of new currentState
							//This FSM is Stack-based:
							//No stackable
							ControlStack[es].Pop();						//Pop top state
							ControlStack[es].Push(CurrentState);			//Push the new one
						}
					}
				}else{ //If there is not a change
						
					//PRIORITY OF PERFORMANCE IN THIS FMS: 	** ControlStack > SuperiorStack > Default **
					if(ControlStack[es].Count>1){	//If(ControlStack > 1) means that some State has interrupted previously
						if(t_aux==null){ //FSM did not find a transition 
							ControlStack[es].Pop();						//CurrentState is unstacked and FSM passes to check the last State in ControlStack
							CurrentState = ControlStack[es].Peek();		//Update CurrentState
							DoActions.Add(CurrentState.getAction());//FSM has to return CurrentState action
						}else{					//FSM found a transition (to myself)
							//FSM has to return CurrentState action
							DoActions.Add(CurrentState.getAction());
						}
					}else{			//if there is not a transition, the FSM tries to find a potentially activation of a prioritary State with the CEvent
						//Check if FSM is in a Submachine
						if(SuperiorStack[es].Count > 0){ //if it is...	
							if(t_aux==null){
								CurrentState = SuperiorStack[es].Pop();//CurrentState is updated to inmediately following one in SuperiorStack 
								FSM_Stack[es].Pop(); 					//The FSM that is in the top of FSM_Stack is unstacked (updated in the next program cycle)
							}else{						//if not
								//FSM has to return CurrentState action
								DoActions.Add(CurrentState.getAction());
							}
						}else{
							//UNKNOWN, by default
							DoActions.Add(CurrentState.getAction()); 
						}
					}
				}
	
				#endregion

				#endregion
				break;
				//...///
			case (int)FA_Classic.FAType.CONCURRENT:
				#region Concurrent-states machine logic
					
				//Foreach of events (this FSM_Machine allows to check more than one)
				foreach (int CurrentEvent in Events){	
					t_aux  = null;
					change = false;

					//Check if currentState can execute its logic or if it has spent its credits on current cycle
					if(StatesCredits[EnabledStatesCopy[es].getTag()]>0){
						#region Concurrent-states Selection
						if(FSM.isProbabilistic()){//(CONCURRENT-PROBABILISTIC FSM)
							
							foreach(Transition t in EnabledStatesCopy[es].getTransitions()){
								RandomNumber = rnd.Next(99);
								List<Event> t_events = t.getEvents();	
								foreach(Event ev in t_events){
									if(ev.getEventTag()==CEvent){
										//Test if there's 
										if(RandomNumber<t.getProbability()){
											t_aux = t;
											if(EnabledStatesCopy[es].getTag()!=t_aux.getFinal().getTag()){
												change=true;	
											}else{
												change=false;
											}
											break;
										}/*else{
											Accumulate += t.getProbability();
										}*/
									}
								}//end foreach events
								if(t_aux!=null) break;
							}//end foreach transitions of currentstate
							
						}else{//(CONCURRENT-DETERMINISTIC FSM)
							foreach(Transition t in EnabledStatesCopy[es].getTransitions()){
									foreach(Event ev in t.getEvents ()){
										if(ev.getEventTag()==CurrentEvent && EnabledStatesCopy[es].getTag()==t.getFinal().getTag()){ //exist but to myself
											t_aux = t;
											break;
										}else if(ev.getEventTag()==CurrentEvent && EnabledStatesCopy[es].getTag()!=t.getFinal().getTag()){//exist but to another	
											t_aux = t;
											change = true;
											break;
										}
									}
								if(t_aux!=null) break;
							}	
						}
						#endregion

						#region Concurrent-states Update
						//There is a change
						if(change){
							if(!onCycle.Contains(t_aux.getFinal())){ //Destination is not onCycle
								int CurrentCredits = StatesCredits[EnabledStatesCopy[es].getTag()];
								//If I've got more than one credit, or I've got one and I'm not on current cycle...
								if(StatesCredits[EnabledStatesCopy[es].getTag()]>1 || (StatesCredits[EnabledStatesCopy[es].getTag()]==1 && !onCycle.Contains(EnabledStatesCopy[es]))){
									if(t_aux.getFinal().getSubFA()!=null){
										DoActions.Add(EnabledStatesCopy[es].getOutAction()); 												//out action of currentState
										StatesCredits[EnabledStatesCopy[es].getTag()] = StatesCredits[EnabledStatesCopy[es].getTag()] - 1;	//CurrentState LOSES one credit

										DoActions.Add(t_aux.getAction());																	//action of transition of triggered transition
										SuperiorStack[es].Push(t_aux.getFinal()); 															//Push currentState in superiorStack[i]
										FSM_Stack[es].Push(t_aux.getFinal().getSubFA());												//Push current FSM in FSM_Stack[i]
									
										StatesCredits[t_aux.getFinal().getTag()] = StatesCredits[t_aux.getFinal().getTag()] + 1;			//CurrentState WINS one credit

										//For concurrent_states
										if(t_aux.getFinal().getSubFA().getTag()== (int)GAIA.FA_Classic.FAType.CONCURRENT)
												{
											foreach	(State sta in (t_aux.getFinal().getSubFA() as FA_Concurrent_States).getInitials()){
												DoActions.Add(sta.getInAction());
											}	
										}else{
											EnabledStatesCopy[es] = t_aux.getFinal().getSubFA().getInitialState();				//Update currentState to subinitial state
											DoActions.Add(EnabledStatesCopy[es].getInAction());									//in action of new currentState
										}
											
										onCycle.Add(t_aux.getFinal());		//FSM has processed the destination once in this cycle, so it is added to OnCycle list.
									}else{
										DoActions.Add(EnabledStatesCopy[es].getOutAction());												//Out action of currentState
										StatesCredits[EnabledStatesCopy[es].getTag()] = StatesCredits[EnabledStatesCopy[es].getTag()] - 1;	//CurrentState LOSES one credit
										DoActions.Add(t_aux.getAction());																	//Action of transition of triggered transition
										EnabledStatesCopy[es] = t_aux.getFinal();															//Update currentState
										StatesCredits[EnabledStatesCopy[es].getTag()] = StatesCredits[EnabledStatesCopy[es].getTag()] + 1;	//CurrentState WINS one credit
										DoActions.Add(EnabledStatesCopy[es].getInAction());													//In action of new currentState
											
										onCycle.Add(t_aux.getFinal());		//FSM has processed the destination once in this cycle, so it is added to OnCycle lis
									}
								}
							}
						}else{
							if(t_aux == null){ //No transition to go
								//Check if there is a submachine
								if(SuperiorStack[es].Count > 0){			
									EnabledStatesCopy[es] = SuperiorStack[es].Pop();//CurrentState is updated to inmediately following one in SuperiorStack 
									FSM_Stack[es].Pop(); 							 //The FSM that is in top of FSM_Stack is unstacked (updated in next loop repetition)
								}else{//DEFAULT
									if(!onCycle.Contains(EnabledStatesCopy[es])){
										onCycle.Add(EnabledStatesCopy[es]);
										DoActions.Add(EnabledStates[es].getAction());
									}
								}
							}else{//DEFAULT
								if(!onCycle.Contains(EnabledStatesCopy[es])){
									onCycle.Add(EnabledStatesCopy[es]);
									DoActions.Add(EnabledStates[es].getAction());
								}
							}
						}
						#endregion
					}else{//If currentState has not got any credits, its has to stop its execution in this cycle
						break;
					}
				}//end FOREACH EVENTS
					
				UpdateEnabled = true;
				#endregion
				break;
			} //END Switch FSM.tag

			//UPDATE COUNTER
			es++;
		}//END Loop for EnabledStates
		//*********************************************************************************
		
		//Concurrent Machine has to update Enabled States List
		if(UpdateEnabled){
			UpdateEnabled = false;
			EnabledStates.Clear();		
			EnabledStates = onCycle.ToList();
			if(EnabledStates.Count == 0){
				//foreach(State s in FSM.getStatesList()){
				foreach(KeyValuePair<int, int> entry in StatesCredits)
				{
					// do something with entry.Value or entry.Key	
					if(entry.Value > 0) 
						EnabledStates.Add(FSM.getStateByTag(entry.Key));
				
				}
			}

		}
		//Here we could browse the list of actions:
		/*(...)*/

		//FSM_Machine returns a list of actions to its assigned object (usually a character)
		return DoActions;
	}

	}	
}

