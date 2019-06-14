using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace AutoplannerConnections
{
    class Simplicate
    {

        public RestClient client;
        
        public Simplicate () 
        {
            client = new RestClient("https://hoppingerdemo2.simplicate.nl");

            GetProjects();
            GetProjectServices("project:f0bda8c63005f5d5152ff8f695efdc4c");
        }

        public string AddHours (Task task) 
        {
            var request = new RestRequest($"api/v2/hours/hours", Method.POST);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            request.AddJsonBody(
                new { 
                    employee_id         = task.employee.simplicateId,
                    project_id          = "project:d906c8651701f4304c13c77ab857ae53",   // Todo: Make this value non-static
                    projectservice_id   = "service:b1df599786b3076dbf0d1878087571b9",   // Todo: Make this value non-static
                    type_id             = "hourstype:36e980b8e87bd4a3ca9dc23db773baf0", // Todo: Make this value non-static
                    approvalstatus_id   = "approvalstatus:c50aea8b97ac61db",            // Add hours as it has yet to be approved
                    hours               = task.hours,
                    start_date          = task.start.ToString("yyyy-MM-dd HH:mm:ss"),
                    end_date            = task.end.ToString("yyyy-MM-dd HH:mm:ss"),
                    is_time_defined     = true,
                    note                = task.name,
                    source              = "schedule"
                }
            );

            IRestResponse response = this.client.Execute(request);
            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
            if (response.IsSuccessful) {
                return responseObject["data"]["id"];
            }   
            return "";         
        }

        public bool RemoveHours (string taskId) 
        {
            var request = new RestRequest($"api/v2/hours/hours/{taskId}", Method.DELETE);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            IRestResponse response = Teamweek.client.Execute(request);
            return response.IsSuccessful;

        }

        /// <summary>
        /// Returns the ID of the employee whose name contains the given name
        /// </summary>
        public string EmployeeNameToId (string name) 
        {
            var request = new RestRequest($"api/v2/hrm/employee", Method.GET);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            request.AddParameter("q[name]", $"*{name}*");

            IRestResponse response = this.client.Execute(request);
            dynamic responsObject = JsonConvert.DeserializeObject<dynamic>(response.Content);

            try {
                return responsObject["data"][0]["id"];
            } catch {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Could not find the Simplicate id for employee name '{name}'");
                Console.ResetColor();
                return "";
            }
        }

        /// <summary>
        /// Returns the ID of the project which name contains the given name
        /// </summary>
        public string ProjectNameToId (string name) 
        {
            var request = new RestRequest($"api/v2/projects/project", Method.GET);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            request.AddParameter("q[name]", $"*{name}*");

            IRestResponse response = this.client.Execute(request);
            dynamic responsObject = JsonConvert.DeserializeObject<dynamic>(response.Content);

            try {
                return responsObject["data"][0]["id"];
            } catch {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Could not find the Simplicate id for project name '{name}'");
                Console.ResetColor();
                return "";
            }
        }

        public void GetProjects () 
        {
            var request = new RestRequest($"api/v2/projects/project", Method.GET);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            IRestResponse response = this.client.Execute(request);
            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
        }

        public void GetProjectServices (string id) 
        {
            var request = new RestRequest($"api/v2/projects/service", Method.GET);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            request.AddParameter("q[project_id]",   id);
            request.AddParameter("select",          "[hour_types,name]");

            IRestResponse response = this.client.Execute(request);
            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
        }
    }
}