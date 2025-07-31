using AmbientTransaction;
using Microsoft.Data.SqlClient;
using Xunit.Sdk;

namespace AmbientTransactionTests
{
    public class UnitTest1Take2
    {
        
        [Fact]
        public async Task TestAsyncAmbientConnectionScopeTake2SingleCommandSingleScope()
        {
            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert =  DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff") ;
            await using (var scope = AmbientConnectionScopeTake2.Create(cnString))
            {
                Assert.NotNull(scope.Connection);
                Assert.NotNull(scope.Transaction);
                Assert.Equal(System.Data.ConnectionState.Open, scope.Connection.State);
                //var cmd = scope.Connection.CreateCommand();
                //cmd.Transaction = scope.Transaction;
                //cmd.CommandText = $"insert into table_1 (id) values ('{insert}')";
                var cmd = scope.CreateCommand($"insert into table_1 (id) values ('{insert}')");   
                try
                {
                    await cmd.ExecuteNonQueryAsync();

                }
                catch (Exception ex)
                {
                    throw;
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
        public async Task TestAsyncAmbientConnectionScopeTake2SingleCommandSingleScopeNoComplete()
        {
            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await using (var scope = AmbientConnectionScopeTake2.Create(cnString))
            {
                Assert.NotNull(scope.Connection);
                Assert.NotNull(scope.Transaction);
                Assert.Equal(System.Data.ConnectionState.Open, scope.Connection.State);
                //var cmd = scope.Connection.CreateCommand();
                //cmd.Transaction = scope.Transaction;
                //cmd.CommandText = $"insert into table_1 (id) values ('{insert}')";
                var cmd = scope.CreateCommand($"insert into table_1 (id) values ('{insert}')"); 
                try
                {
                    await cmd.ExecuteNonQueryAsync();

                }
                catch (Exception ex)
                {
                    throw;
                }
               
            }
            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmd2 = cn.CreateCommand();
            cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == null);

        }

        [Fact]
        public async Task TestAsyncAmbientConnectionScopeTake2TwoCommandsSingleScope()
        {
            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            var insert2 = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await using (var scope = AmbientConnectionScopeTake2.Create(cnString))
            {
                Assert.NotNull(scope.Connection);
                Assert.NotNull(scope.Transaction);
                Assert.Equal(System.Data.ConnectionState.Open, scope.Connection.State);
                //var cmd = scope.Connection.CreateCommand();
                //cmd.Transaction = scope.Transaction;
                //cmd.CommandText = $"insert into table_1 (id) values ('{insert}')";
                var cmd = scope.CreateCommand($"insert into table_1 (id) values ('{insert}')"); 
                try
                {
                    await cmd.ExecuteNonQueryAsync();

                }
                catch (Exception ex)
                {
                    throw;
                }
                cmd.CommandText = $"insert into table_2 (id) values ('{insert2}')";
                try
                {
                    await cmd.ExecuteNonQueryAsync();

                }
                catch (Exception ex)
                {
                    throw;
                }
                scope.Complete();
            }
            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmd2 = cn.CreateCommand();
            cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == insert);
            cmd2.CommandText = $"select id from table_2 where id = '{insert2}'";
            value = await cmd2.ExecuteScalarAsync();
            Assert.True(value as string == insert2);

        }

        [Fact]
        public async Task TestAsyncAmbientConnectionScopeTake2TwoCommandsTwoScope()
        {
            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            var insert2 = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            await using (var scope1 = AmbientConnectionScopeTake2.Create(cnString))
            {
                Assert.NotNull(scope1.Connection);
                Assert.NotNull(scope1.Transaction);
                Assert.Equal(System.Data.ConnectionState.Open, scope1.Connection.State);
                //var cmd = scope1.Connection.CreateCommand();
                //cmd.Transaction = scope1.Transaction;
                //cmd.CommandText = $"insert into table_1 (id) values ('{insert}')";
                var cmd = scope1.CreateCommand($"insert into table_1 (id) values ('{insert}')");
                await cmd.ExecuteNonQueryAsync();

                // NESTED
                await using (var scope2 = AmbientConnectionScopeTake2.Create(cnString))
                {
                    //var cmd2 = scope2.Connection.CreateCommand();
                    //cmd2.Transaction = scope1.Transaction;
                    var cmd2 = scope2.CreateCommand($"insert into table_2 (id) values ('{insert2}')");
                    await cmd2.ExecuteNonQueryAsync();
                    scope2.Complete();
                }

                scope1.Complete();
            }
            using var cn = new SqlConnection(cnString);
            await cn.OpenAsync();
            var cmdcheck = cn.CreateCommand();
            cmdcheck.CommandText = $"select id from table_1 where id = '{insert}'";
            var value = await cmdcheck.ExecuteScalarAsync();
            Assert.True(value as string == insert);
            cmdcheck.CommandText = $"select id from table_2 where id = '{insert2}'";
            value = await cmdcheck.ExecuteScalarAsync();
            Assert.True(value as string == insert2);

        }

        [Fact]
        public async Task TestAsyncAmbientConnectionScopeTake2TwoCommandsTwoScopeInner2ForgetCallToComplete()
        {
            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            var insert2 = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            try
            {
                await using (var scope = AmbientConnectionScopeTake2.Create(cnString))
                {
                    Assert.NotNull(scope.Connection);
                    Assert.NotNull(scope.Transaction);
                    Assert.Equal(System.Data.ConnectionState.Open, scope.Connection.State);
                    //var cmd = scope.Connection.CreateCommand();
                    //cmd.Transaction = scope.Transaction;
                    //cmd.CommandText = $"insert into table_1 (id) values ('{insert}')";
                    var cmd = scope.CreateCommand($"insert into table_1 (id) values ('{insert}')"); 
                    await cmd.ExecuteNonQueryAsync();
                    // NESTED
                    await using (var scope2 = AmbientConnectionScopeTake2.Create(cnString))
                    {
                        //var cmd2 = scope2.Connection.CreateCommand();
                        //cmd2.Transaction = scope.Transaction;
                        //cmd2.CommandText = $"insert into table_2 (id) values ('{insert2}')";
                        var cmd2 = scope2.CreateCommand($"insert into table_2 (id) values ('{insert2}')");
                        await cmd2.ExecuteNonQueryAsync();
                        // NESTED 2 // do not call complete
                    }
                    scope.Complete();
                }
                throw new Exception("Expected exception not thrown");   
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == "A Child scope did not called complete, but parent scope voted to commit.");
                using var cn = new SqlConnection(cnString);
                await cn.OpenAsync();
                var cmdCheck = cn.CreateCommand();
                cmdCheck.CommandText = $"select id from table_1 where id = '{insert}'";
                var value = await cmdCheck.ExecuteScalarAsync();
                Assert.True(value as string == null);
                cmdCheck = cn.CreateCommand();
                cmdCheck.CommandText = $"select id from table_2 where id = '{insert2}'";
                value = await cmdCheck.ExecuteScalarAsync();
                Assert.True(value as string == null);
            }

        }

        [Fact]
        public async Task TestAsyncAmbientConnectionScopeTake2TwoCommands3ScopeInner3ForgetCallToComplete()
        {
            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            var insert2 = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            var insert3 = DateTime.Now.AddSeconds(20).ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            try
            {
                await using (var scope = AmbientConnectionScopeTake2.Create(cnString))
                {
                    Assert.NotNull(scope.Connection);
                    Assert.NotNull(scope.Transaction);
                    Assert.Equal(System.Data.ConnectionState.Open, scope.Connection.State);
                    //var cmd = scope.Connection.CreateCommand();
                    //cmd.Transaction = scope.Transaction;
                    //cmd.CommandText = $"insert into table_1 (id) values ('{insert}')";
                    var cmd = scope.CreateCommand($"insert into table_1 (id) values ('{insert}')"); 
                    await cmd.ExecuteNonQueryAsync();

                   

                    // NESTED
                    await using (var scope2 = AmbientConnectionScopeTake2.Create(cnString))
                    {
                        //var cmd2 = scope2.Connection.CreateCommand();
                        //cmd2.Transaction = scope.Transaction;
                        //cmd2.CommandText = $"insert into table_2 (id) values ('{insert2}')";
                        var cmd2 = scope2.CreateCommand($"insert into table_2 (id) values ('{insert2}')");  
                        try
                        {
                            await cmd2.ExecuteNonQueryAsync();
                            // NESTED 3 // do not call complete
                            await using (var scope3 = AmbientConnectionScopeTake2.Create(cnString))
                            {
                                //var cmd3 = scope3.Connection.CreateCommand();
                                //cmd3.Transaction = scope.Transaction;
                                //cmd3.CommandText = $"insert into table_2 (id) values ('{insert3}')";
                                var cmd3 = scope3.CreateCommand($"insert into table_2 (id) values ('{insert3}')");  
                                await cmd3.ExecuteNonQueryAsync();
                                //scope.Complete(); forget to vote 
                            }
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        scope2.Complete();
                    }

                    scope.Complete();
                }
                throw new Exception("Expected exception not thrown");
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == "A Child scope did not called complete, but parent scope voted to commit.");
                using var cn = new SqlConnection(cnString);
                await cn.OpenAsync();
                var cmd2 = cn.CreateCommand();
                cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
                var value = await cmd2.ExecuteScalarAsync();
                Assert.True(value as string == null);
                cmd2.CommandText = $"select id from table_2 where id = '{insert2}'";
                value = await cmd2.ExecuteScalarAsync();
                Assert.True(value as string == null);
                cmd2.CommandText = $"select id from table_2 where id = '{insert3}'";
                value = await cmd2.ExecuteScalarAsync();
                Assert.True(value as string == null);

            }

        }

        [Fact]
        public async Task TestAsyncAmbientConnectionScopeTake2TwoCommands3ScopeInner3Fails()
        {
            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            var insert2 = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            try
            {
                await using (var scope = AmbientConnectionScopeTake2.Create(cnString))
                {
                    Assert.NotNull(scope.Connection);
                    Assert.NotNull(scope.Transaction);
                    Assert.Equal(System.Data.ConnectionState.Open, scope.Connection.State);
                    //var cmd = scope.Connection.CreateCommand();
                    //cmd.Transaction = scope.Transaction;
                    //cmd.CommandText = $"insert into table_1 (id) values ('{insert}')";
                    var cmd = scope.CreateCommand($"insert into table_1 (id) values ('{insert}')"); 
                    await cmd.ExecuteNonQueryAsync();



                    // NESTED
                    await using (var scope2 = AmbientConnectionScopeTake2.Create(cnString))
                    {
                        //var cmd2 = scope2.Connection.CreateCommand();
                        //cmd2.Transaction = scope.Transaction;
                        //cmd2.CommandText = $"insert into table_2 (id) values ('{insert2}')";
                        var cmd2= scope2.CreateCommand($"insert into table_2 (id) values ('{insert2}')");   
                        try
                        {
                            await cmd2.ExecuteNonQueryAsync();
                            // NESTED 3 // do not call complete
                            await using (var scope3 = AmbientConnectionScopeTake2.Create(cnString))
                            {
                                //var cmd3 = scope3.Connection.CreateCommand();
                                //cmd3.Transaction = scope.Transaction;
                                //// generate violation of PK 
                                //cmd3.CommandText = $"insert into table_2 (id) values ('{insert2}')";
                                var cmd3 = scope3.CreateCommand($"insert into table_2 (id) values ('{insert2}')");  
                                await cmd3.ExecuteNonQueryAsync();


                            }
                            scope.Complete();

                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        scope2.Complete();
                    }

                    scope.Complete();
                }
                throw new Exception("Expected exception not thrown");
            }
            catch (SqlException ex)
            {
                Assert.Contains("Violation of PRIMARY KEY constraint 'PK_Table_2'.", ex.Message);
                using var cn = new SqlConnection(cnString);
                await cn.OpenAsync();
                var cmd2 = cn.CreateCommand();
                cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
                var value = await cmd2.ExecuteScalarAsync();
                Assert.True(value as string == null);
                cmd2.CommandText = $"select id from table_2 where id = '{insert2}'";
                value = await cmd2.ExecuteScalarAsync();
                Assert.True(value as string == null);
            }

        }


        [Fact]
        public async Task TestAsyncAmbientConnectionScopeTake2TwoCommands3ScopeInner2Fails()
        {
            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            var insert2 = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            try
            {
                await using (var scope = AmbientConnectionScopeTake2.Create(cnString))
                {
                    Assert.NotNull(scope.Connection);
                    Assert.NotNull(scope.Transaction);
                    Assert.Equal(System.Data.ConnectionState.Open, scope.Connection.State);
                    //var cmd = scope.Connection.CreateCommand();
                    //cmd.Transaction = scope.Transaction;
                    //cmd.CommandText = $"insert into table_1 (id) values ('{insert}')";
                    var cmd = scope.CreateCommand($"insert into table_1 (id) values ('{insert}')"); 
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    // NESTED
                    await using (var scope2 = AmbientConnectionScopeTake2.Create(cnString))
                    {   // generate violation of PK 
                        //var cmd2 = scope2.Connection.CreateCommand();
                        //cmd2.Transaction = scope.Transaction;
                        //cmd2.CommandText = $"insert into table_2 (id) values ('{insert}')";
                        var cmd2 = scope2.CreateCommand($"insert into table_2 (id) values ('{insert}')");   
                        await cmd2.ExecuteNonQueryAsync();
                            // generate violation of PK 
                            await cmd2.ExecuteNonQueryAsync();
                            // NESTED 3 // do not call complete
                            await using (var scope3 = AmbientConnectionScopeTake2.Create(cnString))
                            {
                                //var cmd3 = scope3.Connection.CreateCommand();
                                //cmd3.Transaction = scope.Transaction;
                                //// generate violation of PK 
                                //cmd3.CommandText = $"insert into table_2 (id) values ('{insert2}')";
                            var cmd3 = scope3.CreateCommand($"insert into table_2 (id) values ('{insert2}')");
                            try
                                {
                                    await cmd3.ExecuteNonQueryAsync();

                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }
                                scope3.Complete();
                        }
                        scope2.Complete();
                    }

                    scope.Complete();
                }
                throw new Exception("Expected exception not thrown");
            }
            catch (SqlException ex)
            {
                Assert.Contains("Violation of PRIMARY KEY constraint 'PK_Table_2'.", ex.Message);
                using var cn = new SqlConnection(cnString);
                await cn.OpenAsync();
                var cmd2 = cn.CreateCommand();
                cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
                var value = await cmd2.ExecuteScalarAsync();
                Assert.True(value as string == null);
                cmd2.CommandText = $"select id from table_2 where id = '{insert2}'";
                value = await cmd2.ExecuteScalarAsync();
                Assert.True(value as string == null);
            }

        }

        [Fact]
        public async Task TestAsyncAmbientConnectionScopeTake2TwoCommands3ScopeRootFails()
        {
            var cnString = "Server=THINKPAD-32;Database=transactions;User Id=sa;Password=SQL2025_;TrustServerCertificate=true";
            //await using (var scope = await XAmbientConnectionScope.CreateAsync(cnString))
            var insert = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            var insert2 = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            var insert4 = DateTime.Now.AddSeconds(20).ToString("yyyy-MM-dd-hh-mm-ss--fffff");
            try
            {
                await using (var scope1 = AmbientConnectionScopeTake2.Create(cnString))
                {
                    Assert.NotNull(scope1.Connection);
                    Assert.NotNull(scope1.Transaction);
                    Assert.Equal(System.Data.ConnectionState.Open, scope1.Connection.State);
                    //var cmd = scope1.Connection.CreateCommand();
                    //cmd.Transaction = scope1.Transaction;
                    //cmd.CommandText = $"insert into table_1 (id) values ('{insert}')";
                    var cmd = scope1.CreateCommand($"insert into table_1 (id) values ('{insert}')");    
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    // NESTED
                    await using (var scope2 = AmbientConnectionScopeTake2.Create(cnString))
                    {   // generate violation of PK 
                        //var cmd2 = scope1.Connection.CreateCommand();
                        //cmd2.CommandText = $"insert into table_2 (id) values ('{insert}')";
                        //cmd2.Transaction = scope1.Transaction;
                        var cmd2 = scope2.CreateCommand($"insert into table_2 (id) values ('{insert}')");   
                        await cmd2.ExecuteNonQueryAsync();
                        // NESTED 3 // do not call complete
                        await using (var scope3 = AmbientConnectionScopeTake2.Create(cnString))
                        {
                            //var cmd3 = scope1.Connection.CreateCommand();
                            //cmd3.Transaction = scope1.Transaction;
                            //// generate violation of PK 
                            //cmd3.CommandText = $"insert into table_2 (id) values ('{insert2}')";
                            var cmd3 = scope3.CreateCommand($"insert into table_2 (id) values ('{insert2}')");  
                            await cmd3.ExecuteNonQueryAsync();
                            scope3.Complete();
                        }
                        scope2.Complete();
                    }
                    // violation PK 
                    await cmd.ExecuteNonQueryAsync();
                    scope1.Complete();
                }
                throw new Exception("Expected exception not thrown");
            }
            catch (SqlException ex)
            {
                Assert.Contains("Violation of PRIMARY KEY constraint 'PK_Table_1'.", ex.Message);
                using var cn = new SqlConnection(cnString);
                await cn.OpenAsync();
                var cmd2 = cn.CreateCommand();
                cmd2.CommandText = $"select id from table_1 where id = '{insert}'";
                var value = await cmd2.ExecuteScalarAsync();
                Assert.True(value as string == null);
                cmd2.CommandText = $"select id from table_2 where id = '{insert2}'";
                value = await cmd2.ExecuteScalarAsync();
                Assert.True(value as string == null);
            }

        }

    }
}
