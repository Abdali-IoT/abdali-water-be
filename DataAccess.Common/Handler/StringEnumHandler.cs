using System.Data;
using Dapper;

namespace DataAccess.Common.Handler;

public class StringEnumHandler<T> : SqlMapper.TypeHandler<T> where T : struct, System.Enum
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value.ToString();
    }

    public override T Parse(object value)
    {
        return System.Enum.Parse<T>(value.ToString()!, ignoreCase: true);
    }
}