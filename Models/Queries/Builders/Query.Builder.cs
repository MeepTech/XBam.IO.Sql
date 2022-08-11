using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Meep.Tech.XBam.IO.Sql.Query.Builder;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    /// <summary>
    /// Interface for a Query Builder
    /// </summary>
    public interface IBuilder {

      public SqlContext SqlContext { get;}
      public IReadOnlyList<Token> Tokens { get;}
      Query Build();
    }

    /// <summary>
    /// A builder for a simple SQL query
    /// </summary>
    [NotMapped]
    public abstract partial class Builder : Model<Builder, Builder.Type>, IBuilder {
      static SqlContext _getCurrentSqlContext => Archetypes.DefaultUniverse.Sql();
      static List<Token> _getEmptyTokenList => new();
      List<Token> _tokens = new();

      /// <summary>
      /// The sql context being used to build this query
      /// </summary>
      [AutoBuild(IsRequiredAsAParameter = true), NotNull]
      [GetTestValueFromMember(nameof(_getCurrentSqlContext))]
      public SqlContext SqlContext {
        get;
        private set;
      }

      /// <summary>
      /// The tokens currently making up this builder
      /// </summary>
      [AutoBuild, Required, NotNull]
      [GetTestValueFromMember(nameof(_getEmptyTokenList))]
      public IReadOnlyList<Token> Tokens {
        get => _tokens;
        private set => _tokens = value.ToList();
      }

      /// <summary>
      /// Used to make other types of builders and other builder steps.
      /// </summary>
      protected Builder(IBuilder<Builder> builder) {}

      /// <summary>
      /// Seal off this builder into a query.
      /// </summary>
      public virtual Query Build()
        => Archetype.Build(this);

      protected ClosedOpperation BuildClosedQuery(Query.Opperation opperation, Metadata.Model? model = null, params object[] parameters)
        => SqlContext.BuildClosedQuery(opperation, this, model, parameters);
    }
  }
}