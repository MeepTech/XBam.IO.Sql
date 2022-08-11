using Meep.Tech.XBam.IO.Sql.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {
    public abstract partial class Opperation {

      public abstract partial class Initial {

        /// <summary>
        /// Used to select items.
        /// Must be overriden
        /// </summary>
        public abstract class SELECT : Initial {

          /// <summary>
          /// The base parameters for the select query.
          /// </summary>
          public static readonly Parameter[] BasicParameters = new[] {
            TableNameParameter,
            ColumnsNamesParameter,
            JsonDataPropertiesNamesParameter
          };

          public static Parameter ColumnsNamesParameter {
            get;
          } = new Parameter("ColumnNames", typeof(IReadOnlyList<string>));

          public static Parameter JsonDataPropertiesNamesParameter {
            get;
          } = new Parameter("JsonDataProperties", typeof(IReadOnlyList<string>));

          public virtual string SelectCommandText { get; } = "SELECT";
          public virtual string AllColumnsSelectorText { get; } = "*";
          public virtual string FromTableCommandText { get; } = "FROM";

          protected SELECT()
            : base(
              new Identity(nameof(SELECT)),
              BasicParameters.ToList()
            ) { }

          protected SELECT(Archetype.Identity id, IList<Parameter> parameters, Collection collection = null, Universe universe = null)
            : base(
              id ?? new Identity(nameof(SELECT), universe: universe),
              BasicParameters.Concat((parameters ?? Enumerable.Empty<Parameter>()))
                .ToList(),
              collection,
              universe
            ) { }

          protected internal override (string text, IEnumerable<object> parameters) Build(Builder.Token token, SqlContext sqlContext, ref int queryParamIndex) => (
            SelectCommandText
              + BuildSqlColumns(token, sqlContext, token.Parameters.Count > 1 ? token.Parameters[1] as IReadOnlyList<string> : null, token.Parameters.Count > 2 ? token.Parameters[2] as IReadOnlyList<string> : null)
              + $"\n{FromTableCommandText} "
              + (token.Parameters[0] as string)
              + "\n",
            Enumerable.Empty<object>()
          );

          protected virtual string BuildSqlColumns(Builder.Token token, SqlContext sqlContext, IReadOnlyList<string>? sqlColumns, IReadOnlyCollection<string>? jsonProperties) {
            if((sqlColumns is null || !sqlColumns.Any()) && (jsonProperties is null || !jsonProperties.Any())) {
              return BuildAllColumnsText(token.Model);
            } else {
              if (jsonProperties is not null) {
                var text = "\n\t" + string.Join(", \n\t", sqlColumns);
                if (!jsonProperties.Any()) {
                  return text;
                }

                text += "\n\t" + string.Join(
                  ", \n\t",
                  jsonProperties.Select(
                    j => BuildJsonDataPropertySelectColumn(j))
                );

              } else if (token.Model is not null) {
                return "\n\t" + string.Join(", \n\t", sqlColumns.Select(
                  c => token.Model.TryToGetJsonDataProperty(c, out var jsonData) 
                    ? BuildJsonDataPropertySelectColumn(c) 
                    : c));
              }

              return "\n\t" + string.Join(", \n\t", sqlColumns);
            }
          }

          /// <summary>
          /// Build the text for selecting all columns.
          /// </summary>
          protected virtual string BuildAllColumnsText(Metadata.Model? model = null) 
            => model is null
              ? " " + AllColumnsSelectorText + " "
              : "\n\t"
                + string.Join(",\n\t", model.Table.Select(c => c.Name)
                  .Concat(model.JsonDataProperties.Select(p => BuildJsonDataPropertySelectColumn(p.Name))));

          /// <summary>
          /// Build the json select column, with an as clause, like:
          /// '_data.PropertyName as PropertyName'
          /// </summary>
          protected internal virtual string BuildJsonDataPropertySelectColumn(string propertyName)
            => SqlContext.JsonDataColumnName + "." + propertyName + " AS " + propertyName;
        }
      }
    }
  }
}