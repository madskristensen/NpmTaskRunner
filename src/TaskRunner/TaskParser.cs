using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NpmTaskRunner
{
    class TaskParser
    {
        static string[] _dependencies = { "dependencies", "devDependencies", "optionalDependencies" };

        public static SortedList<string, string> LoadTasks(string configPath, string cliCommandName)
        {
            var list = new SortedList<string, string>();

            try
            {
                string document = File.ReadAllText(configPath);
                JObject root = JObject.Parse(document);
                JToken scripts = root["scripts"];

                if (scripts != null)
                {
                    var children = scripts.Children<JProperty>();

                    foreach (var child in children)
                    {
                        if (!list.ContainsKey(child.Name))
                            list.Add(child.Name, $"{cliCommandName} run {child.Name}");
                    }
                }

                bool isNpm = (cliCommandName == Constants.NPM_CLI_COMMAND);
                string[] alwaysTasks = (isNpm
                    ? Constants.NPM_ALWAYS_TASKS
                    : Constants.YARN_ALWAYS_TASKS);

                // Only fill default tasks if any scripts are found
                foreach (var reserved in alwaysTasks)
                {
                    if (!list.ContainsKey(reserved))
                        list.Add(reserved, $"{cliCommandName} {reserved}");
                }

                AddMissingDefaultParents(list, cliCommandName, isNpm);

                if (isNpm)
                {
                    bool hasMatch = (from l in list
                                     from t in Constants.RESTART_SCRIPT_TASKS
                                     where l.Key == t
                                     select l).Any();

                    // Add "restart" node if RESTART_SCRIPT_TASKS contains anything in list
                    if (hasMatch)
                        list.Add("restart", $"{cliCommandName} restart");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }

            return list;
        }

        private static void AddMissingDefaultParents(SortedList<string, string> list, string cliCommandName, bool isNpm)
        {
            string[] DEFAULT_TASKS = (isNpm
                ? Constants.NPM_DEFAULT_TASKS
                : Constants.YARN_DEFAULT_TASKS);
            string[] prefixes = { Constants.PRE_SCRIPT_PREFIX, Constants.POST_SCRIPT_PREFIX };
            var newParents = new List<string>();

            foreach (var task in list.Keys)
                foreach (string prefix in prefixes)
                {
                    if (task.Length <= prefix.Length)
                        continue;

                    var parent = task.Substring(prefix.Length);

                    if (!newParents.Contains(parent) && task.StartsWith(prefix) && !list.ContainsKey(parent) && DEFAULT_TASKS.Contains(parent))
                        newParents.Add(parent);
                }

            foreach (var parent in newParents)
            {
                string cmd = parent == "version" ? null : $"{cliCommandName} {parent}";
                list.Add(parent, cmd);
            }
        }
    }
}
