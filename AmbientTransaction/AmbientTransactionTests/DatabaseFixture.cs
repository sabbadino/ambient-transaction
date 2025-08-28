using AmbientTransactionTests;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AssemblyFixture(typeof(DatabaseFixture))]


namespace AmbientTransactionTests
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            var configuration = new ConfigurationBuilder().AddUserSecrets<DatabaseFixture>().Build();
            _connectionString = configuration["cnString"];
        }
        private string _connectionString;
        public string ConnectionString => _connectionString;
        public void Dispose()
        {
            // Cleanup after all tests
        }
    }
}
