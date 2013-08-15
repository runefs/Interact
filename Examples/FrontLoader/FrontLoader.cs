using System.Collections.Generic;
using System.Linq;

namespace Marvin.Examples.FrontLoader
{
    internal class FrontLoader{
        public FrontLoader(IEnumerable<Activity> activities){
            allActivities = activities;
        }

        
        [Role]
        private class allActivities{
            void Plan(){
                IEnumerable<Activity> list;
                do{
                    list = ((IEnumerable<Activity>)this).Where(a => !a.Planned);
                    foreach(var activity in list){
                        activity.Plan();
                    }
                } while(list.Any());
            }
        }

       public IEnumerable<Activity> Plan() {
           allActivities.Plan();
           return allActivities;
       }
    }
}
