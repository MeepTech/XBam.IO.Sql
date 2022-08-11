using System.Collections.Generic;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public partial class SelectOpperation {
        public new class Type : Builder.Type {

          Type() : base(new Type.Identity(nameof(SelectOpperation))) { }

          protected Type(Archetype.Identity id, Collection collection = null, Universe universe = null)
            : base(id, collection, universe) { }

          internal protected SelectOpperation Make(SqlContext sqlContext, Token initialToken)
            => Make<SelectOpperation>(
              (nameof(Builder.SqlContext), sqlContext),
              (nameof(Builder.Tokens), new List<Token> { initialToken })
            );
        }
      }
    }
  }
}