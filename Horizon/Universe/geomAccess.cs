using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Horizon;

namespace Universe{
    public class geomAccess{    
       
        
       public List< List<Task> > getCurrentAccesses(List<Task> taskList, List< List< Dictionary<double, bool> > > access_pregen, double time){
           int _num_tasks = access_pregen.size();
           int _num_assets = access_pregen.at(0).size(); 
           // initialize vector to hold tasks each asset has access to
           List<List<Task>> assetTasks(_num_assets);
           // iterate through each task that acces has been pregenerated for
           //foreach (List<Dictionary<double, bool> accessPregenIt in access_pregen)
           int i = 0, j = 0;
           List<List<Dictionary<double, bool>> accessPregenIt;
           foreach(List<Task> taskIt in taskList){
               accessPregenIt = access_pregen[i++];
               bool hasAccess;
               j = 0;
               foreach(List<Dictionary<double, bool> assetAccessPregenIt in accessPregenIt){
                   List<List<Task> assetTaskIt = assetTasks[j++];
                   // check the map to find if the asset has access to the task
                   foreach(Dictionary<double,bool> access in 
                           assetAccessPregenIt.upper_bound(time)){// not sure about this
                       if(access != accessPregenIt.end()){
                           hasAccess = !(access.second);
                       }
                       else{
                           hasAccess = assetAccessPregenIt.rbegin().second;
                       }
                       if(hasAccess){
                           assetTaskIt.add(taskIt);
                       }
                   }
               }
           }
       }

       public List< List< Dictionary<double, bool> > > pregenerateAccesses(System system, vector<Task> tasks, double stepLength, double endTime){
           
       }

    }
}