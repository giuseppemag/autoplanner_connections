# autoplanner_connections

## CSV
For both Teamweek and Simplicate, there is no "real" way to convert a project and employee name to their respective `id`. Because of this, the code currrently guesses the name, by setting the name collected from the `csv` to lowercase and removing all spaces. For Teamweek it loops trough all employees and checks if the Teamweek name contains the csv name. For Simplicate, it acquires all employees that contain the converted string.

Because of the current method of "guessing" the name, errors may occur when 2 employees have the same first name. A last name is therefor required, which will be implemented later on.

The `csv` requires to be names `plannings.csv`. Any other name for the csv will not work.

## Teamweek
Because of how the Teamweek authentication keys work, the tool needs to run at least once every 2 weeks. If the time between runs takes longer then 2 weeks, a new refresh token is required for it to work again.

## Simplicate
Adding hours to the Simplicate API requires an `id` for not only the project and employee, but also for the `service` and `hoursType`. Every project has a different name for the service and hoursType. Therefor we currently acquire all services and hourstypes for the given project and we then pick the first of both these values, so the employee can change these values themselves.
