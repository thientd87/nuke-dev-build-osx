using Configurations;
using Nuke.Common.IO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Tasks
{
    public class DatabaseInitialization
    {
        private WorkingSpaceConfiguration _workingSpaceConfiguration;
        private AbsolutePath rootDirectory { get; set; }

        public DatabaseInitialization(AbsolutePath rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public static DatabaseInitialization Init(AbsolutePath rootDirectory) => new DatabaseInitialization(rootDirectory);

        public DatabaseInitialization WithWorkingSpaceConfiguration(WorkingSpaceConfiguration workingSpaceConfiguration)
        {
            _workingSpaceConfiguration = workingSpaceConfiguration;
            return this;
        }

        public DatabaseInitialization Run()
        {
            var bakPath = rootDirectory / _workingSpaceConfiguration.BakPath;
            var databaseName = _workingSpaceConfiguration.DatabaseName;

            var connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = _workingSpaceConfiguration.SqlServerAddress;
            connectionStringBuilder.UserID = "sa";
            connectionStringBuilder.Password = _workingSpaceConfiguration.SqlServerSAPassword;

            var connectionString = connectionStringBuilder.ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                var sql = $@"
                    IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{databaseName}')
                        BEGIN
                            RESTORE DATABASE {databaseName}
                            FROM DISK = '{bakPath}'
                            WITH REPLACE
                        END
                ";
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return this;
        }
    }
}
