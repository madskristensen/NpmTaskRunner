using System.Collections.Generic;
using System.Linq;

namespace NpmTaskRunner
{
    class Constants
    {
        public const string FILENAME = "package.json";
        public const string ELEMENT_NAME = "-vs-binding";
        public const string TASK_CATEGORY = "NPM Scripts";

        public static readonly string[] ALWAYS_TASKS = { "install", "uninstall", "publish", "restart" };
        public static readonly string[] DEFAULT_TASKS = { "test", "start", "stop", "version" };
        public static readonly IEnumerable<string> ALL_DEFAULT_TASKS = ALWAYS_TASKS.Union(DEFAULT_TASKS);
    }
}
