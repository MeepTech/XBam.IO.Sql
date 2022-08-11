using Meep.Tech.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using static Meep.Tech.XBam.IO.Sql.Query.Builder;

namespace Meep.Tech.XBam.IO.Sql {

  /// <summary>
  /// Represents a Built Query that can be executed for results.
  /// </summary>
  public partial class Query {
    private (string text, IReadOnlyList<object> parameters)? _compiledQuery;

    /// <summary>
    /// The sql context for this query
    /// </summary>
    public SqlContext SqlContext 
      { get; }

    /// <summary>
    /// The tokens making up this query
    /// </summary>
    public IEnumerable<Token> Tokens
      { get; }

    internal Query(SqlContext sqlContext, IReadOnlyList<Token> tokens) {
      SqlContext = sqlContext;
      Tokens = tokens;
    }

    /// <summary>
    /// Execute the query
    /// </summary>
    public Result Execute() {
      _compiledQuery ??= _build();

      var results = SqlContext.ExecuteQuery(
        _compiledQuery.Value.text, 
        _compiledQuery.Value.parameters,
        this
      );

      return results;
    }

    public override string ToString()
      => (_compiledQuery ??= _build()).text;

    internal (string text, IReadOnlyList<object> parameters) _build() {
      if (_compiledQuery.HasValue) {
        return _compiledQuery.Value;
      }

      string text = "-- XBam Query\n\n";
      List<object> parameters = new();

      int queryParamIndex = 0;
      Tokens.ForEach(t => {
        var tokenResults = t.Build(SqlContext, ref queryParamIndex);
        text += tokenResults.text;
        parameters.AddRange(tokenResults.parameters);
      });

      return (_compiledQuery = (text, parameters)).Value;
    }
  }
}