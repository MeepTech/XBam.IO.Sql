using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public partial class ClauseOpperation {

        public new class Type : Builder.Type {

          Type() : base(new Type.Identity(nameof(ClauseOpperation))) { }

          protected Type(Archetype.Identity id, Collection collection = null, Universe universe = null)
            : base(id, collection, universe) { }

          internal protected ClauseOpperation Make(IBuilder previousOpperation, Token token)
            => Make<ClauseOpperation>(
              (nameof(Builder.Tokens), previousOpperation.Tokens.Append(token).ToList()),
              (nameof(Builder.SqlContext), previousOpperation.SqlContext)
            );
        }
      }
    }
  }
}