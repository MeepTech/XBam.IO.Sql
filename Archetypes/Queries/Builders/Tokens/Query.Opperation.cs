using System.Collections.Generic;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    /// <summary>
    /// Potential Query opperation tokens
    /// </summary>
    public abstract partial class Opperation : Builder.Token.Type {
      
      /// <summary>
      /// A parameter that can be used for table name
      /// </summary>
      public static Parameter TableNameParameter {
        get;
      } = new Parameter("TableName", typeof(string), true);

      /// <summary>
      /// A parameter type for an opperation
      /// </summary>
      public record Parameter(string Name, System.Type ExpectedType, bool IsRequired = false);

      /// <summary>
      /// The parameters this opperation uses
      /// </summary>
      public IList<Parameter> Parameters { get; }

      /// <summary>
      /// Potential initial Query opperations
      /// </summary>
      public abstract partial class Initial : Opperation {

        protected Initial(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
          : base(id, parameters, collection, universe) { }
      }

      /// <summary>
      /// Potential secondary Query opperations
      /// </summary>
      public abstract partial class Secondary : Opperation {

        protected Secondary(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
          : base(id, parameters, collection, universe) { }
      }

      /// <summary>
      /// Potential clause Query opperations
      /// </summary>
      public abstract partial class Clause : Opperation {

        protected Clause(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
          : base(id, parameters, collection, universe) { }
      }

      protected Opperation(Archetype.Identity id, IList<Opperation.Parameter> parameters, Collection collection = null, Universe universe = null)
        : base(id, collection, universe) {
        Parameters = parameters;
      }
    }
  }
}