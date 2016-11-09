using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TaskRunnerExplorer;
using NpmTaskRunner.Helpers;

namespace NpmTaskRunner
{
    [TaskRunnerExport(Constants.FILENAME)]
    class TaskRunnerProvider : ITaskRunner
    {
        private ImageSource _icon;
        private List<ITaskRunnerOption> _options = null;

        public TaskRunnerProvider()
        {
            _icon = new BitmapImage(new Uri(@"pack://application:,,,/NpmTaskRunner;component/Resources/npm.png"));
        }

        private void InitializeNpmTaskRunnerOptions()
        {
            _options = new List<ITaskRunnerOption>();
            _options.Add(new TaskRunnerOption("Verbose", PackageIds.cmdVerbose, PackageGuids.guidVSPackageCmdSet, false, Constants.NPM_VERBOSE_OPTION));
        }

        public List<ITaskRunnerOption> Options
        {
            get
            {
                if (_options == null)
                {
                    InitializeNpmTaskRunnerOptions();
                }

                return _options;
            }
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            ITaskRunnerNode hierarchy = LoadHierarchy(configPath);

            if (!hierarchy.Children.Any() || !hierarchy.Children.First().Children.Any())
                return null;

            return await Task.Run(() =>
            {
                return new TaskRunnerConfig(context, hierarchy, _icon);
            });
        }

        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            var cliCommandName = GetCliCommandName(configPath);
            bool isNpm = (cliCommandName == Constants.NPM_CLI_COMMAND);

            ITaskRunnerNode root = new TaskNode(Vsix.Name, false, isNpm);

            var scripts = TaskParser.LoadTasks(configPath, cliCommandName);
            var hierarchy = GetHierarchy(scripts.Keys);

            IEnumerable<string> allDefaultTasks = (isNpm
                ? Constants.NPM_ALL_DEFAULT_TASKS
                : Constants.YARN_ALL_DEFAULT_TASKS);
            var defaults = hierarchy.Where(h => allDefaultTasks.Contains(h.Key));

            TaskNode defaultTasks = new TaskNode("Defaults", false, isNpm);
            defaultTasks.Description = $"Default predefined {cliCommandName} commands.";
            root.Children.Add(defaultTasks);
            AddCommands(configPath, scripts, defaults, defaultTasks, isNpm);

            if (hierarchy.Count != defaults.Count())
            {
                var customs = hierarchy.Except(defaults);

                TaskNode customTasks = new TaskNode("Custom", false, isNpm);
                customTasks.Description = $"Custom {cliCommandName} commands.";
                root.Children.Add(customTasks);

                AddCommands(configPath, scripts, customs, customTasks, isNpm);
            }

            return root;
        }

        private void AddCommands(string configPath, SortedList<string, string> scripts, IEnumerable<KeyValuePair<string, IEnumerable<string>>> commands, TaskNode tasks, bool isNpm)
        {
            string cwd = Path.GetDirectoryName(configPath);

            foreach (var parent in commands)
            {
                TaskNode parentTask = CreateTask(cwd, parent.Key, scripts[parent.Key], isNpm);

                foreach (var child in parent.Value)
                {
                    TaskNode childTask = CreateTask(cwd, child, scripts[child], isNpm);
                    parentTask.Children.Add(childTask);
                }

                tasks.Children.Add(parentTask);
            }
        }

        private static TaskNode CreateTask(string cwd, string name, string cmd, bool isNpm)
        {
            string colorConfig = (isNpm ? "--color=always" : string.Empty);

            return new TaskNode(name, !string.IsNullOrEmpty(cmd), isNpm)
            {
                Command = new TaskRunnerCommand(cwd, "cmd.exe", $"/c {cmd} {colorConfig}"),
                Description = $"Runs the '{name}' command"
            };
        }

        private SortedList<string, IEnumerable<string>> GetHierarchy(IEnumerable<string> alltasks)
        {
            if (alltasks == null || !alltasks.Any())
                return null;

            var events = alltasks.Where(t => t.StartsWith(Constants.PRE_SCRIPT_PREFIX) || t.StartsWith(Constants.POST_SCRIPT_PREFIX));
            var parents = alltasks.Except(events).ToList();

            var hierarchy = new SortedList<string, IEnumerable<string>>();

            foreach (var parent in parents)
            {
                var children = GetChildScripts(parent, events).OrderByDescending(child => child);
                hierarchy.Add(parent, children);
                events = events.Except(children).ToList();
            }

            foreach (var child in events)
            {
                hierarchy.Add(child, Enumerable.Empty<string>());
            }

            return hierarchy;
        }

        private static IEnumerable<string> GetChildScripts(string parent, IEnumerable<string> events)
        {
            var candidates = events.Where(e => e.EndsWith(parent, StringComparison.OrdinalIgnoreCase));

            foreach (var candidate in candidates)
            {
                if (candidate.StartsWith(Constants.POST_SCRIPT_PREFIX) && Constants.POST_SCRIPT_PREFIX + parent == candidate)
                    yield return candidate;

                if (candidate.StartsWith(Constants.PRE_SCRIPT_PREFIX) && Constants.PRE_SCRIPT_PREFIX + parent == candidate)
                    yield return candidate;
            }
        }

        private static string GetCliCommandName(string configPath)
        {
            string cwd = Path.GetDirectoryName(configPath);

            string yarnCleanPath = Path.Combine(cwd, ".yarnclean");
            string yarnLockPath = Path.Combine(cwd, "yarn.lock");

            // if "yarn.lock" or ".yarnclean" file exist at same level as package.json, switch to Yarn CLI
            if (File.Exists(yarnCleanPath) || File.Exists(yarnLockPath))
                return Constants.YARN_CLI_COMMAND;

            return Constants.NPM_CLI_COMMAND;
        }
    }
}
