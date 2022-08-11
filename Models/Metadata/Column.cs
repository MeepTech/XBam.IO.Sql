using Meep.Tech.XBam.Reflection;
using System;
using static Meep.Tech.XBam.IO.Sql.SqlContext;

namespace Meep.Tech.XBam.IO.Sql.Metadata {
  public class Column : Field {

    internal Column(Field fieldData) 
      : base(fieldData) {}

    protected override string GetDataType(Field field) 
      => Table.BaseModel.SqlContext.GetColumnDataType(this);

    protected override object? DefaultDeserialize(RawCellData cell) 
      => Table.BaseModel.SqlContext.DeserializeRawDatabaseValueWithConverters(cell, ValueType ?? cell.ExpectedValueType);

    protected override object? DefaultSerialize(object value)
      => value is Enum 
        ? value.ToString() 
        : Table.BaseModel.SqlContext.SerializeToRawDatabaseValue(value, false);
  }
}