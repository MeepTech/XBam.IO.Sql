using Meep.Tech.Collections.Generic;
using Meep.Tech.XBam.IO.Sql.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {
  public partial class Query {
    public partial class Result {

      /// <summary>
      /// A row returned for a result set
      /// </summary>
      public class Row {
        Dictionary<string, int> _normalizedColumnNames;
        readonly OrderedDictionary<string, SqlContext.RawCellData> _columnValues;

        /// <summary>
        /// The result set this row is part of
        /// </summary>
        public Result Set { get; }

        /// <summary>
        /// The column names for the result columns.
        /// </summary>
        public IReadOnlyList<string> ColumnNames
          => _columnValues.Keys.ToList();

        /// <summary>
        /// Get the row value for the given column
        /// </summary>
        public object? this[Column column]
          => this[column.Name];

        /// <summary>
        /// Get the row value for the given column
        /// </summary>
        public object? this[string columnName] {
          get {
            if (_columnValues.TryGetValue(columnName, out var foundRawData)) {
              return Set.SqlContext._deserializeRawSqlValue(foundRawData);
            }

            string normalizedColumnName = columnName.ToLower();
            if (_normalizedColumnNames.TryGetValue(normalizedColumnName, out var index)) {
              return Set.SqlContext._deserializeRawSqlValue(_columnValues[index]);
            }

            throw new KeyNotFoundException($"Column with the name: {columnName}, not found in Sql Context");
          }
        }

        /// <summary>
        /// Get the row value for the given column
        /// </summary>
        public object? this[int columnIndex]
          => Set.SqlContext._deserializeRawSqlValue(_columnValues[columnIndex]);

        internal Row(IEnumerable<SqlContext.RawCellData>? columnValues, Result resultSet) {
          _columnValues = new(
            columnValues
              ?.ToDictionary(
                e => e.ColumnName,
                e => e
              ) ?? new()
          );

          _normalizedColumnNames = columnValues
            ?.Select((e, i) => (entry: e, index: i))
            .ToDictionary(
              e => e.entry.ColumnName.ToLower(),
              e => e.index
            ) ?? new();

          Set = resultSet;
        }

        /// <summary>
        /// Get the column name by index
        /// </summary>
        public string GetColumn(int index)
          => _columnValues.GetKeyAtIndex(index);

        /// <summary>
        /// Try to get a column by name. Case insensitive.
        /// </summary>
        public bool TryToGetColumnValue(string columnName, out object? value) {
          if (_columnValues.TryGetValue(columnName, out var foundRawData)) {
            value
              = Set.SqlContext._deserializeRawSqlValue(foundRawData);
            return true;
          }

          string normalizedColumnName = columnName.ToLower();
          if (_normalizedColumnNames.TryGetValue(normalizedColumnName, out var index)) {
            value
              = Set.SqlContext._deserializeRawSqlValue(foundRawData);
            return true;
          }

          value = null;
          return false;
        }

        /// <summary>
        /// Get the row value for the given column
        /// </summary>
        public object? GetColumnValue(string columnName, Metadata.Model? forModel = null) {
          if (_columnValues.TryGetValue(columnName, out var foundRawData)) {
            return (forModel?.SqlContext ?? Set.SqlContext)._deserializeRawSqlValue(foundRawData, forModel);
          }

          string normalizedColumnName = columnName.ToLower();
          if (_normalizedColumnNames.TryGetValue(normalizedColumnName, out var index)) {
            return (forModel?.SqlContext ?? Set.SqlContext)._deserializeRawSqlValue(_columnValues[index], forModel);
          }

          throw new KeyNotFoundException($"Column with the name: {columnName}, not found in Sql Context");
        }

        /// <summary>
        /// Get the row value for the given column
        /// </summary>
        public object? GetColumnValue(int columnIndex, Metadata.Model? forModel = null) 
          => (forModel?.SqlContext ?? Set.SqlContext)._deserializeRawSqlValue(_columnValues[columnIndex], forModel);

        /// <summary>
        /// Try to get a column by name. Case insensitive.
        /// </summary>
        public bool TryToGetColumnValue(string columnName, Metadata.Model forModel, out object? value) {
          if (_columnValues.TryGetValue(columnName, out var foundRawData)) {
            value = (forModel?.SqlContext ?? Set.SqlContext)._deserializeRawSqlValue(foundRawData, forModel);
            return true;
          }

          string normalizedColumnName = columnName.ToLower();
          if (_normalizedColumnNames.TryGetValue(normalizedColumnName, out var index)) {
            value = (forModel?.SqlContext ?? Set.SqlContext)._deserializeRawSqlValue(_columnValues[index], forModel);
            return true;
          }

          value = null;
          return false;
        }

        /// <summary>
        /// Try to convert a selected row to a model via the SqlContext metadata.
        /// </summary>
        public TModel ToModel<TModel>()
          where TModel : class
            => (TModel)ToModel();

        /// <summary>
        /// Try to convert a selected row to a model via the SqlContext metadata.
        /// </summary>
        public object ToModel() {
          Builder.Token firstToken = Set.Query?.Tokens.First()
            ?? throw new System.InvalidOperationException($"You must have Model data in the first token of a query to deserialize rows directly to Models");

          if (firstToken.Model is not null) { 
            if (firstToken.Archetype is Query.Opperation.Initial.SELECT) {
              IEnumerable<string>? secondSelectParam = firstToken.Parameters.Count > 1 ? firstToken.Parameters[1] as IEnumerable<string> : null;
              if (secondSelectParam is null || !secondSelectParam.Any()) {
                return Set.Query.SqlContext._deserializeToModelUsingMetadata(this, firstToken.Model);
              } else throw new System.InvalidOperationException($"Only rows aquired using a 'SELECT ALL' query can be converted directly to models");
            } else throw new System.InvalidOperationException($"Only rows aquired using a 'SELECT ALL' query can be converted directly to models");
          } else throw new System.InvalidOperationException($"You must have Model data in the first token of a query to deserialize rows directly to Models");
        }
      }
    }
  }
}