using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

public abstract partial class Opperation {

      public abstract partial class Initial {
        public class DELETE : Initial {

          /// <summary>
          /// The base parameters for the delete query.
          /// </summary>
          public static readonly Parameter[] BasicParameters = new[] {
            TableNameParameter
          };

          public virtual string DeleteFromCommandText { get; } = "DELETE FROM";

          DELETE()
            : base(
              new Identity(nameof(DELETE)),
              BasicParameters.ToList()
            ) { }

          protected DELETE(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
            : base(
              id ?? new Identity(nameof(DELETE), universe: universe),
              BasicParameters
                .Concat(parameters ?? Enumerable.Empty<Parameter>())
                .ToList(),
              collection,
              universe
            ) { }

          protected internal override (string text, IEnumerable<object> parameters) Build(Builder.Token token, SqlContext sqlContext, ref int queryParamIndex)
            => (
              DeleteFromCommandText + " " + (token.Parameters[0] as string) + "\n",
              Enumerable.Empty<object>()
            );
        }
      }
    }
  }
}