using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace RestaurantApp.UI
{
    public static class ConfigurationHelper
    {
        private static IConfiguration _configuration;

        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    _configuration = builder.Build();
                }

                return _configuration;
            }
        }

        public static string GetConnectionString(string name)
        {
            return Configuration.GetConnectionString(name);
        }
    }
}