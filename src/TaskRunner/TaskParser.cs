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
            List<string> list = new List<string>();

            try
            {
                string document = File.ReadAllText(configPath);
                JObject root = JObject.Parse(document);

                JToken scripts = root["scripts"];

                if (scripts != null) {
                    var children = scripts.Children<JProperty>();

                    foreach (var child in children)
                    {
                        list.Add(child.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }

            return list.OrderBy(k => k);
        }
    }
}
