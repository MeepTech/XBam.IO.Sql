using System.Collections.Generic;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {
    public abstract partial class Builder {

      [Branch]
      public abstract class Type : Archetype<Builder, Builder.Type> {

        protected Type(Archetype.Identity id, Collection collection = null, Universe universe = null)
          : base(id, collection, universe) { }

        internal protected Query Build(Builder builder)
          => new(builder.SqlContext, _orderTokens(builder.SqlContext, builder.Tokens));

        IReadOnlyList<Token> _orderTokens(SqlContext sqlContext, IReadOnlyList<Token> tokens) {
          List<Token> sortedTokens = new();
          List<Token> clauseTokens = new();

          foreach(Token token in tokens) {
            if (token.Archetype is Query.Opperation.Clause) {
              clauseTokens.Add(token);
            } else {
              sortedTokens.Add(token);
            }
          }

          sortedTokens.AddRange(
            sqlContext.SortClauseTokens(clauseTokens));

          return sortedTokens;
        }
      }
    }
  }
}