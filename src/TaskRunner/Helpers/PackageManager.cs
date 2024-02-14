using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

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
            ColorOption = "--color",
            AlwaysTasks = new[] { "install", "update" },
            DefaultTasks = new[] { "test", "start", "version" },
        };

        private PackageManager() { }

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

        public static PackageManager FromManifestFile(string manifestFilePath)
        {
            var cwd = Path.GetDirectoryName(manifestFilePath);

            while (cwd != null)
            {
                if (TryGetByPackageManagerSpecificFiles(cwd, out var packageManager))
                {
                    return packageManager;
                }

                if (TryGetByCorepackPackageManager(cwd, out packageManager))
                {
                    return packageManager;
                }

                cwd = Path.GetDirectoryName(cwd);
            }

            return NPM;
        }

        /// <summary>
        /// Attempts to detect the package manager by looking for sibling files that are specific to a package manager.
        /// </summary>
        private static bool TryGetByPackageManagerSpecificFiles(string directoryPath, out PackageManager packageManager)
        {
            try
            {
                var yarnCleanPath = Path.Combine(directoryPath, ".yarnclean");
                var yarnConfigPath = Path.Combine(directoryPath, ".yarnrc");
                var yarnLockPath = Path.Combine(directoryPath, "yarn.lock");

                // if "yarn.lock", ".yarnrc", or ".yarnclean" file exists at same level as package.json, switch to Yarn CLI
                if (File.Exists(yarnCleanPath) || File.Exists(yarnConfigPath) || File.Exists(yarnLockPath))
                {
                    packageManager = Yarn;
                    return true;
                }

                var pnpmPath = Path.Combine(directoryPath, ".pnpmfile.cjs");
                var pnpmWorkspacePath = Path.Combine(directoryPath, "pnpm-workspace.yaml");
                var pnpmLockPath = Path.Combine(directoryPath, "pnpm-lock.yaml");

                // if ".pnpmfile.cjs", "pnpm-workspace.yaml", or "pnpm-lock.yaml" file exists at same level as package.json, switch to PNPM CLI
                if (File.Exists(pnpmPath) || File.Exists(pnpmWorkspacePath) || File.Exists(pnpmLockPath))
                {
                    packageManager = PNPM;
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }

            packageManager = null;
            return false;
        }

        /// <summary>
        /// When using corepack, the "packageManager" property will be set in the package.json file.
        /// See: https://nodejs.org/api/corepack.html#configuring-a-package
        /// </summary>
        private static bool TryGetByCorepackPackageManager(string directoryPath, out PackageManager packageManager)
        {
            try
            {
                var manifestFilePath = Path.Combine(directoryPath, Constants.FILENAME);
                if (!File.Exists(manifestFilePath))
                {
                    packageManager = null;
                    return false;
                }

                var json = File.ReadAllText(manifestFilePath);
                var parsed = JObject.Parse(json);
                var packageManagerProperty = parsed["packageManager"]?.Value<string>();
                if (string.IsNullOrWhiteSpace(packageManagerProperty))
                {
                    packageManager = null;
                    return false;
                }

                var packageManagerName = packageManagerProperty.Split('@').First();

                if (string.Equals(packageManagerName, NPM.CliCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    packageManager = NPM;
                    return true;
                }

                if (string.Equals(packageManagerName, Yarn.CliCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    packageManager = Yarn;
                    return true;
                }

                if (string.Equals(packageManagerName, PNPM.CliCommandName, StringComparison.OrdinalIgnoreCase))
                {
                    packageManager = PNPM;
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }

            packageManager = null;
            return false;
        }

    }
}
