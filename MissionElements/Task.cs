using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace MissionElements
{
    /// <summary>
    /// An action to be performed at a target, with limitations and suggestions for scheduling.
    /// @author Einar Pehrson
    /// @author Eric Mehiel
    /// </summary>
    [Serializable]
    public class Task
    {
        /* the type of task being performed */
         public TaskType Type { get; private set; }

        /* the target associated with the task */
        public Target Target { get; private set; }

        /** The maximum number of times the task should be performed by the ENTIRE SYSTEM (all assets count towards this)*/
        public int MaxTimesToPerform { get; private set; }



        /// <summary>
        /// Creates a new task to be performed at the given target, with the given scheduling limitations
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="target"></param>
        /// <param name="maxTimesToPerform"></param>
        public Task(TaskType taskType, Target target, int maxTimesToPerform)
        {
            Type = taskType;
            Target = target;
            MaxTimesToPerform = maxTimesToPerform;
        }

        public override string ToString()
        {
            return Target.Name;
        }

        public static bool loadTargetsIntoTaskList(XmlNode targetDeckXMLNode, Stack<Task> tasks)
        {
            Console.WriteLine("Loading target deck...");
            int maxTimesPerform = 1;
            bool allLoaded = true;
            string targetType, taskType;
            foreach (XmlNode targetNode in targetDeckXMLNode.ChildNodes)
            {
                if (targetNode.Attributes["TaskType"] != null)
                    taskType = targetNode.Attributes["TaskType"].Value.ToString();
                else {
                    return false;
                }
                var taskTypeEnum = (TaskType)Enum.Parse(typeof(TaskType), taskType);
                if (targetNode.Attributes["MaxTimes"] != null)
                {
                    Int32.TryParse(targetNode.Attributes["MaxTimes"].Value.ToString(), out maxTimesPerform);
                    tasks.Push(new Task(taskTypeEnum, new Target(targetNode), maxTimesPerform));
                }
                else
                    tasks.Push(new Task(taskTypeEnum, new Target(targetNode), maxTimesPerform));

                // USER - Add other target properties in Target Constructor using XML input
            }
            Console.WriteLine("Number of Targets Loaded: ");

            return allLoaded;
        }
    }
    // ToDo: convert taskType to an extensable enumeration. <-- Still?.... 

    public enum TaskType { EMPTY, COMM, IMAGING }

  
}
