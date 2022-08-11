using Meep.Tech.Collections.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    /// <summary>
    /// Can be passed into a query to reserve space for a parameter for later.
    /// </summary>
    public struct Placeholder {
      internal Func<object?, object?>? Serializer;
      public static Placeholder Make()
       => new();
      public static IEnumerable<Placeholder> Make(int count)
        => count.Of(_ => new Placeholder());
      internal object Replace(object value)
        => Serializer?.Invoke(value) ?? value;
    }

    public partial class Builder {

      [NotMapped]
      public partial class Token : Model<Token, Token.Type> {

        /// <summary>
        /// The model for this token, if there is one.
        /// </summary>
        [AutoBuild]
        public Metadata.Model Model {
          get;
          internal set;
        }

        /// <summary>
        /// The parameters passed to this token
        /// </summary>
        [AutoBuild]
        public IReadOnlyList<object> Parameters {
          get;
          internal set;
        }

        [AutoBuild]
        public Token Previous {
          get;
          private set;
        }

        public virtual (string text, IEnumerable<object> parameters) Build(SqlContext sqlContext, ref int queryParamIndex)
          => Archetype.Build(this, sqlContext, ref queryParamIndex);

        /// <summary>
        /// Used to make a new type of token
        /// </summary>
        /// <param name="builder"></param>
        protected Token(IBuilder<Token> builder) { }
      }
    }
  }
}