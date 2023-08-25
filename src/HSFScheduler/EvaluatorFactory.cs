// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using HSFSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MissionElements;
using Utilities;

namespace HSFScheduler
{
    public class EvaluatorFactory
    {
        /// <summary>
        /// Static method to generate an evaluator from XML node
        /// </summary>
        /// <param name="evalNode"></param>
        /// <returns></returns>
        public static Evaluator GetEvaluator(XmlNode evalNode, List <Subsystem> subList)
        {
            string evalType = evalNode.Attributes["type"].Value.ToString().ToLower();
            Evaluator schedEvaluator = null;    
            if (evalType != null)
            {
                if (evalType.Equals("scripted"))
                {
                    EvaluatorFactory EvaluatorFactory = new EvaluatorFactory(); // is this correct??
                    List<dynamic> keychain = EvaluatorFactory.BuildKeychain(evalNode.SelectNodes("KEYREQUEST"), subList);
                    schedEvaluator = new ScriptedEvaluator(evalNode, keychain);
                    Console.WriteLine("Scripted Evaluator Loaded");
                }
                else if (evalType.Equals("targetvalueevaluator"))
                {
                    EvaluatorFactory EvaluatorFactory = new EvaluatorFactory(); // is this correct??
                    List<dynamic> keychain = EvaluatorFactory.BuildKeychain(evalNode.SelectNodes("KEYREQUEST"), subList);
                    schedEvaluator = new TargetValueEvaluator(keychain);
                    Console.WriteLine("Target Value Evaluator Loaded");
                }
            }
            if (schedEvaluator == null)
            {
                if(evalType != null)
                {
                    Console.WriteLine("Attempt to load input evaluator failed, loading Default Evaluator...");
                }
                schedEvaluator = new DefaultEvaluator(); // ensures at least default is used
                Console.WriteLine("Default Evaluator Loaded");
            }
            return schedEvaluator;
        }

        private List<dynamic> BuildKeychain (XmlNodeList keyRequests, List<Subsystem> subList)
        {
            //List<StateVariableKey<dynamic>> keychain = new List<StateVariableKey<dynamic>>(); // lets see if dynamic works here??? Nope.
            List<dynamic> keychain = new List<dynamic>(); // Here!
            foreach (XmlNode keySourceNode in keyRequests)
            {
                string InputSub = keySourceNode.Attributes["keySub"].Value.ToString().ToLower();
                string InputAsset = keySourceNode.Attributes["keyAsset"].Value.ToString().ToLower();
                string InputType = keySourceNode.Attributes["keyType"].Value.ToString().ToLower();
                //int idx;
                //try
                //{
                //    idx = Int32.Parse(keySourceNode.Attributes["keyIndex"].Value.ToString());
                //}
                //catch
                //{
                //    Console.WriteLine("Key index requested is not Int32!");
                //    throw new ArgumentException("Key index requested is not Int32!");
                //}
                Subsystem subRequested = subList.Find(s => s.Name == InputAsset + "." + InputSub);

                if (subRequested == null)
                {
                    Console.WriteLine("Asset/Subsystem pair requested was not found!");
                    throw new ArgumentException("Asset/Subsystem pair requested was not found!");
                }

                //if (InputType.Equals("int"))
                //{
                //    Utilities.StateVariableKey<Int32> keyRequested = subRequested.Ikeys[idx];
                //    keychain.Add(keyRequested);
                //}
                if (InputType.Equals("int"))
                {
                    foreach (StateVariableKey<Int32> keyOfTypeRequested in subRequested.Ikeys)
                    {
                        keychain.Add(keyOfTypeRequested);
                    }
                }
                else if (InputType.Equals("double"))
                {
                    foreach (StateVariableKey<double> keyOfTypeRequested in subRequested.Dkeys)
                    {
                        keychain.Add(keyOfTypeRequested);
                    }
                }
                else if (InputType.Equals("bool"))
                {
                    foreach (StateVariableKey<bool> keyOfTypeRequested in subRequested.Bkeys)
                    {
                        keychain.Add(keyOfTypeRequested);
                    }
                }
                else if (InputType.Equals("matrix"))
                {
                    foreach (StateVariableKey<Matrix<double>> keyOfTypeRequested in subRequested.Mkeys)
                    {
                        keychain.Add(keyOfTypeRequested);
                    }
                }
                else if (InputType.Equals("quat"))
                {
                    foreach (StateVariableKey<Quaternion> keyOfTypeRequested in subRequested.Qkeys)
                    {
                        keychain.Add(keyOfTypeRequested);
                    }
                }
                else if (InputType.Equals("vector"))
                {
                    foreach (StateVariableKey<Vector> keyOfTypeRequested in subRequested.Vkeys)
                    {
                        keychain.Add(keyOfTypeRequested);
                    }
                }
                else
                {
                    Console.WriteLine("Key type requested is not supported!");
                    throw new ArgumentException("Key type requested is not supported!");
                }
            }
            return keychain;
        }
    }
}
