using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public partial class Builder {
      public partial class Token {

        public abstract class Type : Archetype<Token, Type> {

          protected Type(Archetype.Identity id, Collection collection = null, Universe universe = null)
            : base(id, collection, universe) {}

          protected internal abstract (string text, IEnumerable<object?>? parameters) Build(Token token, SqlContext sqlContext, ref int queryParamIndex);

          protected internal Token Make(params object?[] parameters) 
            => base.Make((nameof(Token.Parameters), parameters?.ToList()));

          protected internal Token Make(Metadata.Model? model, Token? previous, params object?[] parameters)
            => base.Make(
              (nameof(Token.Parameters), parameters?.ToList()),
              (nameof(Token.Previous), previous),
              (nameof(Token.Model), model)
            );
        }
      }
    }
  }
}