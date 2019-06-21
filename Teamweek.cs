using System;
using Newtonsoft.Json;
using RestSharp;

namespace AutoplannerConnections
{
    class Teamweek
    {
        public string secretBase;
        public string accessToken;
        public string refreshToken;

        private RestClient client;

        public Teamweek () {
            client = new RestClient("https://teamweek.com");
        }

        /// <summary>
        /// Adds the given task to the Teamweek environment 
        /// </summary>
        public int AddTeamweekTask (Task task) 
        {

            if (task.employee.teamweekId != -1) {
                var request = new RestRequest($"api/v4/147174/tasks?access_token={accessToken}", Method.POST);
                request.AddParameter("access_token", accessToken);
                request.AddParameter("name", task.name);
                request.AddParameter("start_date", task.start.ToString("yyyy-MM-dd"));
                request.AddParameter("end_date", task.end.ToString("yyyy-MM-dd"));
                request.AddParameter("start_time", task.start.ToString("HH:mm"));
                request.AddParameter("end_time", task.end.ToString("HH:mm"));
                request.AddParameter("estimated_hours", task.hours.ToString());
                request.AddParameter("user_id", task.employee.teamweekId.ToString());
                if (task.project != null && task.project.teamweekId > 0)
                    request.AddParameter("project_id", task.project.teamweekId.ToString());

                IRestResponse response = client.Execute(request);
                dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
                return (int)responseObject["id"];
            }
            return -1;
        }

        /// <summary>
        /// Removes a task with the given `id`. Returns true on success
        /// </summary>
        public bool RemoveTeamweekTask (int taskId)
        {
            var request = new RestRequest($"api/v4/147174/tasks/{taskId}", Method.DELETE);
            request.AddParameter("access_token", accessToken);

            IRestResponse response = client.Execute(request);
            return response.IsSuccessful || response.StatusDescription == "Not Found";
        }

        
        /// <summary>
        /// Needs to be called at least once every 2 weeks, to prevent the `RefreshAccessToken` to expire
        /// </summary>
        public void RefreshAccessToken()
        {
            var request = new RestRequest($"api/v3/authenticate/token?grant_type=refresh_token&refresh_token={refreshToken}", Method.POST);
            request.AddHeader("Authorization", secretBase);
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", refreshToken);

            IRestResponse response = client.Execute(request);
            var responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
            accessToken = responseObject["access_token"];
            refreshToken = responseObject["refresh_token"];
        }
        
        /// <summary>
        /// Returns the Teamweek `id` of the employee whose name contains the given name
        /// </summary>
        public int EmployeeNameToId (string firstName, string lastName) 
        {
            var request = new RestRequest($"api/v4/147174/members?access_token={accessToken}");
            request.AddHeader("authorization", $"Bearer {accessToken}");

            IRestResponse response = client.Execute(request);
            var responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);

            int possibleEmployeeId = -1;
            int employeeId = -1;

            foreach (var employee in responseObject) {
                string employeeName = ((string)employee["name"]).ToLower().Replace(" ", "");
                if (employeeName.Contains(firstName.ToLower().Replace(" ", ""))) {
                    possibleEmployeeId = employee["id"];
                    if (employeeName.Contains(lastName.ToLower().Replace(" ", ""))) {
                        employeeId = possibleEmployeeId;
                        break;
                    }
                }
            }

            if (employeeId == -1) {
                if (possibleEmployeeId == -1) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Could not find the Teamweek id for employee name '{firstName} {lastName}'");
                    Console.ResetColor();
                    return -1;
                } else {
                    return possibleEmployeeId;
                }
            }
            return employeeId;
        }

        /// <summary>
        /// Returns the Teamweek `id` of the project which name contains the given name
        /// </summary>
        public int ProjectNameToId (string name) 
        {
            var request = new RestRequest($"api/v4/147174/projects?access_token={accessToken}");
            request.AddHeader("authorization", $"Bearer {accessToken}");

            IRestResponse response = client.Execute(request);
            var responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);

            foreach (var project in responseObject) 
                if (((string)project["name"]).ToLower().Replace(" ", "").Contains(name.ToLower())) 
                    return project["id"];

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Could not find the Teamweek id for project name '{name}'");
            Console.ResetColor();
            return -1;
        }
    }
}