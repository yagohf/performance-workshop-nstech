using System.Data;

namespace PerformanceApi.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}