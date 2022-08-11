using System.Collections.Generic;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {
    public abstract partial class Clause : Builder.Token.Type {

      /// <summary>
      /// A parameter type for an opperation
      /// </summary>
      public record Parameter(string Name, System.Type ExpectedType, bool IsRequired = false);

      /// <summary>
      /// The parameters this clause uses
      /// </summary>
      public IList<Parameter> Parameters { get; }

      protected Clause(Archetype.Identity id, IList<Clause.Parameter> parameters, Collection collection = null, Universe universe = null)
        : base(id, collection, universe) {
        Parameters = parameters;
      }
    }
  }
}