namespace NpmTaskRunner
{
    class Constants
    {
        public const string FILENAME = "package.json";
        public const string POST_SCRIPT_PREFIX = "post";
        public const string PRE_SCRIPT_PREFIX = "pre";

        public static readonly string[] RESTART_SCRIPT_TASKS = { "prerestart", "prestop", "stop", "poststop", "restart", "prestart", "start", "poststart", "postrestart" };
    }
}
