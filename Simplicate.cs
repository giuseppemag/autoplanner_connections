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

        /// <summary>
        /// Adds the given task to the Simplicate environment
        /// </summary>
        public string AddHours (Task task) 
        {
            var request = new RestRequest($"api/v2/hours/hours", Method.POST);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            bool hasSimplicateIds = task.project != null && task.project.simplicateId != "" && task.serviceId != "" && task.hoursTypeId != "";

            request.AddJsonBody(
                new { 
                    employee_id         = task.employee.simplicateId,

                    // If task has no Simplicate id's, the hours will be registered as Hoppinger Indirect > Indirect > Diversen
                    project_id          = hasSimplicateIds
                                            ? task.project.simplicateId
                                            : "project:d906c8651701f4304c13c77ab857ae53",
                    projectservice_id   = hasSimplicateIds
                                            ? task.serviceId
                                            : "service:b1df599786b3076dbf0d1878087571b9",
                    type_id             = hasSimplicateIds
                                            ? task.hoursTypeId
                                            : "hourstype:36e980b8e87bd4a3ca9dc23db773baf0",
                    approvalstatus_id   = "approvalstatus:c50aea8b97ac61db",
                    hours               = task.hours,
                    start_date          = task.start.ToString("yyyy-MM-dd HH:mm:ss"),
                    end_date            = task.end.ToString("yyyy-MM-dd HH:mm:ss"),
                    is_time_defined     = true,
                    note                = hasSimplicateIds
                                            ? task.name
                                            : task.taskString,
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

        /// <summary>
        /// Removes a task with the given `id`. Returns true on success
        /// </summary>
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
        public string GuessIdFromProjectName (string name) 
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

        /// <summary>
        /// Returns a list of all projects in Simplicate
        /// </summary>
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

        /// <summary>
        /// Returns a list of all services in the project with the given `id`
        /// </summary>
        public List<ProjectService> GetProjectServices (string id) 
        {
            var request = new RestRequest($"api/v2/projects/service", Method.GET);

            request.AddHeader("Authentication-Key",     "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT");
            request.AddHeader("Authentication-Secret",  "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K");

            request.AddParameter("q[project_id]",   id);
            request.AddParameter("select",          "[hour_types,name,id]");

            IRestResponse response = this.client.Execute(request);
            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);
            List<ProjectService> projectServices = new List<ProjectService>();
            if (responseObject["data"] != null) {
                foreach (var responseProjectService in responseObject["data"])
                {
                    ProjectService projectService = new ProjectService() {
                        id = responseProjectService["id"],
                        name = responseProjectService["name"],
                        hoursTypes = new List<HoursType>()
                    };

                    try {
                        foreach (var hoursType in responseProjectService["hour_types"])
                        {
                            projectService.hoursTypes.Add(new HoursType() {
                                id = hoursType["hourstype"]["id"], 
                                type = hoursType["hourstype"]["type"], 
                                label = hoursType["hourstype"]["label"]
                            });
                        }
                    } catch {}
                    projectServices.Add(projectService);
                }

                return projectServices;
            }
            return null;
        }
    }
}