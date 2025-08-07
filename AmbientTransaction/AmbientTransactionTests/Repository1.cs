using AmbientTransaction;
using Microsoft.Data.SqlClient;

namespace AmbientTransactionTests
{
    public class Repository1
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public Repository1(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task DoSingleWork(string id)
        {
            await using (var cn = _dbConnectionFactory.GetOpenConnection(out var dbTransaction))
            {
                var cmd = cn.CreateCommand();
                cmd.Transaction = dbTransaction;
                cmd.CommandText = ($"insert into table_1 (id) values ('{id}')");
                await cmd.ExecuteNonQueryAsync();
            }

        }

        public async Task DoMultipleWorkInTransaction(string id, string id2)
        {
            await using (var scope = AmbientConnectionScope.Create(_dbConnectionFactory.ConnectionString))
            {

                await using (var cn = _dbConnectionFactory.GetOpenConnection(out var dbTransaction))
                {
                    var cmd = cn.CreateCommand();
                    cmd.Transaction = dbTransaction;
                    cmd.CommandText = ($"insert into table_1 (id) values ('{id}')");
                    await cmd.ExecuteNonQueryAsync();
                    cmd.CommandText = ($"insert into table_1 (id) values ('{id2}')");
                    await cmd.ExecuteNonQueryAsync();
                }
                scope.Complete();
            }
        }

        public async Task do1WantToStayOutsideAmbientTransaction(string id)
        {
            await using (var cn = new SqlConnection(_dbConnectionFactory.ConnectionString))
            {
                await cn.OpenAsync();
                var cmd = cn.CreateCommand();
                cmd.CommandText = ($"insert into table_1 (id) values ('{id}')");
                await cmd.ExecuteNonQueryAsync();
            }

        }
    }
}
