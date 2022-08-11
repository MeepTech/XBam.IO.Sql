using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Opperation {
      public abstract partial class Clause {
        public class WHERE : Clause {

          /// <summary>
          /// The base parameters for the select query.
          /// </summary>
          public static readonly Parameter[] BasicParameters = new[] {
            OnClauseParameter
          };

          public static Parameter OnClauseParameter {
            get;
          } = new Parameter("OnClause", typeof(Builder.Token), true);

          public virtual string WhereCommandText { get; } = "WHERE";
          public virtual string AndCommandText { get; } = "AND";

          WHERE()
            : base(
              new Identity(nameof(WHERE)),
              BasicParameters.ToList()
            ) { }

          protected WHERE(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
            : base(
              id ?? new Identity(nameof(WHERE), universe: universe),
              BasicParameters.Concat((parameters ?? Enumerable.Empty<Parameter>()))
                .ToList(),
              collection,
              universe
            ) { }

          protected internal override (string text, IEnumerable<object> parameters) Build(Builder.Token token, SqlContext sqlContext, ref int queryParamIndex) {
            string text;
            var clauseToken = token.Parameters[0] as Builder.Token;
            if (clauseToken.Archetype is not Query.Clause) {
              throw new System.ArgumentException($"Clause Token Parameter Provided For WHERE is not a Query.Clause Token");
            }

            var clause 
              = clauseToken.Build(sqlContext, ref queryParamIndex);

            text = token.Previous.Archetype is not WHERE
              ? WhereCommandText + " " + clause.text + "\n"
              : "\t" + AndCommandText + " " + clause.text + "\n";

            return (text, clause.parameters);
          }
        }
      }
    }
  }
}