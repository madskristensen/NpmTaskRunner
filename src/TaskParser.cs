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

            try {
                string document = File.ReadAllText(configPath);
                JObject root = JObject.Parse(document);

                var children = root["scripts"].Children<JProperty>();

                foreach (var child in children)
                {
                    list.Add(child.Name);
                }
            }
            catch
            {
                return null;
                // TODO: Implement logger
            }

            return list.OrderBy(k => k);
        }
    }
}
