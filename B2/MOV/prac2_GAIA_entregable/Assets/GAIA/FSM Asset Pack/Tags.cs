using System;
using System.Collections;
using System.Runtime.CompilerServices;

//Tags that identify data of XML file
public static class Tags
{
	//State tags
	public enum StateTags
	{
		NULL,
		SCATTERING,  // 1
		CHASING,     // 2
		FRIGHTENING, // 3
		DYING,       // 4
		EATING       // 5
	}

	//Transition tags
	public enum TransitionTags
	{
		NULL,
        SCATTERING_TO_CHASING,
        CHASING_TO_SCATTERING,
        SCATTERING_TO_FRIGHTENING,
        CHASING_TO_FRIGHTENING,
        FRIGHTENING_TO_SCATTERING,
        FRIGHTENING_TO_DYING,
        DYING_TO_SCATTERING
    }

	//EVENT TAGS
	public enum EventTags
	{
		NULL,
        PACMAN_VISTO,
        PACMAN_NO_VISTO,
        POWER_PELLET_ON,
        POWER_PELLET_OFF,
		GHOST_DEAD,
        RESURRECT
    }

	//ACTION TAGS
	public enum ActionTags
	{
		NULL,
		SCATTER,
		CHASE,
		FRIGHTEN,
		DIE,
		RESET
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// <summary>
	// Get a string that has the name of a given enumeration and returns the type of enumerated value associated
	// </summary>
	// <returns> Generic enumerated value</returns>
	// <remarks> Generic lexical analyzer. Converts a lexeme into a tag with meaning </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TEnum name2Tag<TEnum>(string s)
	where TEnum : struct
	{
		TEnum resultInputType;

		Enum.TryParse(s, true, out resultInputType);
		return resultInputType;
	}
}
