using System;

namespace Meep.Tech.XBam.IO.Sql {

  public abstract partial class SqlContext {

    /// <summary>
    /// Used to make return data for a row.
    /// </summary>
    public record RawCellData {
      public string ColumnName { get; internal init; }
      public string SqlDataTypeName { get; internal init; }
      public object? Value { get; internal init; }
      public System.Type ExpectedValueType { get; internal init; }
      internal RawCellData(string columnName, string sqlDataTypeName, object value, Type expectedValueType) {
        ColumnName = columnName;
        SqlDataTypeName = sqlDataTypeName;
        Value = value;
        ExpectedValueType = expectedValueType;
      }
    }
  }
}