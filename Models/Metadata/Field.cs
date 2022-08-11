using Meep.Tech.Collections.Generic;
using System;
using Meep.Tech.XBam.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using static Meep.Tech.XBam.IO.Sql.SqlContext;

namespace Meep.Tech.XBam.IO.Sql.Metadata {

  /// <summary>
  /// The base for json and sql metadata columns.
  /// </summary>
  public class Field {
    string? _name;
    System.Type? _valueType;
    internal string? _dataType;
    SaveDataAttribute.Getter? _getter;
    SaveDataAttribute.Setter? _setter;
    Func<object?, object?>? _serializer;
    Func<RawCellData, object?>? _deserializer;

    /// <summary>
    /// The name of this column
    /// </summary>
    public string Name
      => _name ?? _getPropertyName(this);

    /// <summary>
    /// The property this column was derivd from 
    /// (if one exists, this is null for extra columns)
    /// </summary>
    public PropertyInfo? Property { get; internal protected set; }

    /// <summary>
    /// The provided column attribute for this field if there was one
    /// </summary>
    public ColumnAttribute? Attribute { get; internal protected set; }

    /// <summary>
    /// The provided save data attribute for this field if there was one
    /// </summary>
    public SaveDataAttribute? Data { get; internal protected set; }

    /// <summary>
    /// The table this field belongs to
    /// </summary>
    public Table Table { get; internal set; }

    /// <summary>
    /// If this column has a property on the model backing it
    /// </summary>
    public bool HasBackingProperty
      => Property is not null;

    /// <summary>
    /// The value type of this column
    /// </summary>
    public System.Type? ValueType
      => _valueType ??= Property?.PropertyType;

    /// <summary>
    /// The datatype used for this column
    /// </summary>
    public virtual string DataType
      => _dataType ??= Attribute?.TypeName ?? GetDataType(this);

    /// <summary>
    /// Used to get the data type of a field.
    /// </summary>
    protected virtual string GetDataType(Field field)
      => throw new NotImplementedException();

    /// <summary>
    /// Get the default deserializer for a field type
    /// </summary>
    protected virtual object? DefaultDeserialize(RawCellData cell)
      => throw new NotImplementedException();

    protected virtual object? DefaultSerialize(object value)
      => throw new NotImplementedException();

    internal virtual SaveDataAttribute.Getter Get => _getter
      ??= Data?.GetGetterOverride(Property, Table.BaseModel.SqlContext.Universe) ?? (Property is null
      ? throw new NotImplementedException($"Getter Not Implemented For the Field: {Name}, on Model: {Table.Name}")
      : Property.Get);

    internal virtual SaveDataAttribute.Setter Set => _setter
      ??= Data?.GetSetterOverride(Property, Table.BaseModel.SqlContext.Universe) ?? (Property is null
        ? throw new NotImplementedException($"Setter Not Implemented For the Field: {Name}, on Model: {Table.Name}")
        : Property.Set);

    internal virtual Func<object?, object?> Serializer => _serializer
      ??= Data is not null 
        ? Data.SerializerToRawOverride(Table.BaseModel.SqlContext.Universe)
          ?? (r => DefaultSerialize(r))
        : r => DefaultSerialize(r);

    internal virtual Func<RawCellData, object?> Deserializer => _deserializer
      ??= Data is not null
        ? Data.DeserializerFromRawOverride(Table.BaseModel.SqlContext.Universe)
          ?? new Func<object?, object?>(r => DefaultDeserialize((RawCellData)r!))
        : r => DefaultDeserialize(r);

    internal Field(PropertyInfo? property, ColumnAttribute? attribute, SaveDataAttribute? data, string? dataType = null) {
      Property = property;
      Attribute = attribute;
      Data = data;
      _dataType = dataType;
    }

    internal Field(Field field) {
      Property = field.Property;
      Attribute = field.Attribute;
      Data = field.Data;
      _dataType = field._dataType;
      _valueType = field._valueType;
      _name = field._name;
      _getter = field._getter;
      _setter = field._setter;
      Table = field.Table;
    }

    static string _getPropertyName(Field column)
      => column.Attribute?.Name
        ?? column.Data?.PropertyNameOverride
        ?? column.Property?.Name
        ?? throw new ArgumentNullException("Property Name");
  }
}