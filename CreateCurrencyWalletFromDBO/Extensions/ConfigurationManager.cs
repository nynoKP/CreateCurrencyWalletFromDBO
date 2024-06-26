using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Extensions
{
    public static class ConfigurationManager
    {
        private static IConfigurationRoot configuration;

        static ConfigurationManager()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public static IConfigurationRoot Configuration => configuration;

        public static T GetSection<T>(string key)
        {
            return configuration.GetSection(key).Get<T>();
        }
    }
}
