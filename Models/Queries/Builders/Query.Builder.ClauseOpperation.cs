namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public partial class ClauseOpperation : Builder, ICanWhere {
        protected ClauseOpperation(IBuilder<Builder> builder) 
          : base(builder) { }

        protected ClauseOpperation BuildClauseQuery(Query.Opperation.Clause opperation, Metadata.Model? model = null, params object[] parameters)
          => SqlContext.BuildClauseQuery(opperation, this, model, parameters);
      }
    }
  }
}