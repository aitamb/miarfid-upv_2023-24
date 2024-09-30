//Uncoment the line below to enable the use of the BT functions.
#define PANDA

using System.Collections.Generic;
using System;
using UnityEngine;
using Panda;
using GAIA;

namespace GAIA
{
    public enum ParsingErrors
    {
        OK,         ///< There is no problem
        ParserReq,  ///< There is no parser available
        WrongFA,    ///< The FA cannot be parsed or it is repeated
        WrongBT,    ///< The FA cannot be parsed or it is repeated
        ParsingErr, ///< There is an error parsing the file
        TotalParsingStates
    }

    //-2 if there is a problem adding the BT into the dictionary

    // Manager of FSMs and BTs
    public class GAIA_Manager
    {
        //GAIA_Manager Class Attributes
        // An instance of an GAIA_Parser (filled with GAIA_Manager(parser) constructor.
        GAIAXML.GAIA_Parser parser;
        //Dictionary to add a finite automata with FAtype + FAid (Tuple) and the FA itself
        Dictionary<Tuple, FA_Classic> FSM_dic;
        //Dictionary to add a behaviour tree with the BT definition file name and its contents
        Dictionary<string, string> BT_dic;

        //Struct that allows to add a Finite automata with type+ID
        public struct Tuple
        {
            int FSMtype;
            string FSMid;

            public Tuple(int FSMtype, string FSMid)
            {
                this.FSMtype = FSMtype;
                this.FSMid   = FSMid;
            }
        }

        // Initializes a new instance of the GAIA_Manager class with an instance of GAIA_Parser.
        public GAIA_Manager(GAIAXML.GAIA_Parser parser)
        {
            this.parser = parser;
            FSM_dic = new Dictionary<Tuple, FA_Classic>();
            BT_dic  = new Dictionary<string, string>();
        }

        // Initializes a new instance of the GAIA_Manager class.
        public GAIA_Manager()
        {
            FSM_dic = new Dictionary<Tuple, FA_Classic>();
            BT_dic  = new Dictionary<string, string>();
        }

        // Add a machine (passed as FA parameter) to this GAIA_Manager
        // Returns:
        // true if OK
        // false if cannot be added
        public bool addFSM(FA_Classic fsm)
        {
            try
            {
                //Add the FA to the attribute FSM_dic
                FSM_dic.Add(new Tuple(fsm.getTag(), fsm.getFAid()), fsm);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        // Adds a machine (located in path) to this GAIA_Manager
        public ParsingErrors addFSM(string path)
        {
            if (null == parser)
                return ParsingErrors.ParserReq;
            else
            {
                //Invoke parser with path
                FA_Classic parsedfsm = parser.ParsePath(path);
                if (null != parsedfsm)
                {
                    try
                    {
                        FSM_dic.Add(new Tuple(parsedfsm.getTag(), parsedfsm.getFAid()), parsedfsm);
                        return ParsingErrors.OK;
                    }
                    catch (Exception)
                    {
                        return ParsingErrors.WrongFA;
                    }
                }
                else return ParsingErrors.ParsingErr; 
            }
        }

#if (PANDA)
        // Add a behaviour tree (passed as the contents of an xml file in the form of a string) to this GAIA_Manager
        public ParsingErrors addBT(string content)
        {
            string[] parsedbt = new string[1];

            if (null == parser)
            {
                return ParsingErrors.ParserReq;
            }
            else
            {
                if(null != content)
                { //Invoke parser with string
                    parsedbt = parser.ParseBT(content);
                }
                if (null != parsedbt)
                {
                    try
                    {
                        BT_dic.Add(parsedbt[0], parsedbt[1]);
                        return ParsingErrors.OK;
                    }
                    catch (Exception)
                    {
                        return ParsingErrors.WrongBT;
                    }
                }
                else return ParsingErrors.ParsingErr;
            }
        }
#endif

        // Removes a machine that had been added to this GAIA_Manager
        // Returns:
        // true if OK
        // false if cannot remove that machine (corrupted fsm object or it does not exist)
        public bool deleteFSM(FA_Classic fsm)
        {
            try
            {
                FSM_dic.Remove(new Tuple(fsm.getTag(), fsm.getFAid()));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

#if (PANDA)
        // Removes a behaviour tree that had been added to this GAIA_Manager
        // Returns:
        // true if OK
        // false if cannot remove that behaviour tree (corrupted BT object or it does not exist)
        public bool deleteBT(string bt_id)
        {
            try
            {
                BT_dic.Remove(bt_id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
#endif

        // Creates a returnable FSM_Machine to an entity (character)
        // Returns:
        // The correct value of FSM_Machine or NULL (wrong parameters or that FSM does not exist))
        // It tries to find a matching between the passed parameters and an instance in the internal FSM Dictionary (FSM_Dic)
        public FSM_Machine createMachine(System.Object character, int FSM_type, string FSM_id)
        {
            try
            {
                return new FSM_Machine(FSM_dic[new Tuple(FSM_type, FSM_id)], character);
            }
            catch (Exception e)
            {
                Debug.Log("EXCEPTION: " + e);
                return null;
            }
        }

#if (PANDA)
        // Adds a PandaBehaviour component to the passed GameObject and compiles the selected BT based on the file identifier.
        // MUST specify when the BT will tick:
        //    -->  updadeType:
        //                      1: Tick on Update
        //                      2: Tick manually
        //                      3: Tick on FixedUpdate
        //                      4: Tick on LateUpdate
        public void createBT(GameObject character, string bt_id, PandaBehaviour.UpdateOrder updateType)
        {
            try
            {
                PandaBehaviour component = character.AddComponent<PandaBehaviour>();
                component.Compile(BT_dic[bt_id]);
                component.tickOn = updateType;
            }
            catch (Exception e)
            {
                Debug.Log("EXCEPTION: " + e);
            }
        }

        // Default function to create a BT without specifying when the tree is ticked (DEFAULT TICKS ARE ON UPDATE).
        public void createBT(GameObject character, string bt_id)
        {
            try
            {
                PandaBehaviour component = character.AddComponent<PandaBehaviour>();
                component.Compile(BT_dic[bt_id]);
            }
            catch (Exception e)
            {
                Debug.Log("EXCEPTION: " + e);
            }
        }
#endif

#if (PANDA)
        // Change the BT to run by the PandaBehaviour component.
        // MUST specify the id of the BT you want to compile.
        public void changeBT(GameObject character, string bt_id)
        {
            try
            {
                PandaBehaviour component = character.GetComponent<PandaBehaviour>();
                component.Compile(BT_dic[bt_id]);
            }
            catch (Exception e)
            {
                Debug.Log("EXCEPTION: " + e);
            }
        }
#endif

#if (PANDA)
        // Change the order in which the tree is ticked.
        // MUST specify when the BT will tick:
        //    -->  updadeType:
        //                      1: Tick on Update
        //                      2: Tick manually
        //                      3: Tick on FixedUpdate
        //                      4: Tick on LateUpdate
        public void changeTickOn(GameObject character, PandaBehaviour.UpdateOrder updateType)
        {
            try
            {
                PandaBehaviour component = character.GetComponent<PandaBehaviour>();
                component.tickOn = updateType;
            }
            catch (Exception e)
            {
                Debug.Log("EXCEPTION: " + e);
            }
        }
#endif
    }
}