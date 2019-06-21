namespace AutoplannerConnections
{
    class Employee
    {
        public string firstName {get;}
        public string lastName {get;}
        public int teamweekId {get;}
        public string simplicateId {get;}

        public Employee (string firstName, string lastName, int teamweekId, string simplicateId) 
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.teamweekId = teamweekId;
            this.simplicateId = simplicateId;
        }
    }
}