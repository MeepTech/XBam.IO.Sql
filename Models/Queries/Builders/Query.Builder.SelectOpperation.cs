namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {

      public partial class SelectOpperation : Builder, ICanJoin, ICanWhereSelect, ICanOrderBy {
        protected SelectOpperation(IBuilder<Builder> builder)
          : base(builder) { }

        protected JoinOpperation BuildJoinQuery(Query.Opperation.Secondary opperation, Metadata.Model? newTableModel = null, params object[] parameters)
          => SqlContext.BuildJoinQuery(opperation, this, newTableModel, parameters);

        protected SelectClauseOpperation BuildClauseQuery(Query.Opperation.Clause opperation, Metadata.Model? model = null, params object[] parameters)
          => SqlContext.BuildSelectClauseQuery(opperation, this, model, parameters);
      }
    }
  }
}