using System;

namespace AutoplannerConnections
{
    class Task
    {
        public int teamweekId {get; set; }
        public string simplicateId {get; set; }
        public string name {get; set; }
        public DateTime start {get; set; }
        public DateTime end {get; set; }
        public double hours {get; set; }
        public Employee employee {get; set; }
        public Project project {get; set; }

        public Task () {}

        public Task (string name, DateTime start, DateTime end, Employee employee, Project project = null, int teamweekId = 0, string simplicateId = "") 
        {
            this.name = name;
            this.start = start;
            this.end = end;
            this.employee = employee;
            this.project = project;
            this.hours = (end - start).TotalHours;
            this.teamweekId = teamweekId;
            this.simplicateId = simplicateId;
        }
    }
}