using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSFScheduler
{
    /// <summary>
    /// A class used to calculate and store all access windows from the all systems to all targets
    /// Also used to retrieve all accesses currently available to a system at a given simulation time
    /// This class replaces the geomAccess class from Horizon 2.3
    /// </summary>
    class Access
    {
        //I think this originally took in an array of List<Tasks> and and array of List<List<Dictionary>>? 
        //so I changed it to take in List<List<Task>> and List<List<List<Dictionary>>>
        public static List<List<Task>> getCurrentAccesses(List<List<Task>> taskList, List<List<List<Dictionary<double, bool>>>> access_pregen, double time)
        {
            int num_tasks = access_pregen.Count;
            int num_assets = access_pregen[0].Count;
            // initialize vector to hold tasks each asset has access to
            List<Stack<Task>> assetTasks = new Stack<List<Task>>(num_assets); //TODO double check this
                                                                              // iterate through each task that access has been pregenerated for
                                                                              //foreach (List<Dictionary<double, bool> accessPregenIt in access_pregen)
            int i = 0, j;
            List<List<Dictionary<double, bool>>> accessPregenIt = new List<List<Dictionary<double, bool>>>();
            List<Stack<Task>> assetTaskIt = assetTasks;
            //  List<Task> taskIt = taskList;
            foreach (List<Task> taskIt in taskList)
            {
                // accessPregenIt = access_pregen[i++];// List of dictionaries.
                bool hasAccess;
                j = 0;
                foreach (List<Dictionary<double, bool>> assetAccessPregenIt in accessPregenIt)
                {
                    //assetTaskIt = assetTasks[j++];
                    Dictionary<double, bool> access = assetAccessPregenIt.upper_bound(time);
                    // check the map to find if the asset has access to the task
                    foreach (Dictionary<double, bool> access in
                            assetAccessPregenIt.upper_bound(time))
                    {// need to implement upper_bound extention
                        if (access != accessPregenIt.end())
                        {
                            hasAccess = !(access.TryGetValue());
                        }
                        else {
                            hasAccess = assetAccessPregenIt.Last().TryGetValue();
                        }
                        if (hasAccess)
                        {
                            assetTaskIt.ElementAt(j).Push(taskIt);
                        }
                    }
                }
            }
        }

        public List<List<Dictionary<double, bool>>> pregenerateAccesses(HSFSystem system, vector<Task> tasks, double stepLength, double endTime)
        {

        }
    }
}
