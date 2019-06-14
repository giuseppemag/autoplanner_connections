namespace AutoplannerConnections
{
    class Employee
    {
        public string name {get;}
        public int teamweekId {get;}
        public string simplicateId {get;}

        public Employee (string name, int teamweekId, string simplicateId) 
        {
            this.name = name;
            this.teamweekId = teamweekId;
            this.simplicateId = simplicateId;
        }
    }
}