using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public partial class ClosedOpperation {
        public new class Type : Builder.Type {

          Type() : base(new Type.Identity(nameof(ClosedOpperation))) { }

          protected Type(Archetype.Identity id, Collection collection = null, Universe universe = null)
            : base(id, collection, universe) { }

          internal protected ClosedOpperation Make(Token token, Builder? previousOpperation = null, SqlContext? sqlContext = null)
            => Make<ClosedOpperation>(
              (nameof(Builder.Tokens), (previousOpperation?.Tokens ?? Enumerable.Empty<Token>()).Append(token).ToList()),
              (nameof(Builder.SqlContext), previousOpperation?.SqlContext ?? sqlContext)
            );
        }
      }
    }
  }
}