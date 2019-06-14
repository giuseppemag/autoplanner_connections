using System.Collections.Generic;

namespace AutoplannerConnections
{
    class Project
    {
        public string name {get; set;}
        public int teamweekId {get; set;}
        public string simplicateId {get; set;}
        public List<ProjectService> services {get; set;}

        public Project () {}

        public Project (string name, int teamweekId, string simplicateId) 
        {
            this.name = name;
            this.teamweekId = teamweekId;
            this.simplicateId = simplicateId;
        }
    }
}