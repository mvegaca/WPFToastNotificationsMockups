using System;
using System.Diagnostics;
using System.Reflection;

using LightApp.Contracts.Services;

using OSVersionHelper;

using Windows.ApplicationModel;

namespace LightApp.Services
{
    public class ApplicationInfoService : IApplicationInfoService
    {
        public ApplicationInfoService()
        {
        }

        public Version GetVersion()
        {
            if (WindowsVersionHelper.HasPackageIdentity)
            {
                //// MSIX distribuition
                //// Setup the App Version in LightApp.Packaging > Package.appxmanifest > Packaging > PackageVersion
                var packageVersion = Package.Current.Id.Version;
                return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            }

            //// Setup the App Version in LightApp > Properties > Package > PackageVersion
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            return new Version(version);
        }
    }
}
