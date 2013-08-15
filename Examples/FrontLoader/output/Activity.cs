using System;
using System.Collections.Generic;
using System.Linq;

namespace Marvin.Examples.FrontLoader
{
   internal class Activity{
       
       public Activity(string name, int Duration, DateTime earlyStartDate, IEnumerable<Activity> Predecessors){
           _earlyStartDate = earlyStartDate;
           _name = name;
role____duration= Duration;
role____predecessors= Predecessors ?? Enumerable.Empty<Activity>();
       }
       private bool _planned;
        private DateTime _earlyStartDate;
        private DateTime _earlyEndDate;
       private readonly string _name;
       public string Name{get { return _name; }}
        
       public void  Plan(){
            if(_planned) return;
            Console.WriteLine("Planning: " + Name);
            _earlyStartDate = self__predecessors__GetStartDate();
            _planned = true;
        }

        public DateTime EarlyEndDate{
            get{
                return _earlyEndDate;
            }
        }
        
        public DateTime EarlyStartDate{
            get{
                return _earlyStartDate;
            }
        }

        public bool Planned{
            get { return _planned; }
        }

       public int Duration{
            get { return role____duration; }
        }

       public IEnumerable<Activity> Predecessors{get { return role____predecessors; }}
private dynamic role____duration;private dynamic role____predecessors;            DateTime  self__duration__GetEndDate()
            {
               if(!Planned){
                    Plan();
                }
                return _earlyEndDate = EarlyStartDate + role____duration;
            }
            DateTime  self__predecessors__GetStartDate(){
                var list = ((IEnumerable<Activity>) this);
                if(list == null || !list.Any())
                    return DateTime.Now.Date;
                return list.Select(p => p.EarlyEndDate).Max();
            }

        
    }
}
