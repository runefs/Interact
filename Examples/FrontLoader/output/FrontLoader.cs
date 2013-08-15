using System.Collections.Generic;
using System.Linq;

namespace Marvin.Examples.FrontLoader
{
    internal class FrontLoader{
        public FrontLoader(IEnumerable<Activity> activities){
role____allActivities= activities;
        }

       public IEnumerable<Activity> Plan() {
self__allActivities__Plan();
           return role____allActivities;
       }
private dynamic role____allActivities;            void  self__allActivities__Plan(){
                IEnumerable<Activity> list;
                do{
                    list = ((IEnumerable<Activity>)role____allActivities).Where(a => !a.Planned);
                    foreach(var activity in list){
                        activity.Plan();
                    }
                } while(list.Any());
            }
    }
}
