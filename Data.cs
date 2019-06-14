using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace AutoplannerConnections
{
    class Data
    {
        public List<int> teamweekTaskIds;
        public List<string> simplicateTaskIds;

        public List<Employee> employees {get; set;}
        public List<Project> projects {get; set;}
        public List<Task> tasks {get; set;}
        public Config config;

        public Data (bool fromJson = false) 
        {
            this.employees = new List<Employee>();
            this.projects = new List<Project>();
            this.tasks = new List<Task>();

            if (fromJson) ReadFromJson();
        }

        private void ReadFromJson () 
        {
            using (FileStream fs = new FileStream("Json/employee.json", FileMode.Open))
                using (StreamReader reader = new StreamReader(fs))
                    this.employees = JsonConvert.DeserializeObject<List<Employee>>(reader.ReadToEnd());

            using (FileStream fs = new FileStream("Json/project.json", FileMode.Open))
                using (StreamReader reader = new StreamReader(fs))
                    this.projects = JsonConvert.DeserializeObject<List<Project>>(reader.ReadToEnd());

            using (FileStream fs = new FileStream("Json/teamweekTask.json", FileMode.Open))
                using (StreamReader reader = new StreamReader(fs))
                    this.teamweekTaskIds = JsonConvert.DeserializeObject<List<int>>(reader.ReadToEnd());
            
            using (FileStream fs = new FileStream("Json/simplicateTask.json", FileMode.Open))
                using (StreamReader reader = new StreamReader(fs))
                    this.simplicateTaskIds = JsonConvert.DeserializeObject<List<string>>(reader.ReadToEnd());

            using (FileStream fs = new FileStream("Json/config.json", FileMode.Open))
                using (StreamReader reader = new StreamReader(fs))
                    this.config = JsonConvert.DeserializeObject<Config>(reader.ReadToEnd());
        }

        public void WriteToJson () 
        {
            tasks.ForEach(task => {
                if (task.teamweekId != -1) this.teamweekTaskIds.Add(task.teamweekId);});
            tasks.ForEach(task => { 
                if (task.simplicateId != "") this.simplicateTaskIds.Add(task.simplicateId);});

            string employeeJson = JsonConvert.SerializeObject(this.employees, Formatting.Indented);
            string projectJson = JsonConvert.SerializeObject(this.projects, Formatting.Indented);
            string teamweekTaskJson = JsonConvert.SerializeObject(this.teamweekTaskIds, Formatting.Indented);
            string simplicateTaskJson = JsonConvert.SerializeObject(this.simplicateTaskIds, Formatting.Indented);
            string configJson = JsonConvert.SerializeObject(this.config, Formatting.Indented);

            System.IO.File.WriteAllText("Json/employee.json", employeeJson);
            System.IO.File.WriteAllText("Json/project.json", projectJson);
            System.IO.File.WriteAllText("Json/teamweekTask.json", teamweekTaskJson);
            System.IO.File.WriteAllText("Json/simplicateTask.json", simplicateTaskJson);
            System.IO.File.WriteAllText("Json/config.json", configJson);
        }
    }
}