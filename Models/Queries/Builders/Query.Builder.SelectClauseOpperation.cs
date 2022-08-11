
namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public partial class SelectClauseOpperation : Builder, ICanWhere, ICanOrderBy {
        protected SelectClauseOpperation(IBuilder<Builder> builder) 
          : base(builder) { }

        protected SelectClauseOpperation BuildClauseQuery(Query.Opperation.Clause opperation, Metadata.Model? model = null, params object[] parameters)
          => SqlContext.BuildSelectClauseQuery(opperation, this, model, parameters);
      }
    }
  }
}