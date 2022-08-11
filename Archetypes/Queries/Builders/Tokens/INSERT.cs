using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {
    public abstract partial class Opperation {
      public abstract partial class Initial {
        public abstract class INSERT : Initial {

          /// <summary>
          /// The base parameters for the select query.
          /// </summary>
          public static readonly Parameter[] BasicParameters = new[] {
            TableNameParameter,
            ColumnsNamesParameter,
            ColumnsValuesParameter
          };

          public static Parameter ColumnsNamesParameter {
            get;
          } = new Parameter("ColumnNames", typeof(IReadOnlyList<string>), true);

          public static Parameter ColumnsValuesParameter {
            get;
          } = new Parameter("ColumnValues", typeof(IReadOnlyList<object>), true);

          public static Parameter JsonDataPropertiesNamesParameter {
            get;
          } = new Parameter("JsonDataPropertiesNames", typeof(IReadOnlyList<string>));

          public static Parameter JsonDataPropertiesValuesParameter {
            get;
          } = new Parameter("JsonDataPropertiesValues", typeof(IReadOnlyList<object>));

          public virtual string InsertIntoCommandText { get; } = "INSERT INTO";
          public virtual string ValuesCommandText { get; } = "VALUES";

          protected INSERT()
            : base(
              new Identity(nameof(INSERT)),
              BasicParameters.ToList()
            ) { }

          protected INSERT(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
            : base(
              id ?? new Identity(nameof(SELECT), universe: universe),
              BasicParameters
                .Concat(parameters ?? Enumerable.Empty<Parameter>())
                .ToList(),
              collection,
              universe
            ) { }

          #region Helpers

          /// <summary>
          /// Used to get the query replacement key.
          /// </summary>
          protected string GetValueReplacementKey(SqlContext sqlContext, int index, bool withPrefix = true)
            => sqlContext.GetValueReplacementKey(index, withPrefix);

          /// <summary>
          /// Used to seerialize json fields data
          /// </summary>
          protected JToken JsonSerialize(SqlContext sqlContext, object data)
            => sqlContext._jsonSerialize(data);

          /// <summary>
          /// Helper to serialize a value for a query via a model
          /// </summary>
          protected object? SerializeQueryValueViaModel(SqlContext sqlContext, Builder.Token token, string columnName, object? rawValue)
            => sqlContext.SerializeQueryValueViaModel(token, columnName, rawValue);

          #endregion

          protected internal override (string text, IEnumerable<object?> parameters) Build(Builder.Token token, SqlContext sqlContext, ref int queryParamIndex) {
            IReadOnlyList<string> sqlColumnNames = (IReadOnlyList<string>)token.Parameters[1];
            IReadOnlyList<object> sqlColumnValues = (IReadOnlyList<object>)token.Parameters[2];
            IReadOnlyList<string>? jsonPropertyNames = (token.Parameters.Count > 3 ? (IReadOnlyList<string>)token.Parameters[3] : null);
            IReadOnlyList<object>? jsonPropertyValues = (token.Parameters.Count > 4 ? (IReadOnlyList<object>)token.Parameters[4] : null);

            var columnList 
              = ConfigureParamsAndBuildColumnsList(
                ref sqlColumnNames,
                ref jsonPropertyNames, 
                token, 
                ref queryParamIndex
              );

            var valuesList 
              = ConfigueAndBuildValuesList(
                token, 
                sqlContext,
                sqlColumnValues,
                sqlColumnNames,
                jsonPropertyValues,
                jsonPropertyNames,
                ref queryParamIndex
              );

            return (
              InsertIntoCommandText
                + " "
                + (token.Parameters[0] as string)
                + "\n"
                + columnList.text
                + " "
                + ValuesCommandText
                + " "
                + valuesList.text
                + ";",
              (columnList.parameters ?? Enumerable.Empty<object>())
                .Concat(valuesList.parameters ?? Enumerable.Empty<object>())
            );
          }

          /// <summary>
          /// Build the list of columns
          /// </summary>
          protected virtual (string text, IEnumerable<object?>? parameters) ConfigureParamsAndBuildColumnsList(ref IReadOnlyList<string> columns, ref IReadOnlyList<string>? jsonProperties, Builder.Token token, ref int queryParamIndex) {
            if (jsonProperties is not null) {
              if (jsonProperties.Any()) {
                return (BuildColumnsList(columns.Append(SqlContext.JsonDataColumnName)), null);
              }
            }
            else if (token.Model is not null) {
              jsonProperties = new List<string>();
              var sqlColumns = new List<string>();
              foreach (var columnName in columns) {
                if (token.Model.Table.HasColumn(columnName)) {
                  sqlColumns.Add(SqlContext.JsonDataColumnName);
                }
                else {
                  (jsonProperties as List<string>)!.Add(columnName);
                }
              }

              if (jsonProperties.Any()) {
                return (BuildColumnsList(columns.Append(SqlContext.JsonDataColumnName)), null);
              }
            }

            return (BuildColumnsList(columns), null);
          }

          protected virtual string BuildColumnsList(IEnumerable<string> columns)
            => "(\n\t" + string.Join(", \n\t", columns) + "\n)";

          /// <summary>
          /// Build the list of values
          /// </summary>
          protected virtual (string text, IEnumerable<object?>? parameters) ConfigueAndBuildValuesList(
            Builder.Token token,
            SqlContext sqlContext,
            IReadOnlyList<object?> sqlColumnValues,
            IReadOnlyList<string> sqlColumnNames,
            IReadOnlyList<object>? jsonPropertyValues,
            IReadOnlyList<string>? jsonPropertyNames,
            ref int queryParamIndex
          ) {
            string text = "(";
            List<object?> orderedParameters = new();
            int valuesCount = (sqlColumnValues.Count + (jsonPropertyValues?.Count ?? 0));
            int fieldsCount = (sqlColumnNames.Count + (jsonPropertyNames?.Count ?? 0));

            if (valuesCount % fieldsCount != 0) {
              throw new ArgumentException($"The number of values: {valuesCount} is not divisible by the number of columns: {fieldsCount}.");
            }

            for (int index = 0; index < sqlColumnValues.Count; index++) {
              string valueReplacementKey = GetValueReplacementKey(sqlContext, queryParamIndex++);
              var columnName = sqlColumnNames[index % sqlColumnValues.Count];
              var columnValue = token.Model is null
                ? sqlColumnValues[index]
                : SerializeQueryValueViaModel(sqlContext, token, columnName, sqlColumnValues[index]);

              // cap off the end of the list:
              if (index % sqlColumnValues.Count == sqlColumnValues.Count - 1) {
                // add the last value
                orderedParameters.Add(columnValue);
                text += "\n\t " + valueReplacementKey + " ";

                // add the json _data column
                if (jsonPropertyValues is not null && jsonPropertyValues.Any()) {
                  var jsonIndex = 0;
                  var jsonDataKey = GetJsonDataKey(sqlContext, ref queryParamIndex);
                  JObject jsonColumnData = new();

                  for (int i = 0; i < jsonPropertyNames!.Count; i++) {
                    var valueToSerialize = token.Model is null
                      ? jsonPropertyValues[jsonIndex++]
                      : SerializeQueryValueViaModel(sqlContext, token, columnName, jsonPropertyValues[jsonIndex++]);

                    JToken serializedValue = valueToSerialize is null
                      ? JValue.CreateNull()
                      : JsonSerialize(sqlContext, valueToSerialize);

                    jsonColumnData.Add(
                      jsonPropertyNames[i],
                      serializedValue
                    );
                  }

                  orderedParameters.Add(jsonColumnData.ToString());
                  text += ",\n\t " + jsonDataKey + " ";
                }

                // close the list
                text += "\n)";

                // start the next list if there is one
                if (index != sqlColumnValues.Count - 1) {
                  text += ", (\n\t";
                }
              } // add a regular column to the list:
              else {
                text += "\n\t " + valueReplacementKey + " ,";
                orderedParameters.Add(columnValue);
              }
            }

            return (text, orderedParameters);
          }

          protected virtual string GetJsonDataKey(SqlContext sqlContext, ref int queryParamIndex) 
            => GetValueReplacementKey(sqlContext, queryParamIndex++);
        }
      }
    }
  }
}