using Meep.Tech.XBam.IO.Sql.Metadata;
using static Meep.Tech.XBam.IO.Sql.Query.Builder;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public interface ICanWhere : IBuilder { }
      public interface ICanWhereSelect : IBuilder { }
    }
  }

  public static class BuilderICanWhereSelectExtensions {
    public static SelectClauseOpperation WhereColumnEquals(this ICanWhereSelect builder, Column column, object value)
      => WhereColumnEquals(builder, column.Name, value);

    public static SelectClauseOpperation WhereColumnEquals(this ICanWhereSelect builder, string columnName, object value)
      => builder.SqlContext.BuildSelectClauseQuery(
        (Query.Opperation.Clause)builder.SqlContext.OperationTypes[typeof(Query.Opperation.Clause.WHERE)],
        builder,
        null,
        new[] { Query.Builder.Token.Types.Get<Query.Clause.ColumnEquals>().Make(columnName, value) }
      );

    public static SelectClauseOpperation Where<TClause>(this ICanWhereSelect builder, params object[] parameters) where TClause : Query.Clause
      => builder.SqlContext.BuildSelectClauseQuery(
        (Query.Opperation.Clause)builder.SqlContext.OperationTypes[typeof(Query.Opperation.Clause.WHERE)],
        builder,
        null,
        Query.Builder.Token.Types.Get<TClause>().Make(parameters)
      );
  }

  public static class BuilderICanWhereExtensions {
    public static ClauseOpperation WhereColumnEquals(this ICanWhere builder, Column column, object value)
      => WhereColumnEquals(builder, column.Name, value);

    public static ClauseOpperation WhereColumnEquals(this ICanWhere builder, string columnName, object value)
      => builder.SqlContext.BuildClauseQuery(
        (Query.Opperation.Clause)builder.SqlContext.OperationTypes[typeof(Query.Opperation.Clause.WHERE)],
        builder,
        null,
        new[] { Query.Builder.Token.Types.Get<Query.Clause.ColumnEquals>().Make(columnName, value) }
      );

    public static ClauseOpperation Where<TClause>(this ICanWhere builder, params object[] parameters) where TClause : Query.Clause
      => builder.SqlContext.BuildClauseQuery(
        (Query.Opperation.Clause)builder.SqlContext.OperationTypes[typeof(Query.Opperation.Clause.WHERE)],
        builder,
        null,
        Query.Builder.Token.Types.Get<TClause>().Make(parameters)
      );
  }
}