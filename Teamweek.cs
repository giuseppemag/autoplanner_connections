using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace AutoplannerConnections
{
    class Teamweek
    {

        public static RestClient client;

        public int AddTeamweekTask (Task task, Config config) 
        {

            if (task.employee.teamweekId != -1) {
                var request = new RestRequest($"api/v4/147174/tasks?access_token={config.twAccessToken}", Method.POST);
                request.AddParameter("access_token", config.twAccessToken);
                request.AddParameter("name", task.name);
                request.AddParameter("start_date", task.start.ToString("yyyy-MM-dd"));
                request.AddParameter("end_date", task.end.ToString("yyyy-MM-dd"));
                request.AddParameter("start_time", task.start.ToString("HH:mm"));
                request.AddParameter("end_time", task.end.ToString("HH:mm"));
                request.AddParameter("estimated_hours", task.hours.ToString());
                request.AddParameter("user_id", task.employee.teamweekId.ToString());
                if (task.project != null && task.project.teamweekId > 0)
                    request.AddParameter("project_id", task.project.teamweekId.ToString());

                IRestResponse response = Teamweek.client.Execute(request);
                dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
                return (int)responseObject["id"];
            }
            return -1;
        }

        public bool RemoveTeamweekTask (int taskId, Config config)
        {
            var request = new RestRequest($"api/v4/147174/tasks/{taskId}", Method.DELETE);
            request.AddParameter("access_token", config.twAccessToken);

            IRestResponse response = Teamweek.client.Execute(request);
            return response.IsSuccessful;
        }

        public void RefreshAccessToken(ref Config config)
        {
            var request = new RestRequest($"api/v3/authenticate/token?grant_type=refresh_token&refresh_token={config.twRefreshToken}", Method.POST);
            request.AddHeader("Authorization", config.twSecretBase);
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", config.twRefreshToken);

            IRestResponse response = Teamweek.client.Execute(request);
            var responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
            config.twAccessToken = responseObject["access_token"];
            config.twRefreshToken = responseObject["refresh_token"];
        }
        
        public int EmployeeNameToId (string name, Config config) 
        {
            var request = new RestRequest($"api/v4/147174/members?access_token={config.twAccessToken}");
            request.AddHeader("authorization", $"Bearer {config.twAccessToken}");

            IRestResponse response = Teamweek.client.Execute(request);
            var responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);

            foreach (var employee in responseObject)
                if (((string)employee["name"]).ToString().ToLower().Replace(" ", "").Contains(name.ToLower().Replace(" ", "")))
                    return employee["id"];

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Could not find the Teamweek id for employee name '{name}'");
            Console.ResetColor();
            return -1;
        }

        public int ProjectNameToId (string name, Config config) 
        {
            var request = new RestRequest($"api/v4/147174/projects?access_token={config.twAccessToken}");
            request.AddHeader("authorization", $"Bearer {config.twAccessToken}");

            IRestResponse response = Teamweek.client.Execute(request);
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