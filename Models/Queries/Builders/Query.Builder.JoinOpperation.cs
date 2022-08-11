using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {

      public partial class JoinOpperation : SelectOpperation {

        /// <summary>
        /// Add an On clause to the join.
        /// </summary>
        public JoinOpperation AndOn<TClause>(params object[] parameters) where TClause : Query.Clause {
          var token = Tokens.Last();

          // update the params with the new clause:
          var modifiedTokenParameters = token.Parameters.ToList();
          modifiedTokenParameters[Tokens.Count - 1] = 
            (token.Parameters[Tokens.Count - 1] as IEnumerable<Token>)
              .Append(Query.Builder.Token.Types.Get<TClause>().Make(parameters));

          // add the updated parameters
          token.Parameters = modifiedTokenParameters;
          return this;
        }

        protected JoinOpperation(IBuilder<Builder> builder) 
          : base(builder) {}
      }
    }
  }
}