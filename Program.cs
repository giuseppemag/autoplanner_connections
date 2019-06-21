using System;
using System.Collections.Generic;
using System.IO;

namespace AutoplannerConnections
{
    class Program
    {
        private static Data jsonData;
        private static Simplicate simplicate;
        private static Teamweek teamweek;

        static void Main(string[] args) 
        {
            jsonData = new Data(fromJson: true);

            teamweek = new Teamweek();
            simplicate = new Simplicate();

            // Needs to be called at least once every 2 weeks (before refresh token expires and is unusable)
            teamweek.RefreshAccessToken(ref jsonData.config);

            List<int> teamweekTaskIds = new List<int>();
            teamweekTaskIds.AddRange(jsonData.teamweekTaskIds);
            foreach (int id in teamweekTaskIds) {
                if (teamweek.RemoveTeamweekTask(id, jsonData.config) || id == 0) {
                    jsonData.teamweekTaskIds.Remove(id);
                }
            }

            List<string> simplicateHourIds = new List<string>();
            simplicateHourIds.AddRange(jsonData.simplicateTaskIds);
            foreach (string id in simplicateHourIds) {
                if (simplicate.RemoveHours(id) || id == null) {
                    jsonData.simplicateTaskIds.Remove(id);
                }
            }

            Data planningData = ReadPlanning();
            jsonData.tasks.Clear();
            foreach (Task task in planningData.tasks) {
                Task tempTask = new Task();
                // tempTask.simplicateId = simplicate.AddHours(task);
                // tempTask.teamweekId = teamweek.AddTeamweekTask(task, jsonData.config);
                jsonData.tasks.Add(tempTask);
            }

            Data tempData = jsonData;

            jsonData.WriteToJson();
        }

        /// <summary>
        /// Reads the `plannings.csv` and returns a Data object the represents the content of the `csv` file
        /// </summary>
        private static Data ReadPlanning () 
        {
            Data data = new Data();
            Dictionary<int, Employee> cellEmployees = new Dictionary<int, Employee>();

            using(var reader = new StreamReader(@"./planning.csv")) {

                string[] line = reader.ReadLine().Split(',');
                Array.Resize(ref line, line.Length);

                // For each employee (loop trough first row, skip first 3 columns)
                for (int i = 3; i < line.Length; i++) {

                    // Get employee names from first line
                    Employee employee = null;
                    string[] names = line[i].Trim().Split('-');
                    string firstName = names[0];
                    string lastName = names[1];

                    // Get employee from Json 
                    foreach (var item in jsonData.employees) {
                        if (item.firstName == firstName && 
                            item.lastName == lastName && 
                            item.teamweekId > 0
                        ) {
                            employee = item;
                            break;
                        }
                    }

                    // If employee not found in employee.json, try get Teamweek and Simplicate ID
                    if (employee == null) {
                        string simplicateId = simplicate.EmployeeNameToId(firstName, lastName);
                        int teamweekId = teamweek.EmployeeNameToId(firstName, lastName, jsonData.config);
                        employee = new Employee(firstName, lastName, teamweekId, simplicateId);
                        jsonData.employees.Add(employee);
                    }
                    
                    data.employees.Add(employee);
                    cellEmployees.Add(i, employee);
                }

                // Loop trough all tasks
                while (!reader.EndOfStream) {

                    line = reader.ReadLine().Split(',');

                    if (line[0].Trim() == "") 
                        break;

                    for (int i = 3; i < line.Length; i++) {
                        
                        string[] taskString = line[i].Split('/');

                        DateTime start = TimeStringToDateTime(line[0], line[1]);
                        DateTime end = TimeStringToDateTime(line[0], line[2]);

                        Task task = new Task() {
                            start = start, 
                            end = end, 
                            hours = (end - start).TotalHours,
                            employee = cellEmployees[i],
                            taskString = line[i]
                        };

                        if (taskString.Length > 1) {

                            Project project = null;

                            // Find IDs in project.json      
                            string projectName = taskString[0].Split('-')[1];
                            foreach (var item in jsonData.projects) {
                                if (item.name.ToLower().Replace(" ", "").Contains(projectName.ToLower())) {
                                    project = item;
                                    break;
                                }
                            }
                            
                            // If ID not found in project.json, try get IDs from Teamweek and Simplicate API
                            if (project == null) {
                                string simplicateId = simplicate.GuessIdFromProjectName(projectName);
                                int teamweekId = teamweek.ProjectNameToId(projectName, jsonData.config);
                                project = new Project(projectName, teamweekId, simplicateId);
                                jsonData.projects.Add(project);
                            }

                            task.project = project;
                            task.name = taskString[1];

                            if (project.simplicateId != "" || project.simplicateId != null) {
                                List<ProjectService> services = simplicate.GetProjectServices(project.simplicateId);
                                
                                if (services != null) {
                                    ProjectService service = services[0];
                                    task.serviceId = service.id;
                                    task.hoursTypeId = service.hoursTypes[0].id;
                                }
                            }
                        } else {
                            task.name = taskString[0];
                        }
                        
                        data.tasks.Add(task);
                    }
                }
            }

            return data;
        }
        
        private static DateTime TimeStringToDateTime (string date, string time) 
        {
            string[] dateString = date.Split('/');
            string[] timeString = time.Split(':');

            List<int> dateInt = new List<int>();
            List<int> timeInt = new List<int>();

            for (int index = 0; index < dateString.Length; index++)
                dateInt.Add(Convert.ToInt32(dateString[index]));
            for (int index = 0; index < timeString.Length; index++)
                timeInt.Add(Convert.ToInt32(timeString[index]));
            
            return new DateTime(dateInt[2], dateInt[1], dateInt[0], timeInt[0], timeInt[1], timeInt[2]);
        }
    }
}
