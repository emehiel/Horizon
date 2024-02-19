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
using Newtonsoft.Json.Linq;
using UserModel;
using static Community.CsharpSqlite.Sqlite3;

namespace HSFScheduler
{
    public class EvaluatorFactory
    {

        public static Evaluator GetEvaluator(JObject evaluatorJson, List<Subsystem> subsystemList)
        {
            Evaluator schedEvaluator = null;

            if (JsonLoader<JToken>.TryGetValue("KeyRequests", evaluatorJson, out JToken keyRequests))

            if (JsonLoader<string>.TryGetValue("Type", evaluatorJson, out string type))
            {
                if (type.Equals("scripted", StringComparison.InvariantCultureIgnoreCase))
                {
                    List<dynamic> keychain = BuildKeyChain(keyRequests, subsystemList);
                    schedEvaluator = new ScriptedEvaluator(evaluatorJson, keychain);
                    Console.WriteLine("Scripted Evaluator Loaded");
                }
                else if (type.ToLower().Equals("TargetValueEvaluator", StringComparison.InvariantCultureIgnoreCase))
                {
                    List<dynamic> keychain = BuildKeyChain(keyRequests, subsystemList);
                    schedEvaluator = new TargetValueEvaluator(keychain);
                    Console.WriteLine("Target Value Evaluator Loaded");
                }
                else
                {
                    schedEvaluator = new DefaultEvaluator(); // ensures at least default is used
                    Console.WriteLine("Default Evaluator Loaded...  This may cause problems...");
                }
            }
            else
            {
                Console.WriteLine("Attempt to load evaluator failed, loading Default Evaluator...  This may cause problems...");
                schedEvaluator = new DefaultEvaluator(); // ensures at least default is used
            }

            return schedEvaluator;
        }
        /// <summary>
        /// Static method to generate an evaluator from XML node
        /// </summary>
        /// <param name="evalNode"></param>
        /// <returns></returns>
        //public static Evaluator GetEvaluator(XmlNode evalNode, List <Subsystem> subList)
        //{
        //    Evaluator schedEvaluator = null;
        //    if (evalNode == null) // If no evaluator node is provided,
        //    {
        //        schedEvaluator = new DefaultEvaluator(); // ensures at least default is used
        //        Console.WriteLine("Default Evaluator Loaded");
        //    }
        //    else // try to get the right evaluator
        //    {
        //        string evalType = evalNode.Attributes["type"].Value.ToString().ToLower();

        //        if (evalType != null)
        //        {
        //            if (evalType.Equals("scripted"))
        //            {
        //                EvaluatorFactory EvaluatorFactory = new EvaluatorFactory(); // is this correct??
        //                List<dynamic> keychain = EvaluatorFactory.BuildKeyChain(evalNode.SelectNodes("KEYREQUEST"), subList);
        //                schedEvaluator = new ScriptedEvaluator(evalNode, keychain);
        //                Console.WriteLine("Scripted Evaluator Loaded");
        //            }
        //            else if (evalType.Equals("targetvalueevaluator"))
        //            {
        //                EvaluatorFactory EvaluatorFactory = new EvaluatorFactory(); // is this correct??
        //                List<dynamic> keychain = EvaluatorFactory.BuildKeyChain(evalNode.SelectNodes("KEYREQUEST"), subList);
        //                schedEvaluator = new TargetValueEvaluator(keychain);
        //                Console.WriteLine("Target Value Evaluator Loaded");
        //            }
        //        }
        //        if (schedEvaluator == null) // Last ditch, evaluator node is provided, but not built by the factory
        //        {
        //            if (evalType != null)
        //            {
        //                Console.WriteLine("Attempt to load input evaluator failed, loading Default Evaluator...");
        //            }
        //            schedEvaluator = new DefaultEvaluator(); // ensures at least default is used
        //            Console.WriteLine("Default Evaluator Loaded");
        //        }
        //    }
        //    return schedEvaluator;
        //}

        private static List<dynamic> BuildKeyChain(JToken keyRequests, List<Subsystem> subsystems)
        {
            List<dynamic> keychain = new List<dynamic>();

            foreach (JObject key in keyRequests)
            {
                JsonLoader<string>.TryGetValue("subsystem", key, out string InputSub);
                InputSub =InputSub.ToLower();
                JsonLoader<string>.TryGetValue("asset", key, out string InputAsset);
                InputAsset = InputAsset.ToLower();
                JsonLoader<string>.TryGetValue("type", key, out string InputType);
                InputType = InputType.ToLower();
                //string InputSub = keySourceNode.Attributes["keySub"].Value.ToString().ToLower();
                //string InputAsset = keySourceNode.Attributes["keyAsset"].Value.ToString().ToLower();
                //string InputType = keySourceNode.Attributes["keyType"].Value.ToString().ToLower();
                
               Subsystem subRequested = subsystems.Find(s => s.Name == InputAsset + "." + InputSub);

                if (subRequested == null)
                {
                    Console.WriteLine("Asset/Subsystem pair requested was not found!");
                    throw new ArgumentException("Asset/Subsystem pair requested was not found!");
                }
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
