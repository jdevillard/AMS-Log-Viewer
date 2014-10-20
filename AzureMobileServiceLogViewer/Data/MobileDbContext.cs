using AzureMobileServiceLogViewer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMobileServiceLogViewer.Data
{
    public class MobileDbContext : DbContext, IObjectContextAdapter
    {
        private string tableName_;

        public MobileDbContext()
            : this(null)
        {
        }

        public MobileDbContext(string tableName)
            : base(nameOrConnectionString: "AzureMobileServiceLogViewer.Properties.Settings.LogViewerConnectionString")
        {

            tableName_ = tableName;
        }

        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<MobileService> MobileServices { get; set; }
    }

    public class DbConnection
    {
        private String connectionString = System.Configuration
            .ConfigurationManager.ConnectionStrings["AzureMobileServiceLogViewer.Properties.Settings.LogViewerConnectionString"].ToString();

        private string tableName_;

        public DbConnection() : this(null)
        {

        }

        public DbConnection(string tableName)
        {
            tableName_ = tableName;
            if (tableName != null)
                CheckIfTableExist(tableName);
        }

        public IList<Models.Subscription> AddSubscription(IEnumerable<Models.Subscription> subscriptionsToAdd)
        {
            var subInDb = GetSubscription();

            using (var connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                foreach (var item in subscriptionsToAdd)
                {
                    if (subInDb.Any(u => u.Id == item.Id))
                    {

                    }
                    else
                    {
                        using (var command = new SqlCommand("insert into [dbo].[subscriptions] (id, name) values (@id,@name)", connection))
                        {
                            command.Parameters.Add("id", SqlDbType.UniqueIdentifier).Value = item.Id;
                            command.Parameters.Add("name", SqlDbType.NVarChar).Value = item.Name;
                            command.ExecuteNonQuery();

                        }
                    }

                }
            }

            return GetSubscription();
        }

        public IList<Models.Subscription> GetSubscription()
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(String.Format("select * from [dbo].subscriptions"), connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                var table = new DataTable();
                int count = adapter.Fill(table);

                var result = new List<Models.Subscription>();

                foreach (DataRow item in table.Rows)
                {
                    result.Add(new Models.Subscription()
                    {
                        Id = Guid.Parse(item["Id"].ToString()),
                        Name = item["Name"].ToString(),

                    });
                }

                return result;
            }
        }

        public IEnumerable<Models.Result> GetData()
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(String.Format("select * from [dbo].[{0}]", tableName_), connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                var table = new DataTable();
                int count = adapter.Fill(table);

                var result = new List<Models.Result>();

                foreach (DataRow  item in  table.Rows)
                {
                    result.Add(new Models.Result()
                    {
                        message = item["message"].ToString(),
                        source = item["source"].ToString(),
                        type = item["type"].ToString(),
                        timeCreated = (DateTime) item["timeCreated"]
                    });
                }

                return result;
            }
        }

        public void InsertData(IEnumerable<Models.Result> result)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                foreach (var item in result)
                {
                    using (var command = new SqlCommand(String.Format("insert into [dbo].[{0}] (Source, Message, Type, TimeCreated) values (@source,@message,@type,@timeCreated)",
                        tableName_, item.source, item.message, item.type), connection))
                    {
                        command.Parameters.Add("source", SqlDbType.NVarChar).Value = item.source;
                        command.Parameters.Add("message", SqlDbType.NVarChar).Value = item.message;
                        command.Parameters.Add("type", SqlDbType.NVarChar).Value = item.type;
                        command.Parameters.Add("timeCreated", SqlDbType.DateTime2).Value = item.timeCreated;


                        command.ExecuteNonQuery();

                    }
                }
            }

        }


        private void CheckIfTableExist(string tableName)
        {
            var sqlQuery = String.Format(@"
                        if not exists (select * from sysobjects where name='{0}' and xtype='U')
                            CREATE TABLE [dbo].[{0}] (
                        [Id]            INT            IDENTITY (1, 1) NOT NULL,
                        [Message]       NVARCHAR (MAX) NULL,
                        [Source]        NVARCHAR (255) NOT NULL,
                        [Type]          NVARCHAR (50)  NOT NULL,
                        [TimeCreated]   DATETIME2       NOT NULL,
                        [MobileService] NVARCHAR (255) NULL,
                        PRIMARY KEY CLUSTERED ([Id] ASC) );
                    ", tableName);

            using (var connection = new SqlConnection(connectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    try
                    {
                        connection.Open();
                    }
                    catch
                    {
                        return;
                    }
                }


                using (var command = new SqlCommand(sqlQuery, connection))
                    command.ExecuteNonQuery();


            }

        }


    }

}
