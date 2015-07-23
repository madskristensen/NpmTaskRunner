using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace NpmTaskRunner
{
    class TaskParser
    {
        public static Dictionary<string, string> LoadTasks(string configPath)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic.Add("serve", "broccoli serve");
            dic.Add("build", "broccoli build dist");

            return dic;
        }

        //public static Dictionary<string, string> LoadTasks(string configPath)
        //{
        //    Dictionary<string, string> dic = new Dictionary<string, string>();

        //    try {
        //        string document = File.ReadAllText(configPath);
        //        JObject root = JObject.Parse(document);

        //        var children = root["scripts"].Children<JProperty>();

        //        foreach (var child in children)
        //        {
        //            dic.Add(child.Name, child.Value.ToString());
        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //        // TODO: Implement logger
        //    }

        //    return dic;
        //}
    }
}
