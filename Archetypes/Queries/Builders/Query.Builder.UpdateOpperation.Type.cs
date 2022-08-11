using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {
      public partial class UpdateOpperation {
        public new class Type : Builder.Type {

          Type() : base(new Type.Identity(nameof(UpdateOpperation))) { }

          protected Type(Archetype.Identity id, Collection collection = null, Universe universe = null)
            : base(id, collection, universe) { }

          internal protected UpdateOpperation Make(Token token, SqlContext sqlContext)
            => Make<UpdateOpperation>(
              (nameof(Builder.Tokens), new[] { token }.ToList()),
              (nameof(Builder.SqlContext), sqlContext)
            );
        }
      }
    }
  }
}