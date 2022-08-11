using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public partial class JoinOpperation {
        public new class Type : Builder.Type {

          Type() : base(new Type.Identity(nameof(JoinOpperation))) { }

          protected Type(Archetype.Identity id, Collection collection = null, Universe universe = null)
            : base(id, collection, universe) { }

          internal protected JoinOpperation Make(Query.Builder.ICanJoin initialOpperation, Token token)
            => Make<JoinOpperation>(
              (nameof(Builder.Tokens), initialOpperation.Tokens.Append(token).ToList()),
              (nameof(Builder.SqlContext), initialOpperation.SqlContext)
            );
        }
      }
    }
  }
}