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

        public static SortedList<string, string> LoadTasks(string configPath)
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
                        AddTasks(list, child.Name);
                    }

                    // Only fill default tasks if any scripts are found
                    if (list.Any())
                    {
                        foreach (var reserved in Constants.ALWAYS_TASKS)
                        {
                            if (!list.ContainsKey(reserved))
                                list.Add(reserved, $"npm {reserved}");
                        }

                        AddMissingDefaultParents(list);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return list;
        }

        private static void AddMissingDefaultParents(SortedList<string, string> list)
        {
            string[] prefixes = { "pre", "post" };
            var newParents = new List<string>();

            foreach (var task in list.Keys)
                foreach (string prefix in prefixes)
                {
                    if (task.Length <= prefix.Length)
                        continue;

                    var parent = task.Substring(prefix.Length);

                    if (!newParents.Contains(parent) && task.StartsWith(prefix) && !list.ContainsKey(parent) && Constants.DEFAULT_TASKS.Contains(parent))
                        newParents.Add(parent);
                }

            foreach (var parent in newParents)
            {
                string cmd = parent == "version" ? null : $"npm {parent}";
                list.Add(parent, cmd);
            }
        }

        public static IDictionary<string, List<string>> LoadDependencies(string configPath)
        {
            var dic = new Dictionary<string, List<string>>();

            try
            {
                string document = File.ReadAllText(configPath);
                JObject root = JObject.Parse(document);

                foreach (var dep in _dependencies)
                {
                    JToken scripts = root[dep];

                    if (scripts != null)
                    {
                        dic[dep] = new List<string>();

                        var children = scripts.Children<JProperty>();

                        foreach (var child in children)
                        {
                            dic[dep].Add(child.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }

            return dic;
        }

        private static void AddTasks(SortedList<string, string> list, string child)
        {
            if (list.ContainsKey(child))
                return;

            if (child.Equals("install", StringComparison.OrdinalIgnoreCase))
                list.Add("install", "npm install");

            //else if (child.Equals("uninstall", StringComparison.OrdinalIgnoreCase))
            //    list.Add("uninstall", null);

            else
                list.Add(child, $"npm run {child}");
        }
    }
}
