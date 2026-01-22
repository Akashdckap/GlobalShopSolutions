using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;

namespace GlobalSolutions.Services
{
    public interface IDbConnectionService
    {
        OdbcConnection GetodbcDbConnection();
    }

    public class DbConnectionService : IDbConnectionService
    {
        private readonly string _connStr;

        public DbConnectionService(string connStr)
        {
            _connStr = connStr;
        }

        public virtual OdbcConnection GetodbcDbConnection()
        {
            return new OdbcConnection(_connStr);
        }
    }
}
