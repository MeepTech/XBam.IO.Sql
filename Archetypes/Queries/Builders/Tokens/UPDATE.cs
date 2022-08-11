using Meep.Tech.Collections.Generic;
using Meep.Tech.XBam.IO.Sql.Metadata;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using static Meep.Tech.XBam.IO.Sql.Query.Builder;
using static System.Net.Mime.MediaTypeNames;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

public abstract partial class Opperation {

      public abstract partial class Initial {
        public abstract class UPDATE : Initial {

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

          public virtual string UpdateCommandText { get; } = "UPDATE";
          public virtual string SetColumnValueCommandText { get; } = "SET";

          protected UPDATE()
            : base(
              new Identity(nameof(UPDATE)),
              BasicParameters.ToList()
            ) { }

          protected UPDATE(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
            : base(
              id ?? new Identity(nameof(UPDATE), universe: universe),
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
          protected virtual string GetValueReplacementKey(SqlContext sqlContext, int index, bool withPrefix = true, bool forJsonField = false)
            => sqlContext.GetValueReplacementKey(index, withPrefix);

          /// <summary>
          /// Helper to serialize a value for a query via a model
          /// </summary>
          protected object? SerializeQueryValueViaModel(SqlContext sqlContext, Builder.Token token, string columnName, object? rawValue)
            => sqlContext.SerializeQueryValueViaModel(token, columnName, rawValue);

          #endregion

          protected internal override (string text, IEnumerable<object?>? parameters) Build(Builder.Token token, SqlContext sqlContext, ref int queryParamIndex) {
            IReadOnlyList<string> sqlColumnNames = (IReadOnlyList<string>)token.Parameters[1];
            IReadOnlyList<object> sqlColumnValues = (IReadOnlyList<object>)token.Parameters[2];
            IReadOnlyList<string>? jsonPropertyNames = (token.Parameters.Count > 3 ? (IReadOnlyList<string>)token.Parameters[3] : null);
            IReadOnlyList<object>? jsonPropertyValues = (token.Parameters.Count > 4 ? (IReadOnlyList<object>)token.Parameters[4] : null);

            var (text, parameters) = ConfigueAndBuildValuesList(
                token,
                sqlContext,
                sqlColumnValues,
                sqlColumnNames,
                jsonPropertyValues,
                jsonPropertyNames,
                ref queryParamIndex
              );

            return (
              UpdateCommandText
                + " "
                + (token.Parameters[0] as string)
                + "\n"
                + text
                + "\n",
              parameters
            );
          }

          protected virtual (string text, IEnumerable<object?>? parameters) ConfigueAndBuildValuesList(
            Builder.Token token,
            SqlContext sqlContext,
            IReadOnlyList<object?> sqlColumnValues,
            IReadOnlyList<string> sqlColumnNames,
            IReadOnlyList<object?>? jsonPropertyValues,
            IReadOnlyList<string>? jsonPropertyNames,
            ref int queryParamIndex
          ) {
            List<string> textParts = new();
            List<object?> orderedParameters = new();

            if (jsonPropertyNames is null && token.Model is not null) {
              jsonPropertyNames = new List<string>();
              jsonPropertyValues = new List<object?>();
              var sqlColumns = new List<string>();
              var sqlValues = new List<object?>();

              for (int i = 0; i < sqlColumnValues.Count; i++) {
                var columnName = sqlColumnNames[i];
                var columnValue = sqlColumnValues[i];

                if (token.Model.Table.HasColumn(columnName)) {
                  sqlColumns.Add(SqlContext.JsonDataColumnName);
                  sqlValues.Add(columnValue);
                }
                else {
                  (jsonPropertyNames as List<string>)!.Add(columnName);
                  (jsonPropertyValues as List<object?>)!.Add(columnValue);
                }
              }

              sqlColumnNames = sqlColumns;
              sqlColumnValues = sqlValues;
            }

            foreach(var entry in sqlColumnNames.Select((c, i) => (
              name: c,
              value: token.Model is null 
                ? sqlColumnValues[i] 
                : SerializeQueryValueViaModel(sqlContext, token, c, sqlColumnValues[i]),
              isJson: false
            )).Concat(jsonPropertyNames
              ?.Select(
                (p, i) => (
                  name: p,
                  value: token.Model is null 
                    ? jsonPropertyValues![i] 
                    : SerializeQueryValueViaModel(sqlContext, token, p, jsonPropertyValues![i]),
                  isJson: true
                )) 
              ?? Enumerable.Empty<(string, object?, bool)>()
            )) {
              var parameterKey = GetValueReplacementKey(sqlContext, queryParamIndex++, forJsonField: entry.isJson);
              textParts.Add(BuildSetStatement(entry, parameterKey));
              orderedParameters.Add(entry.value);
            }

            return ("\n\t" + string.Join(",\n\t", textParts), orderedParameters);
          }

          protected virtual string BuildSetStatement((string name, object? value, bool isJson) entry, string parameterKey) {
            return SetColumnValueCommandText + " " + (entry.isJson ? (SqlContext.JsonDataColumnName + ".") : "") + entry.name + " = " + parameterKey;
          }
        }
      }
    }
  }
}