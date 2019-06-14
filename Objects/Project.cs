namespace AutoplannerConnections
{
    class Project
    {
        public string name {get;}
        public int teamweekId {get;}
        public string simplicateId {get;}
        public string serviceId {get;}
        public string hoursTypeId {get;}

        public Project (string name, int teamweekId, string simplicateId) 
        {
            this.name = name;
            this.teamweekId = teamweekId;
            this.simplicateId = simplicateId;
        }
    }
}