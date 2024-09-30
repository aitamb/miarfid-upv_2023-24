using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GAIA{

    // <summary>
    // Concurrent_States Finite Automaton based on FA_Classic
    // </summary>
    // <remarks></remarks>
    public class FA_Concurrent_States : FA_Classic {

	    //<summary>Max number of concurrent states</summary>
	    private int n_MAX_Concurrent;
        //<summary>Collection that stores initial enabled states </summary>
	    private List<State> initiallyEnabled;
        //<summary>Dictionary of execution credits. Used and handled internally</summary>
	    private Dictionary<int, int> StatesCredits;

        // <summary>
        // Initializes a new instance of the <see cref="T:FSM.FA_Concurrent_States">FA_Concurrent_States</see> class. 
        // </summary>
        // <param name="ID">Name of the FSM based on this FA</param>
        // <param name="tag">Tag identifier for the FSM based on this FA</param>
        // <param name="num">Max number of concurrent states</param>
        // <param name="CallbackName">This events routine must be implemented</param>
        // <param name="FlagProbability">If set to <see langword="true"/>, then, it is a probabilistic FA_Classic ; otherwise, it is a deterministic FA_Concurrent_States</param>
        // <remarks></remarks>
	    public FA_Concurrent_States(string ID, int tag, int num, string CallbackName, bool FlagProbability) : base(ID, tag, CallbackName, FlagProbability) {
		    name = ID;
		    FAtype = FAType.CONCURRENT;
		    FAId = tag;
		    n_MAX_Concurrent = num;
		    this.CallbackName = CallbackName;
		    initiallyEnabled = new List<State>();
	    }
        // Use this for initialization
        // <summary>
        // This method allows the starting of the FSM based on this FA
        // </summary>
        // <remarks>It must be called when the FA is complete</remarks>
	    public override void Start(){
		    //Start Method as Deterministic

		    StatesCredits = new Dictionary<int, int>();

		    foreach(State st in StatesList){
			    if(st.initial){
				    initiallyEnabled.Add(st);
				    initial = st;
				    stateIndex = StatesList.IndexOf(st);
			    }

			    //add to a dictionary of credits
			    StatesCredits.Add(st.getTag(), (int)st.getCredits());

			    //st.setEnabledBy(EnableByEvents(st));
			    if(st.getSubFA()!=null){
				    st.getSubFA().Start();
			    }
		    }
	    }

        //private List<int> EnableByEvents(State s){
        //    List<int> EnableBy = new List<int>();
        //    //UnityEngine.Debug.Log(this.getTransitionsList().Count+" lista de transiciones");
        //    foreach(Transition t in this.getTransitionsList()){
        //        if(t.getFinal().getTag() == s.getTag()){
        //            //EnableBy.Add(t.getEvent());
        //        }
        //    }
        //            //UnityEngine.Debug.Log(EnableBy.Count+" lista de activaciones para el estado: "+s.getID());

        //    return EnableBy;
        //}

        // <summary>
        // Get the dictionary that controls execution credits
        // </summary>
        // <returns>Dictionary collection</returns>
        // <remarks>Used internally</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<int, int> getCreditsDic() { return StatesCredits; }

        // <summary>
        // Get initial states of the FSM based on this FA
        // </summary>
        // <returns>List of initial states </returns>
        // <remarks>It could be void if there are not any initial state </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<State> getInitials() { return initiallyEnabled; }

        // <summary>
        // Get Max number of concurrent states
        // </summary>
        // <returns>An int value</returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int getMaxConcurrent() { return n_MAX_Concurrent; }
	}
}
