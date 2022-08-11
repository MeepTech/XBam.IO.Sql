using Meep.Tech.XBam.Utility;
using System.Collections.Generic;
using System.Linq;
using static Meep.Tech.XBam.IO.Sql.Query.Builder;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Clause {
      public class AreEqual : Clause {

      public static Parameter LeftValueParameter {
        get;
      } = new Parameter("LeftValue", typeof(object), true);

      public static Parameter RightValueParameter {
        get;

      } = new Parameter("RightValue", typeof(object), true);

        AreEqual()
          : base(
            new Identity(nameof(AreEqual)),
            new[] {
              LeftValueParameter,
              RightValueParameter
            }.ToList()
          ) { }

        protected AreEqual(Archetype.Identity id, Collection collection = null, Universe universe = null)
          : base(
            id ?? new Identity(nameof(AreEqual)),
            new[] {
              LeftValueParameter,
              RightValueParameter
            }.ToList(),
            collection,
            universe
          ) { }

        // TODO: implement injection here and elsewhere
        protected internal override (string text, IEnumerable<object> parameters) Build(Token token, SqlContext sqlContext, ref int queryParamIndex) => (
            $"{sqlContext.GetValueReplacementKey(queryParamIndex++)} = {sqlContext.GetValueReplacementKey(queryParamIndex++)}",
            new object[] {
              token.Parameters[0],
              token.Parameters[1]
            }
          );
      }
    }
  }
}