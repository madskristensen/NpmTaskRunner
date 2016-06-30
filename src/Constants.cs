using System.Collections.Generic;
using System.Linq;

namespace NpmTaskRunner
{
    class Constants
    {
        public const string ELEMENT_NAME = "-vs-binding";
        public const string FILENAME = "package.json";
        public const string POST_SCRIPT_PREFIX = "post";
        public const string PRE_SCRIPT_PREFIX = "pre";

        public static readonly string[] ALWAYS_TASKS = { "install", "update" };
        public static readonly string[] DEFAULT_TASKS = { "test", "uninstall", "restart", "start", "stop", "version" };
        public static readonly string[] RESTART_SCRIPT_TASKS = { "prerestart", "prestop", "stop", "poststop", "restart", "prestart", "start", "poststart", "postrestart"};
        public static readonly IEnumerable<string> ALL_DEFAULT_TASKS = ALWAYS_TASKS.Union(DEFAULT_TASKS);
    }
}
