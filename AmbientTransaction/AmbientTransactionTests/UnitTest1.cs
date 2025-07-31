//using AmbientTransaction;
//using Microsoft.Data.SqlClient;
//using Xunit.Sdk;

//namespace AmbientTransactionTests
//{
//    public class UnitTest1
//    {
//        [Fact]
//        public async Task TestAsync()
//        {
//            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
//            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
//            using (var scope = XAmbientConnectionScope.CreateAsync(cnString).Result)
//            {
//                var x = scope.Current2;
//                var y = AmbientConnectionScope.Current;
//                await scope.DisposeAsync();
//                Assert.NotNull(scope.Connection);
//                Assert.NotNull(scope.Transaction);
//                Assert.Equal(System.Data.ConnectionState.Open, scope.Connection.State);
//                var cmd = scope.Connection.CreateCommand();
//                cmd.Transaction = scope.Transaction;
//                cmd.CommandText = $"insert into table_1 (id) values ({DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff")})";
//                try
//                {
//                    await cmd.ExecuteNonQueryAsync();

//                }
//                catch (Exception ex) {
//                    throw;
//                }
//                scope.Complete();
//            }   
//        }

        
      

//        [Fact]
//        public async Task TestAsyncNoDictionary()
//        {
//            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
//            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
//            using (var scope = await AmbientConnectionScopeNoDictionary.CreateAsync(cnString))
//            {
//                var x = scope.Current2;
//                var y = AmbientConnectionScope.Current;
//                await scope.DisposeAsync();
//                Assert.NotNull(scope.Connection);
//                Assert.NotNull(scope.Transaction);
//                Assert.Equal(System.Data.ConnectionState.Open, scope.Connection.State);
//                var cmd = scope.Connection.CreateCommand();
//                cmd.Transaction = scope.Transaction;
//                cmd.CommandText = $"insert into table_1 (id) values ({DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff")})";
//                try
//                {
//                    await cmd.ExecuteNonQueryAsync();

//                }
//                catch (Exception ex)
//                {
//                    throw;
//                }
//                scope.Complete();
//            }
//        }

//        [Fact]
//        public async Task TestSynch()
//        {
//            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
//            await using (var scope = XAmbientConnectionScopeNoAsync.Create(cnString))
//            {
//                scope.Complete();
//            }
//        }

//        [Fact]
//        public async Task TestBasic()
//        {
//            using (var customizedScope = new CustomizedScope())
//            {
//                customizedScope.Activate();
//            }
            
//        }
//    }
//}
