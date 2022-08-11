using Meep.Tech.XBam.IO.Sql.Metadata;
using static Meep.Tech.XBam.IO.Sql.Query.Builder;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public interface ICanOrderBy  : IBuilder { }
    }
  }

  public static class BuilderICanOrderByExtensions {
    public static SelectClauseOpperation OrderBy(this ICanOrderBy builder, string columnName, bool inAscendingOrder = true)
      => builder.SqlContext.BuildSelectClauseQuery(
        (Query.Opperation.Clause)builder.SqlContext.OperationTypes[typeof(Query.Opperation.Clause.ORDER_BY)],
        builder,
        null,
        columnName,
        inAscendingOrder
      );

    public static SelectClauseOpperation OrderBy(this ICanOrderBy builder, Column column, bool inAscendingOrder = true)
      => OrderBy(builder, column.Name, inAscendingOrder);

  }
}