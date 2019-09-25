using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Data.Entity.Sqlite.Utilities;
using System.Data.Entity.Infrastructure.Interception;

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Instances of this class are used to create DbConnection objects for
    /// SQLite based on a given database name or connection string.
    /// </summary>
    public sealed class SqliteConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameOrConnectionString">db connection string</param>
        /// <returns></returns>
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            var factory = DbConfiguration.DependencyResolver.GetService(typeof(DbProviderFactory), "System.Data.SQLite.EF6") as DbProviderFactory;
            
            var connection = factory.CreateConnection();
            if (connection == null)
            {
                throw new Exception("The provider factory returned a null connection");
            }

            string connectionString;
            if (nameOrConnectionString.IndexOf('=') >= 0)
            {
                connectionString = nameOrConnectionString;
            }
            else
            {
                if (!nameOrConnectionString.EndsWith(".db", ignoreCase: true, culture: null))
                {
                    nameOrConnectionString += ".db";
                }

                connectionString = String.Format(CultureInfo.InvariantCulture, "Data Source={0}; ", nameOrConnectionString);
            }

            DbInterception.Dispatch.Connection.SetConnectionString(
               connection,
               new DbConnectionPropertyInterceptionContext<string>().WithValue(connectionString));

            return connection;
        }
    }
}
