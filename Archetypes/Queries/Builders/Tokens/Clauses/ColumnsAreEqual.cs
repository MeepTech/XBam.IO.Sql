using System.Collections.Generic;
using System.Linq;
using static Meep.Tech.XBam.IO.Sql.Query.Builder;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Clause {
      public class ColumnsAreEqual : Clause {

        public static Parameter LeftColumnParameter {
          get;
        } = new Parameter("LeftColumnName", typeof(object), true);

        public static Parameter RightColumnParameter {
          get;
        } = new Parameter("RightColumnName", typeof(object), true);

        ColumnsAreEqual()
          : base(
            new Identity(nameof(ColumnsAreEqual)),
            new[] {
              LeftColumnParameter,
              RightColumnParameter
            }.ToList()
          ) { }

        protected ColumnsAreEqual(Archetype.Identity id, Collection collection = null, Universe universe = null)
          : base(
            id ?? new Identity(nameof(ColumnsAreEqual)),
            new[] {
              LeftColumnParameter,
              RightColumnParameter
            }.ToList(),
            collection,
            universe
          ) { }

        // TODO: implement injection here and elsewhere
        protected internal override (string, IEnumerable<object>) Build(Token token, SqlContext sqlContext, ref int queryParamIndex)
          => (
            $"{token.Parameters[0] as string} = {token.Parameters[1] as string}",
            new object[] {}
          );
      }
    }
  }
}