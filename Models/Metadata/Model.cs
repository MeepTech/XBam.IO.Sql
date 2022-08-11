using Meep.Tech.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql.Metadata {


  /// <summary>
  /// Represents a model type/class that can be saved to the DB
  /// </summary>
  public class Model {
    internal readonly System.Func<Query.Result.Row, object> _modelConstructor;
    OrderedDictionary<string, JsonDataProperty> _properties;
    Dictionary<string, int> _normalizedPropertyNames;
    string? _allColumnsListText;
    IEnumerable<string>? _allColumnsNamesList;

    /// <summary>
    /// The sql context this is for
    /// </summary>
    public SqlContext SqlContext { get; }

    /// <summary>
    /// The Model type this table is for
    /// </summary>
    public System.Type SystemType { get; }

    /// <summary>
    /// The table built to hold data for this model
    /// </summary>
    public Table Table { get; }

    /// <summary>
    /// Json Property Fields in the _data SQL column.
    /// </summary>
    public IEnumerable<JsonDataProperty> JsonDataProperties
      => _properties.Values;

    // TODO: cache these somewhere else.
    /*public IEnumerable<string> AllColumnsList {
      get {
        if (_allColumnsNamesList == null) {
          _allColumnsNamesList = Table.Select(c => c.Name)
            .Concat(JsonDataProperties.Select(p => (SqlContext.OperationTypes[typeof(Query.Opperation.Initial.SELECT
            )] as Query.Opperation.Initial.SELECT).BuildJsonDataPropertySelectColumn(p.Name)));
        }

        return _allColumnsNamesList;
      }
    }

    // TODO: cache these somewhere else.
    public string AllColumnsListText {
      get {
        if (_allColumnsListText == null) {
          _allColumnsListText = string.Join(", \n\t", AllColumnsList);
        }

        return _allColumnsListText;
      }
    }*/

    internal Model(Table table, IEnumerable<JsonDataProperty> jsonDataProperties, SqlContext sqlContext, System.Type modelType, System.Func<Query.Result.Row, object> modelConstructor) {
      Table = table;
      Table.BaseModel = this;
      SqlContext = sqlContext;
      SystemType = modelType;
      _properties = new(jsonDataProperties.ToDictionary(p => p.Name));
      _normalizedPropertyNames = _properties.Select((p, i) => (p, i)).ToDictionary(e => e.p.Key.ToLower(), e => e.i);
      JsonDataProperties.ForEach(p => p.Table = Table);

      _modelConstructor = modelConstructor;
    }

    /// <summary>
    /// Get a json data property by it's key (case insensitive)
    /// </summary>
    public JsonDataProperty GetJsonDataProperty(string propertyKey) 
      => _properties.TryGetValue(propertyKey, out JsonDataProperty? jsonDataProperty)
        ? jsonDataProperty
        : _properties[_normalizedPropertyNames[propertyKey.ToLower()]];

    /// <summary>
    /// Try to get a json data property if it exists (case insensitive)
    /// </summary>
    public bool TryToGetJsonDataProperty(string propertyKey, out JsonDataProperty jsonDataProperty) {
      if (_properties.TryGetValue(propertyKey, out jsonDataProperty)) {
        return true;
      }

      if (_normalizedPropertyNames.TryGetValue(propertyKey.ToLower(), out var index)) {
        jsonDataProperty = _properties[index];
        return true;
      }

      /*if (SqlContext.TryToDeconstructJsonDataColumnName(propertyKey, out var jsonPropertyName)) {
        return TryToGetJsonDataProperty(jsonPropertyName, out jsonDataProperty);
      }*/

      return false;
    }
  }
}
