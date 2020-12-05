using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cofoundry.Plugins.Azure.Tests
{
    public static class ConfigurationHelper
    {
        public static IConfigurationRoot GetConfigurationRoot()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            return new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
