using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Opperation {

      public abstract partial class Clause {
        public class ORDER_BY : Clause {


          /// <summary>
          /// The base parameters for the select query.
          /// </summary>
          public static readonly Parameter[] BasicParameters = new[] {
            ColumnNameParameter,
            IsAscendingParameter
          };

          public static Parameter ColumnNameParameter {
            get;
          } = new Parameter("ColumnName", typeof(string), true);

          public static Parameter IsAscendingParameter {
            get;
          } = new Parameter("IsAscending", typeof(bool));

          public virtual string OrderByCommandText { get; } = "ORDER BY";
          public virtual string AndCommandText { get; } = ",";
          public virtual string AscendingCommandText { get; } = "ASC";
          public virtual string DecendingCommandText { get; } = "DESC";

          ORDER_BY()
            : base(
              new Identity(nameof(ORDER_BY)),
              BasicParameters.ToList()
            ) { }

          protected ORDER_BY(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
            : base(
              id ?? new Identity(nameof(WHERE), universe: universe),
              BasicParameters.Concat((parameters ?? Enumerable.Empty<Parameter>()))
                .ToList(),
              collection,
              universe
            ) { }

          protected internal override (string text, IEnumerable<object> parameters) Build(Builder.Token token, SqlContext sqlContext, ref int queryParamIndex) {
            string text;
            var columnName = token.Parameters[0] as string;
            var isAscending = token.Parameters.Count > 1 ? (bool)token.Parameters[1] : false;

            text = token.Previous.Archetype is not ORDER_BY
              ? OrderByCommandText + " " + columnName + " " + (isAscending ? AscendingCommandText : DecendingCommandText) + "\n"
              : "\t" + AndCommandText + " " + columnName + " " + (isAscending ? AscendingCommandText : DecendingCommandText) + "\n";

            return (text, Enumerable.Empty<object>());
          }
        }
      }
    }
  }
}