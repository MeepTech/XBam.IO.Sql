using Meep.Tech.XBam.IO.Sql.Metadata;
using System.Linq;
using static Meep.Tech.XBam.IO.Sql.Query.Builder;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public interface ICanJoin : IBuilder { }
    }
  }
  public static class BuilderICanJoinExtensions {

    #region On Columns Are Equal

    public static JoinOpperation Join<TModel>(this ICanJoin builder, string initialTableColumnName, string newTableColumnName) {
      var model = builder.SqlContext.Models[typeof(TModel)];
      return Join(builder, model.Table.Name, initialTableColumnName, newTableColumnName, model);
    }

    public static JoinOpperation Join<TModel>(this ICanJoin builder, Column initialTableColumn, Column newTableColumn) {
      var model = builder.SqlContext.Models[typeof(TModel)];
      return Join(builder, model.Table.Name, initialTableColumn.Name, newTableColumn.Name, model);
    }

    public static JoinOpperation Join(this ICanJoin builder, System.Type modelType, Column initialTableColumn, Column newTableColumn)
      => Join(builder, builder.SqlContext.Models[modelType], initialTableColumn.Name, newTableColumn.Name);

    public static JoinOpperation Join(this ICanJoin builder, System.Type modelType, string initialTableColumnName, string newTableColumnName)
      => Join(builder, builder.SqlContext.Models[modelType], initialTableColumnName, newTableColumnName);

    public static JoinOpperation Join(this ICanJoin builder, Metadata.Model newTableModel, Column initialTableColumn, Column newTableColumn)
      => Join(builder, newTableModel.Table.Name, initialTableColumn.Name, newTableColumn.Name, newTableModel);

    public static JoinOpperation Join(this ICanJoin builder, Metadata.Model newTableModel, string initialTableColumnName, string newTableColumnName)
      => Join(builder, newTableModel.Table.Name, initialTableColumnName, newTableColumnName, newTableModel);

    public static JoinOpperation Join(this ICanJoin builder, Table newTable, Table initialTable, Column initialTableColumn, Column newTableColumn, Metadata.Model? newTableModel = null)
      => Join(builder, newTable.Name, initialTable.Name, initialTableColumn.Name, newTableColumn.Name, newTableModel);

    public static JoinOpperation Join(this ICanJoin builder, Table newTable, Column initialTableColumn, Column newTableColumn, Metadata.Model? newTableModel = null)
      => Join(builder, newTable.Name, initialTableColumn.Name, newTableColumn.Name, newTableModel);

    public static JoinOpperation Join(this ICanJoin builder, string newTableName, string initialTableColumnName, string newTableColumnName, Metadata.Model? newTableModel = null)
      => Join(builder, newTableName, builder.Tokens.First().Parameters[0] as string, initialTableColumnName, newTableColumnName, newTableModel);

    public static JoinOpperation Join(this ICanJoin builder, string newTableName, string initialTableName, string initialTableColumn, string newTableColumnName, Metadata.Model? newTableModel = null)
      => builder.SqlContext.BuildJoinQuery(
        (Query.Opperation.Secondary)builder.SqlContext.OperationTypes[typeof(Query.Opperation.Secondary.JOIN)],
        builder,
        newTableModel,
        newTableName,
        new Token[] {
            Query.Builder.Token.Types.Get<Query.Clause.ColumnsAreEqual>().Make($"{initialTableName}.{initialTableColumn}", $"{newTableModel}.{newTableColumnName}")
        }
      );

    #endregion

    #region Where Column Equals

    public static JoinOpperation JoinWhere<TModel>(this ICanJoin builder, string newTableColumnName, object equalsValue) {
      var model = builder.SqlContext.Models[typeof(TModel)];
      return JoinWhere(builder, model, newTableColumnName, equalsValue);
    }

    public static JoinOpperation JoinWhere<TModel>(this ICanJoin builder, Column newTableColumn, object equalsValue) {
      var model = builder.SqlContext.Models[typeof(TModel)];
      return JoinWhere(builder, model, newTableColumn.Name, equalsValue);
    }

    public static JoinOpperation JoinWhere(this ICanJoin builder, System.Type modelType, Column newTableColumn, object equalsValue)
      => JoinWhere(builder, builder.SqlContext.Models[modelType], newTableColumn.Name, equalsValue);

    public static JoinOpperation JoinWhere(this ICanJoin builder, System.Type modelType, string newTableColumnName, object equalsValue)
      => JoinWhere(builder, builder.SqlContext.Models[modelType], newTableColumnName, equalsValue);

    public static JoinOpperation JoinWhere(this ICanJoin builder, Metadata.Model newTableModel, Column newTableColumn, object equalsValue)
      => JoinWhere(builder, newTableModel, newTableColumn.Name, equalsValue);

    public static JoinOpperation JoinWhere(this ICanJoin builder, Metadata.Model newTableModel, string newTableColumnName, object equalsValue)
      => JoinWhere(builder, newTableModel.Table.Name, newTableColumnName, equalsValue, newTableModel);

    public static JoinOpperation JoinWhere(this ICanJoin builder, Table newTable, Column newTableColumn, object equalsValue, Metadata.Model? newTableModel = null)
      => JoinWhere(builder, newTable.Name, newTableColumn.Name, equalsValue, newTableModel);

    public static JoinOpperation JoinWhere(this ICanJoin builder, string newTableName, string newTableColumnName, object equalsValue, Metadata.Model? newTableModel = null)
      => builder.SqlContext.BuildJoinQuery(
        (Query.Opperation.Secondary)builder.SqlContext.OperationTypes[typeof(Query.Opperation.Secondary.JOIN)],
        builder,
        newTableModel,
        newTableName,
        new Token[] {
          Query.Builder.Token.Types.Get<Query.Clause.ColumnEquals>().Make($"{newTableModel}.{newTableColumnName}", equalsValue)
        });

    #endregion

    public static JoinOpperation Join<TClause>(this ICanJoin builder, Table newTable, params object[] clauseParameters)
      where TClause : Query.Clause
        => Join<TClause>(builder, newTable.Name, clauseParameters);

    public static JoinOpperation Join<TClause>(this ICanJoin builder, string newTableName, params object[] clauseParameters)
      where TClause : Query.Clause
        => Join<TClause>(builder, newTableName, newTableModel: null, clauseParameters: clauseParameters);

    public static JoinOpperation Join<TClause>(this ICanJoin builder, string newTableName, Metadata.Model? newTableModel = null, params object[] clauseParameters)
      where TClause : Query.Clause
        => builder.SqlContext.BuildJoinQuery(
            (Query.Opperation.Secondary)builder.SqlContext.OperationTypes[typeof(Query.Opperation.Secondary.JOIN)],
            builder,
            newTableModel,
            newTableName,
            new Token[] {
              Query.Builder.Token.Types.Get<TClause>().Make(clauseParameters)
            }
          );
  }
}