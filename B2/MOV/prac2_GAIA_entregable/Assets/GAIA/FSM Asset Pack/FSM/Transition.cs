using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GAIA{

    // <summary>
    // Transition object class
    // </summary>
    // <remarks></remarks>
public class Transition {

    //Probability constants
    public const double SURE_EVENT       = 1.0,
                        IMPOSSIBLE_EVENT = 0.0;

    //Transition Class attributes

    // <summary>Name that identifies this transition</summary>
    string transitionName;
    // <summary>Transition tag identifier</summary>
	int transitionId,
    // <summary>Action tag identifier of this transition</summary>
        action;

    // <summary>State origin from which this transition leaves</summary>
	State origin,
    // <summary>State final to which this transition arrives</summary>
          final;
    // <summary>Collection of events that enable this transition</summary>
	List<Event> EventsList;
    //<summary>Probability of execution of this transition. Only used if the FA is probabilistic </summary>
	double probability = SURE_EVENT; 
	
	
    // <summary>
    // Initializes a new instance of the <see cref="T:FSM.Transition">Transition</see> class. 
    // </summary>
    // <param name="ID">Transition's identifier name</param>
    // <param name="A">State origin</param>
    // <param name="B">State destination</param>
    // <param name="transition_tag">Transition's tag identifier</param>
    // <param name="action_tag">Transition's action tag identifier</param>
    // <param name="EventsList">List of events that enable this transition</param>
    // <remarks>Used in all FAs</remarks>
	public Transition(string ID, State A, State B, int action_tag, List<Event> EventsList){
		transitionName = ID;
		origin = A;
		final = B;
		transitionId = (int)Tags.name2Tag<Tags.TransitionTags>(transitionName);
		action = action_tag;
		this.EventsList = EventsList;
	}

    // <summary>
    // Initializes a new instance of the <see cref="T:FSM.Transition">Transition</see> class. 
    // </summary>
    // <param name="ID">Transition's identifier name</param>
    // <param name="A">State origin</param>
    // <param name="B">State destination</param>
    // <param name="transition_tag">Transition's tag identifier</param>
    // <param name="action_tag">Transition's action tag identifier</param>
    // <param name="EventsList">List of events that enable this transition</param>
    // <param name="probability">Probability between 0 and 100. Only used if the FA is probabilistic</param>
    // <remarks>It only can be used if FA is probabilistic. Its use does not make sense in the other FA</remarks>
	public Transition(string ID, State A, State B, int action_tag,  List<Event> EventsList, double probability){
		transitionName = ID;
		origin = A;
		final = B;
		transitionId = (int)Tags.name2Tag<Tags.TransitionTags>(transitionName);
		action = action_tag;
		this.EventsList = EventsList;
		this.probability = probability;
        checkProbability();
	}

    // <summary>
    // Verifies if the probability is between low and high limits and it corrects it if exceeded
    // </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void checkProbability () {
        //Probability cannot be neither higher than 1.0 nor negative
        if (probability > SURE_EVENT)
             probability = SURE_EVENT;
        else if (probability < IMPOSSIBLE_EVENT) 
                 probability = IMPOSSIBLE_EVENT; 
    }

    // <summary>
    // Verifies if the probability is between low and high limits and it corrects it if exceeded
    // </summary>
    // <returns>The probability corrected and verified</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double checkProbability(double prob)
    {
        //Probability cannot be neither higher than 1.0 nor negative
        if (prob > SURE_EVENT)
            prob = SURE_EVENT;
        else if (prob < IMPOSSIBLE_EVENT)
            prob = IMPOSSIBLE_EVENT;
        
        return prob;
    }

        #region GET methods

    // <summary>
    // Get transition's tag identifier
    // </summary>
    // <returns>transition's tag value</returns>
    // <remarks></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int getTag() { return transitionId; }

    // <summary>
    // Get transition's action tag identifier
    // </summary>
    // <returns>transition's action tag value</returns>
    // <remarks></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int getAction(){ return action; }

    //returns events list
    // <summary>
    // Get the specified list of events that can enable this transition
    // </summary>
    // <returns>List FSM_Events list attached to this transition </returns>
    // <remarks>It can be empty</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<Event> getEvents() { return EventsList; }

    // <summary>
    // Get probability attached to this transition. 100% default value
    // </summary>
    // <returns>Double number</returns>
    // <remarks></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double getProbability() { return probability; }

    // <summary>
    // Get the origin state of this transition
    // </summary>
    // <returns>State object</returns>
    // <remarks></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State getOrigin() { return origin; }

    // <summary>
    // Get the destination state of this transition
    // </summary>
    // <returns>State object</returns>
    // <remarks></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public State getFinal() { return final; }

    // <summary>
    // Get transition's name identifier
    // </summary>
    // <returns>A string value with the ID or null value</returns>
    // <remarks></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string getID() { if(transitionName!=null) return transitionName; else return null; }
    #endregion

    #region SET methods

    // <summary>
    // Set this transition's probability value
    // </summary>
    // <returns>
    // true if OK
    // false if error. This transition is not probabilistic
    //</returns>
    // <remarks></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void setProbability(float newProbability) { probability = newProbability; }
	
	#endregion
}
}
