using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public partial class SelectClauseOpperation {
        public new class Type : Builder.Type {

          Type() : base(new Type.Identity(new(nameof(SelectClauseOpperation)))) { }

          protected Type(Archetype.Identity id, Collection collection = null, Universe universe = null)
            : base(id, collection, universe) { }

          internal protected SelectClauseOpperation Make(IBuilder previousOpperation, Token token)
            => Make<SelectClauseOpperation>(
              (nameof(Builder.Tokens), previousOpperation.Tokens.Append(token).ToList()),
              (nameof(Builder.SqlContext), previousOpperation.SqlContext)
            );
        }
      }
    }
  }
}