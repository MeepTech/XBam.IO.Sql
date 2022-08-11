using Meep.Tech.XBam.Reflection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Meep.Tech.XBam.IO.Sql.Metadata {
  public class JsonDataProperty : Field {
    internal JsonDataProperty(Field field) : base(field) { }

    protected override string GetDataType(Field field)
      => "json";

    protected override object? DefaultDeserialize(SqlContext.RawCellData cell) {
      if (cell.Value is null) {
        return null;
      }

      JToken token;
      string cellJsonText = cell.Value.ToString();

      if (ValueType!.IsNumeric()) {
        token = JToken.Parse(cellJsonText);
      }
      else if (ValueType!.IsText() || typeof(Enum).IsAssignableFrom(ValueType!))
        return cellJsonText;
      else {
        token = JToken.Parse(cellJsonText);
      }

      if (token.Type == JTokenType.Null) {
        return null;
      }

      var cellValue = token.ToObject(ValueType!);
      cell = cell with { Value = cellValue };

      return Table.BaseModel.SqlContext.DeserializeRawDatabaseValueWithConverters(cell, ValueType!);
    }

    protected override object? DefaultSerialize(object value)
      => value is Enum
        ? value.ToString()
        : Table.BaseModel.SqlContext.SerializeToRawDatabaseValue(value, true);
  }
}
