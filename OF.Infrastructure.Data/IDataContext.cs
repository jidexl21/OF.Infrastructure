﻿using System;
using System.Data;

namespace OF.Infrastructure.Data
{
    public interface IDataContext
    {
        void Dispose();
        IDbCommand CreateCommand();
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        bool SaveChanges();
    }
}
