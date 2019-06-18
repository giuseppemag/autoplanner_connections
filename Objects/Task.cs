using System;

namespace AutoplannerConnections
{
    class Task
    {
        public int teamweekId {get; set; }
        public string simplicateId {get; set; }
        public string serviceId {get; set;}
        public string hoursTypeId {get; set;}
        public string name {get; set; }
        public string taskString {get; set; }
        public DateTime start {get; set; }
        public DateTime end {get; set; }
        public double hours {get; set; }
        public Employee employee {get; set; }
        public Project project {get; set; }

        public Task () {}
    }
}