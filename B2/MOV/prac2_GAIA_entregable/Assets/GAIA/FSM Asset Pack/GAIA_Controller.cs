//Uncoment the line below to enable the use of the BT functions.
#define PANDA

using UnityEngine;
using System.Collections;
using System.IO;
using GAIA;

/// <summary>
/// Class honding the Finite State Machines Manager. It holds all the different 
/// State Machines. Since only one instance of this makes sense and different
/// elements could request a State Machine the Singletone Patterns has been implemented
/// to simplify things.
/// </summary>
public class GAIA_Controller : MonoBehaviour 
{
    public TextAsset[] m_xmlFilesFSM; // ngs - xml files with the specification of the finite state machines

    public TextAsset[] m_xmlFilesBT; // xml files with the specification of the behavior trees

    public GAIA_Manager m_manager;		 //MANAGER 		-> controls the parsing and initialization of FSMs

    // Singleton
    private static GAIA_Controller m_instance;

    public static GAIA_Controller INSTANCE
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = GameObject.FindObjectOfType<GAIA_Controller>();
            }
            return m_instance;
        }
    }

	void Awake () 
    {
        //Creation of a new xmltest.GAIA_Parser
        GAIAXML.GAIA_Parser parser = new GAIAXML.GAIA_Parser();

        //Creation of a new Manager (with a GAIA_Parser)
        m_manager = new GAIA_Manager(parser);

        //Loads and parses all FSM files
        if (m_xmlFilesFSM != null)
        {
            for (int i = 0; i < m_xmlFilesFSM.Length; i++)
            {
                m_manager.addFSM(m_xmlFilesFSM[i].text);
            }
        }

#if (PANDA)
        //Loads and parses all xml definitions of BTs
        if (m_xmlFilesBT != null)
        {
            for (int i = 0; i < m_xmlFilesBT.Length; i++)
            {
                m_manager.addBT(m_xmlFilesBT[i].text);
            }
        }
#endif
        parser.WriteLog("");

    }

}
