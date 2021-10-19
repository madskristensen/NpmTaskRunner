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
    internal class TaskRunnerProvider : ITaskRunner
    {
        //private ImageSource _icon;
        private List<ITaskRunnerOption> _options = null;
        private string _cliCommandName = null;

        private void InitializeNpmTaskRunnerOptions()
        {
            _options = new List<ITaskRunnerOption>
            {
                new TaskRunnerOption("Verbose", PackageIds.cmdVerbose, PackageGuids.guidVSPackageCmdSet, false, Constants.NPM_VERBOSE_OPTION)
            };
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

        public Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            ITaskRunnerNode hierarchy = LoadHierarchy(configPath);

            if (!hierarchy.Children.Any() || !hierarchy.Children.First().Children.Any())
            {
                return Task.FromResult<ITaskRunnerConfig>(null);
            }

            return Task.FromResult<ITaskRunnerConfig>(new TaskRunnerConfig(context, hierarchy, _cliCommandName));
        }

        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            _cliCommandName = GetCliCommandName(configPath);
            var isNpm = (_cliCommandName == Constants.NPM_CLI_COMMAND);

            ITaskRunnerNode root = new TaskNode(Vsix.Name, false, isNpm);

            SortedList<string, string> scripts = TaskParser.LoadTasks(configPath, _cliCommandName);
            SortedList<string, IEnumerable<string>> hierarchy = GetHierarchy(scripts.Keys);

            IEnumerable<string> allDefaultTasks = (isNpm
                ? Constants.NPM_ALL_DEFAULT_TASKS
                : Constants.YARN_ALL_DEFAULT_TASKS);
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> defaults = hierarchy.Where(h => allDefaultTasks.Contains(h.Key));

            var defaultTasks = new TaskNode("Defaults", false, isNpm)
            {
                Description = $"Default predefined {_cliCommandName} commands."
            };
            root.Children.Add(defaultTasks);
            AddCommands(configPath, scripts, defaults, defaultTasks, isNpm);

            if (hierarchy.Count != defaults.Count())
            {
                IEnumerable<KeyValuePair<string, IEnumerable<string>>> customs = hierarchy.Except(defaults);

                var customTasks = new TaskNode("Custom", false, isNpm)
                {
                    Description = $"Custom {_cliCommandName} commands."
                };
                root.Children.Add(customTasks);

                AddCommands(configPath, scripts, customs, customTasks, isNpm);
            }

            return root;
        }

        private void AddCommands(string configPath, SortedList<string, string> scripts, IEnumerable<KeyValuePair<string, IEnumerable<string>>> commands, TaskNode tasks, bool isNpm)
        {
            var cwd = Path.GetDirectoryName(configPath);

            foreach (KeyValuePair<string, IEnumerable<string>> parent in commands)
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
            var colorConfig = (isNpm ? "--color=always" : string.Empty);

            return new TaskNode(name, !string.IsNullOrEmpty(cmd), isNpm)
            {
                Command = new TaskRunnerCommand(cwd, "cmd.exe", $"/c {cmd} {colorConfig}"),
                Description = $"Runs the '{name}' command"
            };
        }

        private SortedList<string, IEnumerable<string>> GetHierarchy(IEnumerable<string> alltasks)
        {
            if (alltasks == null || !alltasks.Any())
            {
                return null;
            }

            IEnumerable<string> events = alltasks.Where(t => t.StartsWith(Constants.PRE_SCRIPT_PREFIX) || t.StartsWith(Constants.POST_SCRIPT_PREFIX));
            var parents = alltasks.Except(events).ToList();

            var hierarchy = new SortedList<string, IEnumerable<string>>();

            foreach (var parent in parents)
            {
                IOrderedEnumerable<string> children = GetChildScripts(parent, events).OrderByDescending(child => child);
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
            IEnumerable<string> candidates = events.Where(e => e.EndsWith(parent, StringComparison.OrdinalIgnoreCase));

            foreach (var candidate in candidates)
            {
                if (candidate.StartsWith(Constants.POST_SCRIPT_PREFIX) && Constants.POST_SCRIPT_PREFIX + parent == candidate)
                {
                    yield return candidate;
                }

                if (candidate.StartsWith(Constants.PRE_SCRIPT_PREFIX) && Constants.PRE_SCRIPT_PREFIX + parent == candidate)
                {
                    yield return candidate;
                }
            }
        }

        private static string GetCliCommandName(string configPath)
        {
            var cwd = Path.GetDirectoryName(configPath);

            var yarnCleanPath = Path.Combine(cwd, ".yarnclean");
            var yarnConfigPath = Path.Combine(cwd, ".yarnrc");
            var yarnLockPath = Path.Combine(cwd, "yarn.lock");

            // if "yarn.lock", ".yarnrc", or ".yarnclean" file exists at same level as package.json, switch to Yarn CLI
            if (File.Exists(yarnCleanPath) || File.Exists(yarnConfigPath) || File.Exists(yarnLockPath))
            {
                return Constants.YARN_CLI_COMMAND;
            }

            return Constants.NPM_CLI_COMMAND;
        }
    }
}
