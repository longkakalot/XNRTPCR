using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class DapperService : IDapperService
    {
        private readonly Startup.ConnectionStrings _connectionStrings;
        private readonly ILogger<DapperService> _logger;
        public DapperService(IOptions<Startup.ConnectionStrings> connectionStrings, ILogger<DapperService> logger, ILoggerFactory logFactory)
        {
            _logger = logger;
            _connectionStrings = connectionStrings.Value;
            _logger = logFactory.CreateLogger<DapperService>();
        }

        public void Dispose()
        {

        }

        public int Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            throw new NotImplementedException();
        }

        public async Task<T> Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {

            using IDbConnection db = new SqlConnection(_connectionStrings.SqlFpt);
            try
            {
                var resultAwait = await db.QueryAsync<T>(sp, parms, commandType: commandType);
                var result = resultAwait.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return default;
            }

        }

        public async Task<List<T>> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            List<T> result = null;
            using (var conn = new SqlConnection(_connectionStrings.SqlFpt))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    try
                    {
                        var resultAwait = await conn.QueryAsync<T>(sp, parms, commandType: commandType);
                        result = resultAwait.ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation(ex.Message);

                    }
                }
            }
            return result;
        }



        public async Task<int> Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            int result = 0;

            using (var conn = new SqlConnection(_connectionStrings.SqlFpt))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var kq = await conn.ExecuteAsync(sp, parms, commandType: commandType, transaction: tran);
                            //var a = parms.Get<int>("@Id");
                            result = kq;
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            _logger.LogInformation(ex.Message);
                            result = 0;
                        }
                    }
                }
            }

            return result;
        }

        public async Task<int> InsertList<T>(string sp, List<DynamicParameters> parms, CommandType commandType = CommandType.StoredProcedure)
        {
            int result = 0;

            using (var conn = new SqlConnection(_connectionStrings.SqlFpt))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        foreach (var item in parms)
                        {
                            try
                            {
                                await conn.ExecuteAsync(sp, item, commandType: commandType, transaction: tran);
                            }
                            catch (Exception ex)
                            {
                                tran.Rollback();
                                _logger.LogInformation(ex.Message);
                                result = 0;
                            }
                        }
                        result = 1;
                        tran.Commit();
                    }
                }
            }

            return result;
        }

        public async Task<int> Update<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            int result = 0;
            using (var conn = new SqlConnection(_connectionStrings.SqlFpt))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            await conn.ExecuteAsync(sp, parms, commandType: commandType, transaction: tran);
                            result = 1;
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            _logger.LogInformation(ex.Message);
                            result = 0;
                        }
                    }
                }
            }

            return result;
        }
    }
}
