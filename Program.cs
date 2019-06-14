using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RestSharp;

namespace AutoplannerConnections
{
    class Program
    {
        private static Data jsonData;
        private static Simplicate simplicate;
        private static Teamweek teamweek;

        static void Main(string[] args) 
        {

            Teamweek.client = new RestClient("https://teamweek.com");
            jsonData = new Data(fromJson: true);

            teamweek = new Teamweek();
            simplicate = new Simplicate();

            List<int> tempTeamweekIds = new List<int>();
            tempTeamweekIds.AddRange(jsonData.teamweekTaskIds);
            foreach (int item in tempTeamweekIds) {
                if (teamweek.RemoveTeamweekTask(item, jsonData.config)) {
                    jsonData.teamweekTaskIds.Remove(item);
                }
            }

            List<string> tempSimplicateIds = new List<string>();
            tempSimplicateIds.AddRange(jsonData.simplicateTaskIds);
            foreach (var item in tempSimplicateIds) {
                if (simplicate.RemoveHours(item)) {
                    jsonData.simplicateTaskIds.Remove(item);
                }
            }

            // Needs to be called at least once every 2 weeks (before refresh token expires and is unusable)
            teamweek.RefreshAccessToken(ref jsonData.config);

            Data planningData = ReadPlanning();
            jsonData.tasks.Clear();
            foreach (Task task in planningData.tasks) {
                Task tempTask = new Task();
                tempTask.simplicateId = simplicate.AddHours(task);
                tempTask.teamweekId = teamweek.AddTeamweekTask(task, jsonData.config);
                jsonData.tasks.Add(tempTask);
            }

            Data tempData = jsonData;

            jsonData.WriteToJson();
        }

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
                    string employeeName = line[i].Trim();
                    Employee employee = null;

                    // Get employee from Json 
                    foreach (var item in jsonData.employees) {
                        if (item.name.ToLower().Replace(" ", "").Contains(employeeName.ToLower()) && item.teamweekId > 0) {
                            employee = item;
                            break;
                        }
                    }

                    // If employee not found in employee.json, try get Teamweek and Simplicate ID
                    if (employee == null) {
                        string simplicateId = simplicate.EmployeeNameToId(employeeName);
                        int teamweekId = teamweek.EmployeeNameToId(employeeName, jsonData.config);
                        employee = new Employee(employeeName, teamweekId, simplicateId);
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
                        if (taskString.Length > 1) {

                            Project project = null;

                            // Find ID in project.json      
                            string projectName = taskString[0].Split('-')[1];
                            foreach (var item in jsonData.projects) {
                                if (item.name.ToLower().Replace(" ", "").Contains(projectName.ToLower())) {
                                    project = item;
                                    break;
                                }
                            }
                            
                            // If ID not found in project.json, try get ID from Teamweek API
                            if (project == null) {
                                string simplicateId = simplicate.ProjectNameToId(projectName);
                                int teamweekId = teamweek.ProjectNameToId(projectName, jsonData.config);
                                project = new Project(projectName, teamweekId, simplicateId);
                                jsonData.projects.Add(project);
                            }

                            data.tasks.Add(new Task(taskString[1], TimeStringToDateTime(line[0], line[1]), TimeStringToDateTime(line[0], line[2]), cellEmployees[i], project));
                        } else {
                            data.tasks.Add(new Task(taskString[0], TimeStringToDateTime(line[0], line[1]), TimeStringToDateTime(line[0], line[2]), cellEmployees[i]));
                        }
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
