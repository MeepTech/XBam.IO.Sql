using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {
  public partial class Query {
    public abstract partial class Opperation {
      public abstract partial class Secondary {
        public class JOIN : Secondary {

          /// <summary>
          /// The base parameters for the select query.
          /// </summary>
          public static readonly Parameter[] BasicParameters = new[] {
            TableNameParameter,
            OnClauseParameter
          };

          public static Parameter OnClauseParameter {
            get;
          } = new Parameter("OnClauses", typeof(IEnumerable<Builder.Token>), true);

          public virtual string JoinCommandText { get; } = "JOIN";

          JOIN()
            : base(
              new Identity(nameof(JOIN)),
              BasicParameters.ToList()
            ) { }

          protected JOIN(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
            : base(
              id ?? new Identity(nameof(JOIN), universe: universe),
              BasicParameters.Concat((parameters ?? Enumerable.Empty<Parameter>()))
                .ToList(),
              collection,
              universe
            ) { }

          protected internal override (string text, IEnumerable<object> parameters) Build(Builder.Token token, SqlContext sqlContext, ref int queryParamIndex) {
            string text = "";
            List<object> parameters = new();

            var joinTableName = token.Parameters[0] as string;
            var joinClauses = token.Parameters[1] as IEnumerable<Builder.Token>;

            text += JoinCommandText + " " + joinTableName;

            int index = 0;
            foreach (var clause in joinClauses) {
              if (index++ == 0) {
                text += "\n\t ON ";
              } else {
                text += "\n\t\t AND ";
              }
              text += clause.Build(sqlContext, ref queryParamIndex);
            }

            return (text + "\n", parameters);
          }
        }
      }
    }
  }
}