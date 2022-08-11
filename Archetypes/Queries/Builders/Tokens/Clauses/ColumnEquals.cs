using System.Collections.Generic;
using System.Linq;
using static Meep.Tech.XBam.IO.Sql.Query.Builder;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Clause {
      public class ColumnEquals : Clause {

          public static Parameter ColumnParameter {
            get;
          } = new Parameter("ColumnName", typeof(object), true);

          public static Parameter ValueParameter {
            get;

          } = new Parameter("Value", typeof(object), true);

          ColumnEquals()
          : base(
            new Identity(nameof(ColumnEquals)),
            new[] {
              ColumnParameter,
              ValueParameter
            }.ToList()
          ) { }

        protected ColumnEquals(Archetype.Identity id, Collection collection = null, Universe universe = null)
          : base(
            id ?? new Identity(nameof(ColumnEquals)),
            new[] {
              ColumnParameter,
              ValueParameter
            }.ToList(),
            collection,
            universe
          ) { }

        // TODO: implement injection here and elsewhere
        protected internal override (string, IEnumerable<object>) Build(Token token, SqlContext sqlContext, ref int queryParamIndex) => (
            $"{token.Parameters[0] as string} = {sqlContext.GetValueReplacementKey(queryParamIndex++)}",
            new object[] {
              token.Parameters[1]
            }
          );
      }
    }
  }
}