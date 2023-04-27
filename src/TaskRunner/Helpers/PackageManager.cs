using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NpmTaskRunner.Helpers
{
    public class PackageManager
    {
        public readonly static PackageManager NPM = new PackageManager
        {
            CliCommandName = "npm",
            VerboseOption = "-d",
            ColorOption = "--color=always",
            AlwaysTasks = new[] { "install", "update" },
            DefaultTasks = new[] { "test", "uninstall", "restart", "start", "stop", "version" },
        };

        public readonly static PackageManager Yarn = new PackageManager
        {
            CliCommandName = "yarn",
            VerboseOption = "--verbose",
            ColorOption = string.Empty,
            AlwaysTasks = new[] { "install", "upgrade" },
            DefaultTasks = new[] { "test", "version" },
        };

        public readonly static PackageManager PNPM = new PackageManager
        {
            CliCommandName = "pnpm",
            VerboseOption = "--long",
            ColorOption = string.Empty,
            AlwaysTasks = new[] { "install", "update" },
            DefaultTasks = new[] { "test", "start", "version" },
        };

        public string CliCommandName { get; private set; }
        public string VerboseOption { get; private set; }
        public string ColorOption { get; private set; }
        public string[] AlwaysTasks { get; private set; }
        public string[] DefaultTasks { get; private set; }
        public IEnumerable<string> AllDefaultTasks => AlwaysTasks.Union(DefaultTasks);

        public override bool Equals(object obj)
        {
            return obj is PackageManager other && this.CliCommandName == other.CliCommandName;
        }

        public override int GetHashCode()
        {
            return this.CliCommandName.GetHashCode();
        }

        public override string ToString()
        {
            return this.CliCommandName;
        }

        public static PackageManager DetectByPath(string configPath)
        {
            var cwd = Path.GetDirectoryName(configPath);

            while (cwd != null)
            {
                var yarnCleanPath = Path.Combine(cwd, ".yarnclean");
                var yarnConfigPath = Path.Combine(cwd, ".yarnrc");
                var yarnLockPath = Path.Combine(cwd, "yarn.lock");

                // if "yarn.lock", ".yarnrc", or ".yarnclean" file exists at same level as package.json, switch to Yarn CLI
                if (File.Exists(yarnCleanPath) || File.Exists(yarnConfigPath) || File.Exists(yarnLockPath))
                {
                    return Yarn;
                }

                var pnpmPath = Path.Combine(cwd, ".pnpmfile.cjs");
                var pnpmWorkspacePath = Path.Combine(cwd, "pnpm-workspace.yaml");
                var pnpmLockPath = Path.Combine(cwd, "pnpm-lock.yaml");

                // if ".pnpmfile.cjs", "pnpm-workspace.yaml", or "pnpm-lock.yaml" file exists at same level as package.json, switch to PNPM CLI
                if (File.Exists(pnpmPath) || File.Exists(pnpmWorkspacePath) || File.Exists(pnpmLockPath))
                {
                    return PNPM;
                }

                cwd = Path.GetDirectoryName(cwd);
            }

            return NPM;
        }
    }
}
