using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace GAIA{

    // <summary>
    // FSM_Event class used to create events in the loading phase
    // </summary>
    // <remarks></remarks>
	public class Event {

        public enum EventType
        {
            NULL, BASIC, STACKABLE, HIERARCHICAL
        }

        // <summary>
        // Get a string that has the name of a given EventType of State Machine and returns the EventType
        // </summary>
        // <returns>FAType</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EventType string2Tag(string s)
        {
            EventType type;

            if (Enum.TryParse(s, out type))
                return type;
            else return EventType.BASIC;
        }

        //<summary>Identifier or name of this FSM_Event</summary>
        private string eventName;
        //<summary>Identifier tag number of this FSM_Event</summary>
        private int    eventId;

        //<summary>Type of FSM_Event (BASIC or STACKABLE)</summary>
        private EventType type;

        // <summary>
        // Initializes a new instance of the <see cref="T:FSM.FSM_Event">FSM_Event</see> class. 
        // </summary>
        // <param name="id">Name of this FSM_Event</param>
        // <param name="event_tag">Identifier tag of this FSM_Event</param>
        // <param name="type">"BASIC" or "STACKABLE"</param>
        // <remarks></remarks>
		public Event (string id, int event_tag, string type) {
			eventName = id;
			eventId = event_tag;

            this.type = string2Tag(type);
		}

        // <summary>
        // Get this FSM_Event identifier name
        // </summary>
        // <returns> string value </returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string getID() { return eventName;}

        // <summary>
        // Get this FSM_Event identifier tag
        // </summary>
        // <returns> int value </returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int getEventTag() { return eventId; }

        // <summary>
        // Get this FSM_Event type (BASIC or STACKABLE)
        // </summary>
        // <returns> int value </returns>
        // <remarks></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public EventType getEventType() { return type; }
	}
}
