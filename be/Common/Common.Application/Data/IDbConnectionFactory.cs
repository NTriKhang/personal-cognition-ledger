using System.Data.Common;

namespace Common.Application.Data;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
