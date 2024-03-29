﻿
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;


namespace OF.Infrastructure.Data
{
    
    public enum Driver
    {
        MSSQL, MySql, Oracle
    }
    public class AdoNetContext : IDataContext, IDisposable
    {
        private readonly Driver dialect;
        private IDbConnection connection;
        private bool ownsConnection;
        private IDbTransaction transaction;

        public AdoNetContext(string connectionString, bool ownsConnection)
        {
            this.connection = new SqlConnection(connectionString);
            dialect = Driver.MSSQL;
            this.connection.Open();
            this.ownsConnection = ownsConnection;
            this.transaction = this.connection.BeginTransaction();
        }

        public AdoNetContext(string connectionString, bool ownsConnection, Driver driver)
        {
            dialect = driver;
            this.connection = GetConnection(connectionString, driver);
            this.connection.Open();
            this.ownsConnection = ownsConnection;
            this.transaction = this.connection.BeginTransaction();
           
        }


        private IDbConnection GetConnection(string connectionString, Driver driver)
        {
            switch (driver)
            {
                case Driver.MySql:
                    return new MySqlConnection(connectionString);
                    break;
                default:
                    return new SqlConnection(connectionString);
                    break;
            }

        }
        public IDbCommand CreateCommand()
        {
            var command = this.connection.CreateCommand();
            command.Transaction = this.transaction;
            return command;
        }

        public IDbConnection Connection { get { return this.connection; } }
        public IDbTransaction Transaction { get { return this.transaction; } }

        public Dialect SqlDialect => (Dialect) Enum.Parse(typeof(Dialect), dialect.ToString(), true);

        public bool SaveChanges()
        {
            if (this.transaction == null)
            {
                throw new InvalidOperationException("Transaction has already been already been committed. Check your transaction handling.");
            }
            this.transaction.Commit();
            this.transaction = null;
            return true;
        }

        public void Dispose()
        {
            if (this.transaction != null && this.transaction.Connection != null)
            {
                if (this.Connection.State == ConnectionState.Closed) return;
                this.transaction.Rollback();
                this.transaction = null;
            }

            if (this.connection != null && this.ownsConnection)
            {
                this.connection.Close();
                this.connection = null;
            }
        }
    }

}