using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace NpmTaskRunner
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Constants.VERSION, IconResourceID = 400)]
    [Guid(VSPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : Package
    {
        public const string PackageGuidString = "68f5ee87-8633-4f4c-8849-fdb6e22ef84a";

        protected override void Initialize()
        {
            Logger.Initialize(this, Constants.VSIX_NAME);
            Telemetry.Initialize(this, Constants.VERSION, "27e387d5-9428-4617-b79f-bcc80d4247b0");

            base.Initialize();
        }
    }
}
