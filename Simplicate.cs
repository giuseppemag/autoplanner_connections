using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;

namespace AutoplannerConnections
{
    class Simplicate
    {

        private RestClient client;
        public const string url = "https://hoppingerdemo2.simplicate.nl";
        public const string authenticationKey = "guF5DqZqJ8cDxtKe29G6VQjg8pZwN2zT";
        public const string authenticationSecret = "m5PQlF6VD8o1sY5kXChJiqPhGFEjx33K";
        
        public Simplicate () 
        {
            client = new RestClient(url);
        }

        /// <summary>
        /// Adds the given task to the Simplicate environment
        /// </summary>
        public string AddHours (Task task) 
        {
            bool hasSimplicateIds = task.project != null && task.project.simplicateId != "" && task.serviceId != "" && task.hoursTypeId != "";

            var request = CreateRequest($"api/v2/hours/hours", Method.POST);
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
            var request = CreateRequest($"api/v2/hours/hours/{taskId}", Method.DELETE);

            IRestResponse response = client.Execute(request);
            return response.IsSuccessful || response.StatusDescription == "Not Found";
        }

        /// <summary>
        /// Returns the ID of the employee whose name contains the given name
        /// </summary>
        public string EmployeeNameToId (string firstName, string lastName) 
        {
            var request = CreateRequest($"api/v2/hrm/employee", Method.GET);
            request.AddParameter("q[name]", $"*{firstName}*");
            request.AddParameter("q[name]", $"*{lastName}*");

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
                Console.WriteLine($"Could not find the Simplicate id for employee name '{firstName}'");
                Console.ResetColor();
                return null;
            } else if (multipleNames) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Multiple Simplicate employees have the name '{firstName}'");
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
            var request = CreateRequest($"api/v2/projects/project", Method.GET);
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
            var request = CreateRequest($"api/v2/projects/project", Method.GET);

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
            var request = CreateRequest($"api/v2/projects/service", Method.GET);
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

        private IRestRequest CreateRequest(string location, Method method) {
            IRestRequest request = new RestRequest(location, method);
            request.AddHeader("Authentication-Key", authenticationKey);
            request.AddHeader("Authentication-Secret", authenticationSecret);
            return request;
        }
    }
}