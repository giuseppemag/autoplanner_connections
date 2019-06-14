using System;
using System.Collections.Generic;
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
                    approvalstatus_id   = "approvalstatus:c50aea8b97ac61db",
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

            IRestResponse response = client.Execute(request);
            return response.IsSuccessful || response.StatusDescription == "Not Found";
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
            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);

            string employeeId = null;
            bool multipleNames = false;

            foreach (var employee in responseObject["data"]) {
                if (employeeId != null) 
                    multipleNames = true;
                employeeId = employee["id"];
            }

            if (employeeId == null) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Could not find the Teamweek id for employee name '{name}'");
                Console.ResetColor();
                return null;
            } else if (multipleNames) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Multiple employees have the name '{name}'");
                Console.ResetColor();
                return null;
            }

            return employeeId;
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

        public List<Project> GetProjects () 
        {
            var request = new RestRequest($"api/v2/projects/project", Method.GET);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            IRestResponse response = this.client.Execute(request);
            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
            List<Project> projects = new List<Project>();
            foreach (var project in responseObject["data"])
            {
                projects.Add(new Project() {simplicateId = project["id"], name = project["organization"]["name"]});
            }
            return projects;
        }

        public List<ProjectService> GetProjectServices (string id) 
        {
            var request = new RestRequest($"api/v2/projects/service", Method.GET);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            request.AddParameter("q[project_id]",   id);
            request.AddParameter("select",          "[hour_types,name]");

            IRestResponse response = this.client.Execute(request);
            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
            List<ProjectService> projectServices = new List<ProjectService>();
            foreach (var projectService in responseObject["data"])
            {
                ProjectService projectServiceObject = new ProjectService() {
                    id = projectService["id"],
                    name = projectService["name"],
                    hoursTypes = new List<HoursType>()
                };

                try {
                    foreach (var hoursType in projectService["hour_types"])
                    {
                        projectServiceObject.hoursTypes.Add(new HoursType() {
                            id = hoursType["hourstype"]["id"], 
                            type = hoursType["hourstype"]["type"], 
                            label = hoursType["hourstype"]["label"]
                        });
                    }
                } catch {}
            }
            return null;
        }
    }
}