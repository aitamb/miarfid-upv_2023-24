using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace GAIA{

    // <summary>
    // State object class
    // </summary>
    // <remarks></remarks>
    public class State {

        public const int NO_PRIORITY = -1;
        public const int NO_LATENCY  = -1;
        public const int NO_CREDITS  = -1;

        //State Class attributes
        //<summary>State's name (string identifier)	 </summary>								
        string stateName;
        //<summary>State tag identifier</summary>
        int stateId,
        //<summary>Action tag identifier of this State</summary>
            action,
        //<summary>IN Action tag identifier of this State </summary>
            in_action,
        //<summary>OUT Action tag identifier of this State </summary>
            out_action;
        //<summary>Flag that determines if this state is initial </summary>
        public bool initial { get; }

        //<summary>List of this state's output transitions to other states</summary>	
	    protected List<Transition> Transitions;

	    //Class attributes for hierarchical aspect in FAs
        //<summary>Object to attach a submachine</summary>
	    protected FA_Classic subFA;

	    //Class attributes for STACK_BASED FA
        //<summary>Priority of this state. Only used in FA_Stack</summary>
	    protected uint priority;

	    //Class attributes for INERTIAL FA
        //<summary>Latency of this state. Only used in FA_Inertial </summary>
	    protected int latency;
		
	    //Class attributes for CONCURRENT STATES FA (based on Petri's net)
        //<summary>State execution credits. Only used in FA_Concurrent_States </summary>
	    protected short credits;
	
	    //Create a new State
	    //Constructor for a classic fsm state
        // <summary>
        // Initializes a new instance of the <see cref="T:FSM.State">State</see> class. 
        // </summary>
        // <param name="ID">Name of this State</param>
        // <param name="FlagInitial">Flag that determines if it is initial ("YES") or it is not ("NO")</param>
        // <param name="state_tag">State tag identifier</param>
        // <param name="action_tag">Action tag identifier of this state</param>
        // <param name="in_action_TAG">IN action tag identifier of this state</param>
        // <param name="out_action_TAG">OUT action tag identifier of this state</param>
        // <remarks>It can be used in every FA</remarks>
	    public State(string ID, bool FlagInitial, int action_tag, int in_action_TAG, int out_action_TAG){
		    stateName = ID;
		    initial = FlagInitial;
		    Transitions = new List<Transition>();
            stateId = (int)Tags.name2Tag<Tags.StateTags>(stateName);
            action = action_tag;
		    this.in_action = in_action_TAG;
		    this.out_action = out_action_TAG;
	    }

	    //Constructor for a stack_based fsm state
        // <summary>
        // Initializes a new instance of the <see cref="T:FSM.State">State</see> class. 
        // </summary>
        // <param name="ID">Name of this State</param>
        // <param name="FlagInitial">Flag that determines if it is initial ("YES") or it is not ("NO")</param>
        // <param name="state_tag">State tag identifier</param>
        // <param name="action_tag">Action tag identifier of this state</param>
        // <param name="in_action_TAG">IN action tag identifier of this state</param>
        // <param name="out_action_TAG">OUT action tag identifier of this state</param>
        // <param name="priority">Priority of this State. It has to be >=0 </param>
        // <remarks>Only used in FA_Stack</remarks>
	    public State(string ID, bool FlagInitial, int action_tag, int in_action_TAG, int out_action_TAG, uint priority){
		    stateName = ID;
		    initial = FlagInitial;
		    Transitions = new List<Transition>();
		    stateId = (int)Tags.name2Tag<Tags.StateTags>(stateName);
		    action = action_tag;
		    this.in_action = in_action_TAG;
		    this.out_action = out_action_TAG;
		    this.priority = priority;
	    }
	    //Constructor of an inertial fsm state
        // <summary>
        // Initializes a new instance of the <see cref="T:FSM.State">State</see> class. 
        // </summary>
        // <param name="ID">Name of this State</param>
        // <param name="FlagInitial">Flag that determines if it is initial ("YES") or it is not ("NO")</param>
        // <param name="state_tag">State tag identifier</param>
        // <param name="action_tag">Action tag identifier of this state</param>
        // <param name="in_action_TAG">IN action tag identifier of this state</param>
        // <param name="out_action_TAG">OUT action tag identifier of this state</param>
        // <param name="latency">Latency of this state</param>
        // <remarks>Only used in FA_Inertial</remarks>
	    public State(string ID, bool FlagInitial, int action_tag, int in_action_TAG, int out_action_TAG, int latency){
		    stateName = ID;
		    initial = FlagInitial;
		    Transitions = new List<Transition>();
		    stateId = (int)Tags.name2Tag<Tags.StateTags>(stateName);
            action = action_tag;
		    this.in_action = in_action_TAG;
		    this.out_action = out_action_TAG;
		    this.latency = latency;
	    }
	    //Constructor of a Concurrent-states fsm state
        // <summary>
        // Initializes a new instance of the <see cref="T:FSM.State">State</see> class. 
        // </summary>
        // <param name="ID">Name of this State</param>
        // <param name="FlagInitial">Flag that determines if it is initial ("YES") or it is not ("NO")</param>
        // <param name="state_tag">State tag identifier</param>
        // <param name="action_tag">Action tag identifier of this state</param>
        // <param name="in_action_TAG">IN action tag identifier of this state</param>
        // <param name="out_action_TAG">OUT action tag identifier of this state</param>
        // <param name="credits">Credits of execution of this state</param>
        // <remarks>Only used in FA_Concurrent_States</remarks>
	    public State(string ID, bool FlagInitial, int state_tag, int action_tag, int in_action_TAG, int out_action_TAG, short credits){
		    stateName = ID;
		    initial = FlagInitial;
		    Transitions = new List<Transition>();
		    stateId = state_tag;
		    action = action_tag;
		    this.in_action = in_action_TAG;
		    this.out_action = out_action_TAG;
		    this.credits = credits;
	    }
	
	    #region ADD METHODS
	
        // <summary>
        // Adds a submachine to this State or changes the one that is already attached 
        // </summary>
        // <param name="subFA"> FA_Classic object (any FA is valid)</param>
        // <returns>
        // true if OK
        // false if the SubFA cannot be attached
        //</returns>
        // <remarks></remarks>
	    public bool addFA(FA_Classic subFA){
		    try{
			    this.subFA = subFA;
			    return true;
		    }catch(Exception){
			    return false;
		    }
	    }
	
        // <summary>
        // Adds a transition to this state if It is possible
        // </summary>
        // <param name="newTransition">Transition object</param>
        // <returns>
        // true if OK
        // false if error. (Transition has already been added to this State or wrong parameter)
        //</returns>
        // <remarks></remarks>
	    public bool addTransition(Transition newTransition){
		    if(Transitions.Contains(newTransition)){
			    return false;
			    //Debug.LogError("State.cs - This state has already the transition");
		    }else{
			    Transitions.Add(newTransition);
			    return true;
		    }
	    }
        #endregion

        #region GET METHODS

        // <summary>
        // Get the tag assigned to this state
        // </summary>
        // <returns>int value</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int getTag() { return stateId; }

        // <summary>
        // Get the ID that identifies this state
        // </summary>
        // <returns>string value</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string getID() { return stateName; }


        // <summary>
        // Get latency if it exists
        // </summary>
        // <returns>
        // Latency value if OK
        // NO_LATENCY if error. This State has not got latency
        //</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int getLatency() { if(0 <= latency) return latency; else return NO_LATENCY; }

        // <summary>
        // Get priority if it exists
        // </summary>
        // <returns>
        // Priority value if OK
        // NO_PRIORITY if error. This State has not got priority
        //</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int getPriority(){ if(0 <= priority) return (int)priority; else return NO_PRIORITY; }

        // <summary>
        // Get credits if they exist
        // </summary>
        // <returns>
        // Credits value if OK
        // NO_CREDITS if error. This State has not got execution credits
        //</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short getCredits() { if(0 <= credits) return credits; else return NO_CREDITS; }

        // <summary>
        // Get subFA if it exists
        // </summary>
        // <returns>
        // SubFA value if OK
        // Null value if error. There is not a subFA attached to this State
        //</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FA_Classic getSubFA() { if(subFA!=null) return subFA; else return null; }

        // <summary>
        // Get the list of transitions of this state
        // </summary>
        // <returns>List of transitions</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<Transition> getTransitions() { return Transitions; }

        // <summary>
        // Get the action tag assigned to this state
        // </summary>
        // <returns>tag value</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int getAction() { return action; }

        // <summary>
        // Get the IN action tag assigned to this state
        // </summary>
        // <returns>IN tag value</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int getInAction() { return in_action; }

        // <summary>
        // Get the OUT action tag assigned to this state
        // </summary>
        // <returns>OUT tag value</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int getOutAction() { return out_action; }

	    #endregion	
    }
}