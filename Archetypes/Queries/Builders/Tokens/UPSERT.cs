using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Opperation {

      public abstract partial class Initial {
        public abstract class UPSERT : Initial {

          /// <summary>
          /// The base parameters for the select query.
          /// </summary>
          public static readonly Parameter[] BasicParameters =
            INSERT.BasicParameters;

          protected UPSERT(IList<Parameter> parameters = null, Collection collection = null, Universe universe = null)
            : base(new Identity(nameof(UPSERT), universe: universe),
              BasicParameters.Concat((parameters ?? Enumerable.Empty<Parameter>()))
                .ToList(),
              collection,
              universe
            ) { }

          protected internal override (string text, IEnumerable<object> parameters) Build(Builder.Token token, SqlContext sqlContext, ref int queryParamIndex) {
            var pre = BuildPreInsertText(token, sqlContext);
            var insert = sqlContext.OperationTypes[typeof(INSERT)].Build(token, sqlContext, ref queryParamIndex);
            var post = BuildPostInsertText(token, sqlContext);
            return (string.Join('\n', new[] {pre,insert,post}), pre.parameters.Concat(insert.parameters.Concat(post.parameters)).ToList());
          }

          protected abstract (string text, IEnumerable<object> parameters) BuildPreInsertText(Builder.Token token, SqlContext sqlContext);
          protected abstract (string text, IEnumerable<object> parameters) BuildPostInsertText(Builder.Token token, SqlContext sqlContext);
        }
      }
    }
  }
}