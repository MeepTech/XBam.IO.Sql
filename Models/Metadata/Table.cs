using Meep.Tech.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.XBam.IO.Sql.Metadata {

  /// <summary>
  /// A DB Table for a Model
  /// </summary>
  public class Table : IEnumerable<Column> {
    Dictionary<string, int> _normalizedColumnNames;
    OrderedDictionary<string, Column> _columns;
    Dictionary<PropertyInfo, Column> _columnsByProperty;

    /// <summary>
    /// The name of the table
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The base model this table was made for
    /// </summary>
    public Model BaseModel { get; internal set; }

    /*/// <summary>
    /// The model this table was made for
    /// </summary>
    public Model Model { get; internal set; }*/

    /// <summary>
    /// The columns in this table
    /// </summary>
    public IReadOnlyList<Column> Columns
      => _columns.Values.ToList();

    /// <summary>
    /// Get the column by name (case insensitive)
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public Column this[string columnName]
      => _columns.TryGetValue(columnName, out var found)
        ? found
        : _normalizedColumnNames.TryGetValue(columnName.ToLower(), out var index)
          ? _columns[index]
          : throw new KeyNotFoundException($"Column with the name: {columnName}, not found in Sql Context");

    /// <summary>
    /// Get the column by index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Column this[int index]
      => _columns[index];

    /// <summary>
    /// Get the column by the property that it links to
    /// </summary>
    public Column this[PropertyInfo property]
      => _columnsByProperty[property];

    internal Table(string name, IEnumerable<Column> columns) {
      Name = name;

      _columns = new(columns.ToDictionary(c => c.Name));
      _columns.Values.ForEach(c => c.Table = this);
      _columnsByProperty = Columns
        .Where(c => c.HasBackingProperty)
        .ToDictionary(c => c.Property!);
      _normalizedColumnNames 
        = _columns
          .Select((e, i) => (entry: e, index: i))
          .ToDictionary(
            e => e.entry.Value.Name.ToLower(), 
            e => e.index
          );
    }

    /// <summary>
    /// Try to get a column by name. Case insensitive.
    /// </summary>
    public bool HasColumn(string columnName)
      => _columns.ContainsKey(columnName) || _normalizedColumnNames.ContainsKey(columnName.ToLower());

    /// <summary>
    /// Try to get a column by name. Case insensitive.
    /// </summary>
    public bool TryToGetColumn(string columnName, out Column? column)
      => (column 
        = _columns.TryGetValue(columnName, out var found)
          ? found
          : _normalizedColumnNames.TryGetValue(columnName.ToLower(), out var index)
            ? _columns[index]
            : null
      ) != null;

    public IEnumerator<Column> GetEnumerator() {
      return _columns.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return ((IEnumerable)_columns).GetEnumerator();
    }
  }
}