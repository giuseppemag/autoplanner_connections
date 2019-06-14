using System.Collections.Generic;

namespace AutoplannerConnections
{
    class ProjectService
    {
        public string id {get; set;}
        public string name {get; set;}
        public List<HoursType> hoursTypes {get; set;}
    }
}