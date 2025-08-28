using AmbientTransaction;
using Castle.DynamicProxy;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Xunit.Sdk;

namespace AmbientTransactionTests
{

        public class UnitTest1Take2
    {
        private readonly DatabaseFixture _databaseFixture;

        public UnitTest1Take2(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }
        

        [Fact]
        public async Task TestAmbientConnectionScopeDoMultipleWorkInTransactionCommit()
        {
            var cnString = _databaseFixture.ConnectionString;
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await Task.Delay(100); // Ensure different timestamps for inserts  
            var insert2 = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await using (var scope = AmbientTransactionScope.Create(cnString))
            {
                var r = new Repository1(new DbConnectionFactory(cnString));
                await r.DoMultipleWorkInTransaction(insert,insert2);
                scope.Complete();
            }
            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmd2 = cn.CreateCommand();
            cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == insert);
            cmd2.CommandText = $"select id from table_1 where id = '{insert2}'";
            value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == insert2);

        }

        [Fact]
        public async Task TestAmbientConnectionScopeDoMultipleWorkInTransactionRollBack()
        {
            var cnString = _databaseFixture.ConnectionString;
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await Task.Delay(100); // Ensure different timestamps for inserts  
            var insert2 = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");

            var dbConnectionFactory = new DbConnectionFactory(cnString);
            await using (var scope = AmbientTransactionScope.Create(dbConnectionFactory.ConnectionString))
            {
                var r = new Repository1(dbConnectionFactory);
                await r.DoMultipleWorkInTransaction(insert, insert2);
                //scope.Complete();
            }
            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmd2 = cn.CreateCommand();
            cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmd2.ExecuteScalarAsync();
            Assert.True(value ==null);
            cmd2.CommandText = $"select id from table_1 where id = '{insert2}'";
            value = await cmd2.ExecuteScalarAsync();
            Assert.True(value == null);

        }


        [Fact]
        public async Task TestCnStringMismatchRaiseException()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var cnString = _databaseFixture.ConnectionString;
                //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
                var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
                await using (var scope = AmbientTransactionScope.Create(cnString + "diff"))
                {
                    var r = new Repository1(new DbConnectionFactory(cnString));
                    await r.DoSingleWork(insert);
                    scope.Complete();
                }
            });
            Assert.Contains("The connection string of DbConnectionFactory does not match the one of the existing ambient scope.", ex.Message, StringComparison.OrdinalIgnoreCase);
        }


        [Fact]
        public async Task TestNoAmbientConnectionScopeDoMultipleWorkInTransactionCommit()
        {
            var cnString = _databaseFixture.ConnectionString;
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await Task.Delay(100); // Ensure different timestamps for inserts  
            var insert2 = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
           
            var r = new Repository1(new DbConnectionFactory(cnString));
            await r.DoMultipleWorkInTransaction(insert, insert2);

            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmd2 = cn.CreateCommand();
            cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == insert);

            cmd2.CommandText = $"select id from table_1 where id = '{insert2}'";
            value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == insert2);

        }


        [Fact]
        public async Task TestAmbientConnectionScopeDoSingleWorkInTransactionCommit()
        {
            var cnString = _databaseFixture.ConnectionString;
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await using (var scope = AmbientTransactionScope.Create(cnString))
            {
                var r = new Repository1(new DbConnectionFactory(cnString));
                await r.DoSingleWork(insert);
                scope.Complete();
            }
            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmd2 = cn.CreateCommand();
            cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == insert);

        }

        [Fact]
        public async Task TestAmbientConnectionScopeTwoLevelScopeDoSingleWorkInTransactionCommit()
        {
            var cnString = _databaseFixture.ConnectionString;
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await using (var scope = AmbientTransactionScope.Create(cnString))
            {
                await using (var scope1 = AmbientTransactionScope.Create(cnString))
                {
                    var r = new Repository1(new DbConnectionFactory(cnString));
                    await r.DoSingleWork(insert);
                    scope1.Complete();
                }
                scope.Complete();
            }
            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmd2 = cn.CreateCommand();
            cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == insert);

        }

        [Fact]
        public async Task TestAsyncNoAmbientConnectionScopeDoSingleWork()
        {
            var cnString = _databaseFixture.ConnectionString;
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
          
                    var r = new Repository1(new DbConnectionFactory(cnString));
                    await r.DoSingleWork(insert);
                   
          
            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmd2 = cn.CreateCommand();
            cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == insert);

        }

        [Fact]
        public async Task TestAmbientConnectionScopeDoSingleWorkRollBack()
        {
            var cnString = _databaseFixture.ConnectionString;
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await using (var scope = AmbientTransactionScope.Create(cnString))
            {
                var r = new Repository1(new DbConnectionFactory(cnString));
                await r.DoSingleWork(insert);
                //scope.Complete()
            }
            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmd2 = cn.CreateCommand();
            cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmd2.ExecuteScalarAsync();
            Assert.True(value ==null);


        }

       
   

    }
}
