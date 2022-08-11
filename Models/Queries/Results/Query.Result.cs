using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {
  public partial class Query {

    /// <summary>
    /// Results returned from a query
    /// </summary>
    public partial class Result {

      /// <summary>
      /// The success status
      /// </summary>
      public bool Success 
        { get; }

      /// <summary>
      /// The returned rows on success
      /// </summary>
      public IReadOnlyList<Row>? Rows 
        { get; }

      /// <summary>
      /// The error if there was a failure
      /// </summary>
      public System.Exception? Error 
        { get; }

      /// <summary>
      /// The query this was executed by
      /// </summary>
      public Query? Query 
        { get; internal set; }

      /// <summary>
      /// The sql context used to execute this
      /// </summary>
      public SqlContext SqlContext 
        { get; internal set; }

      internal Result(IEnumerable<IEnumerable<SqlContext.RawCellData>>? rows, bool success, Exception? error) {
        Rows = rows?.Select(r => new Row(r, this)).ToList();
        Success = success;
        Error = error;
      }
    }
  }
}