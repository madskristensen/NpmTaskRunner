using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NpmTaskRunner
{
    class TaskParser
    {
        public static IEnumerable<string> LoadTasks(string configPath)
        {
            var list = new List<string>();

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
                        if (!list.Contains(child.Name))
                            list.Add(child.Name);
                    }

                    AddParentIfOrphansExist(list, Constants.RESERVED_TASKS);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }

            return list.OrderBy(k => k);
        }

        private static void AddParentIfOrphansExist(List<string> list, params string[] parents)
        {
            string[] prefixes = { "pre", "post" };

            foreach (var parent in parents)
            {
                if (list.Contains(parent, StringComparer.OrdinalIgnoreCase))
                    continue;

                foreach (var prefix in prefixes)
                {
                    if (list.Contains(prefix + parent, StringComparer.OrdinalIgnoreCase))
                    {
                        list.Add(parent);
                        break;
                    }
                }
            }
        }
    }
}
