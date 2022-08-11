using Meep.Tech.Collections.Generic;
using System;
using Meep.Tech.Reflection;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Meep.Tech.XBam.IO.Sql.Metadata;
using static Meep.Tech.XBam.IO.Sql.Query.Clause;
using Meep.Tech.XBam.IO.Configuration;
using static Meep.Tech.XBam.IO.Sql.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.Common;

namespace Meep.Tech.XBam.IO.Sql {

  /// <summary>
  /// Context used to in/export stuff from a DB.
  /// </summary>
  public abstract partial class SqlContext : ModelIOContext {
    readonly Dictionary<string, int> _normalizedTableNames = new();
    readonly OrderedDictionary<string, Table> _tablesByName = new();
    readonly Dictionary<System.Type, string> _tableNamesByModelType = new();
    readonly OrderedDictionary<System.Type, Metadata.Model> _models = new();
    IReadOnlyDictionary<Type, Opperation>? _operationTypes = null;


    Dictionary<System.Type, Exception> _uninitializedModelTypes = new();

    public const string IdColumnName = "Id";

    public const string ModelTypeColumnName = "_model";

    public const string ArchetypeFactoryColumnName = "_type";

    public const string ComponentsColumnName = "_components";

    public const string JsonDataColumnName = "_data";

    /// <summary>
    /// The options passed in for this SQL context
    /// </summary>
    public Settings Options {
      get;
    }

    /// <summary>
    /// The type used for the initial operation builder
    /// </summary>
    public virtual Query.Builder.SelectOpperation.Type SelectOpperationBuilderType
      => Archetypes<Query.Builder.SelectOpperation.Type>.Instance;

    /// <summary>
    /// The type used for the initial operation builder
    /// </summary>
    public virtual Query.Builder.JoinOpperation.Type JoinOpperationBuilderType
      => Archetypes<Query.Builder.JoinOpperation.Type>.Instance;

    /// <summary>
    /// The type used for the initial operation builder
    /// </summary>
    public virtual Query.Builder.UpdateOpperation.Type UpdateOpperationBuilderType
      => Archetypes<Query.Builder.UpdateOpperation.Type>.Instance;

    /// <summary>
    /// The type used for the initial operation builder
    /// </summary>
    public virtual Query.Builder.SelectClauseOpperation.Type SelectClauseOpperationBuilderType
      => Archetypes<Query.Builder.SelectClauseOpperation.Type>.Instance;

    /// <summary>
    /// The type used for the initial operation builder
    /// </summary>
    public virtual Query.Builder.ClauseOpperation.Type ClauseOpperationBuilderType
      => Archetypes<Query.Builder.ClauseOpperation.Type>.Instance;

    /// <summary>
    /// The type used for the initial operation builder
    /// </summary>
    public virtual Query.Builder.ClosedOpperation.Type ClosedOpperationBuilderType
      => Archetypes<Query.Builder.ClosedOpperation.Type>.Instance;

    /// <summary>
    /// The drop table command
    /// </summary>
    protected virtual string DropTableCommandText { get; } = "DROP TABLE";

    /// <summary>
    /// The archetype to use for the select function.
    /// </summary>
    protected abstract Opperation.Initial.SELECT SelectArchetype { get; }

    /// <summary>
    /// The archetype to use for the inset function.
    /// </summary>
    protected abstract Opperation.Initial.INSERT InsertArchetype { get; }

    /// <summary>
    /// The archetype to use for the upsert function.
    /// </summary>
    protected abstract Opperation.Initial.UPSERT UpsertArchetype { get; }

    /// <summary>
    /// The archetype to use for the update function.
    /// </summary>
    protected abstract Opperation.Initial.UPDATE UpdateArchetype { get; }

    /// <summary>
    /// Opperations.
    /// This should at least contain one key for each built in opperation type that needs to be overloaded.
    /// </summary>
    public virtual IReadOnlyDictionary<System.Type, Query.Opperation> OperationTypes
      => _operationTypes ??= new Dictionary<Type, Opperation>() {
        {typeof(Opperation.Initial.SELECT), SelectArchetype},
        {typeof(Opperation.Initial.INSERT), InsertArchetype},
        {typeof(Opperation.Initial.UPDATE), UpdateArchetype},
        {typeof(Opperation.Initial.DELETE), Builder.Token.Types.Get<Opperation.Initial.DELETE>()},
        {typeof(Opperation.Secondary.JOIN), Builder.Token.Types.Get<Opperation.Secondary.JOIN>()},
        {typeof(Opperation.Clause.WHERE), Builder.Token.Types.Get<Opperation.Clause.WHERE>()},
        {typeof(Opperation.Clause.ORDER_BY), Builder.Token.Types.Get<Opperation.Clause.ORDER_BY>()},
        {typeof(Opperation.Initial.UPSERT), UpsertArchetype}
      };

    /// <summary>
    /// Make a new SQL Context
    /// </summary>
    /// <param name="options"></param>
    public SqlContext(Settings options) {
      Options = options;
    }

    #region Table and Model Access

    /// <summary>
    /// The models created by this context
    /// </summary>
    public IReadOnlyDictionary<System.Type, Metadata.Model> Models
      => _models;

    /// <summary>
    /// The tables created by this context
    /// </summary>
    public IReadOnlyDictionary<string, Table> Tables
      => _tablesByName;

    /// <summary>
    /// The tables created by this context
    /// </summary>
    public IReadOnlyDictionary<System.Type, string> TableNames
      => _tableNamesByModelType;

    /// <summary>
    /// Get a table by it's model type.
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public Table GetTable(System.Type modelType)
      => GetTable(TableNames[modelType]);

    /// <summary>
    /// Get the table by name, case insensitive.
    /// </summary>
    public Table GetTable(string tableName)
      => _tablesByName.TryGetValue(tableName, out var found)
        ? found
        : _normalizedTableNames.TryGetValue(tableName.ToLower(), out var index)
          ? _tablesByName[index]
          : throw new KeyNotFoundException($"Table with the name: {tableName}, not found in Sql Context");

    /// <summary>
    /// try to get the table by model type
    /// </summary>
    public bool TryToGetTable(System.Type tableType, out Table? table)
      => (TableNames.TryToGet(tableType) is string tableName && TryToGetTable(tableName, out table))
        || (table = null) != null;

    /// <summary>
    /// try to get the table by name, case insensitive.
    /// </summary>
    public bool TryToGetTable(string tableName, out Table? table)
      => (table =
        _tablesByName.TryGetValue(tableName, out var found)
          ? found
          : _normalizedTableNames.TryGetValue(tableName.ToLower(), out var index)
            ? _tablesByName[index]
            : null)
      != null;

    #endregion

    #region Opperations

    #region SELECT

    #region Basic

    /// <summary>
    /// Select from a table without model specific logic applied.
    /// </summary>
    public Query.Builder.SelectOpperation Select(string tableName, IEnumerable<string>? columnNames = null, IEnumerable<string>? jsonPropertyNames = null, Metadata.Model? model = null)
      => BuildSelectQuery(
        (Query.Opperation.Initial)OperationTypes[typeof(Query.Opperation.Initial.SELECT)],
        model,
        tableName, columnNames?.ToList(), jsonPropertyNames?.ToList()
      );

    /// <summary>
    /// Select from a table without model specific logic applied.
    /// </summary>
    public Query.Builder.SelectOpperation Select(Table table, IEnumerable<string>? columnNames = null, IEnumerable<string>? jsonPropertyNames = null, Metadata.Model? model = null)
      => Select(table.Name, columnNames, jsonPropertyNames, model);

    /// <summary>
    /// Select from a table without model specific logic applied.
    /// </summary>
    public Query.Builder.SelectOpperation Select(Table table, System.Type model, IEnumerable<string>? columnNames = null, IEnumerable<string>? jsonPropertyNames = null)
      => Select(table.Name, columnNames, jsonPropertyNames, Models[model]);

    /// <summary>
    /// Select from a table with model specific logic applied.
    /// </summary>
    public Query.Builder.SelectOpperation Select(Metadata.Model model, IEnumerable<string>? columnNames, IEnumerable<string>? jsonPropertyNames)
      => Select(model.Table.Name, columnNames, jsonPropertyNames, model);

    /// <summary>
    /// Select from a table with model specific logic applied.
    /// </summary>
    public Query.Builder.SelectOpperation Select(Metadata.Model model)
      => Select(model, columnNames: null, jsonPropertyNames: null);

    /// <summary>
    /// Select from a table with model specific logic applied.
    /// </summary>
    public Query.Builder.SelectOpperation Select(System.Type modelType, IEnumerable<string>? columnNames, IEnumerable<string>? jsonPropertyNames)
      => Select(Models[modelType], columnNames, jsonPropertyNames);

    /// <summary>
    /// Select from a table with model specific logic applied.
    /// </summary>
    public Query.Builder.SelectOpperation Select(System.Type modelType)
      => Select(Models[modelType], columnNames: null, jsonPropertyNames: null);

    /// <summary>
    /// Select from a table with model specific logic applied.
    /// </summary>
    public Query.Builder.SelectOpperation Select<TModel>(IEnumerable<string>? columnNames, IEnumerable<string>? jsonPropertyNames)
      => Select(typeof(TModel), columnNames, jsonPropertyNames);

    /// <summary>
    /// Select from a table with model specific logic applied.
    /// </summary>
    public Query.Builder.SelectOpperation Select<TModel>()
      => Select(typeof(TModel), columnNames: null, jsonPropertyNames: null);

    /// <summary>
    /// Select a model from a table by Id.
    /// </summary>
    public Query.Result TryToSelect<TModel>(string modelId)
      where TModel : IUnique
        => Select<TModel>()
          .Where<ColumnEquals>(IdColumnName, modelId)
            .Build()
            .Execute();

    /// <summary>
    /// Select a model from a table by Id.
    /// </summary>
    public Query.Result TryToSelect(System.Type modelType, string modelId)
      => Select(modelType)
        .Where<ColumnEquals>(IdColumnName, modelId)
          .Build()
          .Execute();

    #endregion

    #region IUnique

    /// <summary>
    /// Select a model from a table by Id.
    /// </summary>
    public TModel Select<TModel>(string modelId)
      where TModel : class, IUnique
        => TryToSelect<TModel>(modelId)
          .Rows
          .First()
            .ToModel<TModel>();

    /// <summary>
    /// Select a model from a table by Id.
    /// </summary>
    public IUnique Select(System.Type modelType, string modelId)
        => (IUnique)
        TryToSelect(modelType, modelId)
          .Rows
          .First()
            .ToModel();

    #endregion

    #endregion

    #region INSERT

    #region Basic

    /// <summary>
    /// Insert data into a table
    /// </summary>
    public Query.Builder.ClosedOpperation Insert(string tableName, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null, Metadata.Model? model = null)
      => BuildClosedQuery(
        (Query.Opperation.Initial)OperationTypes[typeof(Query.Opperation.Initial.INSERT)],
        null,
        model,
        tableName, columnNames?.ToList(), columnValues?.ToList(), jsonPropertyNames?.ToList(), jsonPropertyValues?.ToList()
      );

    /// <summary>
    /// Insert data into a table
    /// </summary>
    public Query.Builder.ClosedOpperation Insert(Table table, IEnumerable<Column> columns, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null, Metadata.Model? model = null)
      => Insert(table.Name, columns.Select(c => c.Name), columnValues, jsonPropertyNames, jsonPropertyValues, model);

    /// <summary>
    /// Insert data into a table
    /// </summary>
    public Query.Builder.ClosedOpperation Insert(Table table, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null, Metadata.Model? model = null)
      => Insert(table.Name, columnNames, columnValues, jsonPropertyNames, jsonPropertyValues, model);

    /// <summary>
    /// Insert data into a table
    /// </summary>
    public Query.Builder.ClosedOpperation Insert(Table table, System.Type modelType, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null)
      => Insert(table.Name, columnNames, columnValues, jsonPropertyNames, jsonPropertyValues, Models[modelType]);

    /// <summary>
    /// Insert data into a table
    /// </summary>
    public Query.Builder.ClosedOpperation Insert(Metadata.Model model, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null)
      => Insert(model.Table.Name, columnNames, columnValues, jsonPropertyNames, jsonPropertyValues, model);

    /// <summary>
    /// Insert data into a table
    /// </summary>
    public Query.Builder.ClosedOpperation Insert(System.Type modelType, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null)
      => Insert(Models[modelType], columnNames, columnValues, jsonPropertyNames, jsonPropertyValues);

    /// <summary>
    /// Insert data into a table
    /// </summary>
    public Query.Builder.ClosedOpperation Insert<TModel>(IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null)
      => Insert(typeof(TModel), columnNames, columnValues, jsonPropertyNames, jsonPropertyValues);

    #endregion

    #region IUnique

    /// <summary>
    /// Insert a model fully into a table
    /// </summary>
    public Query.Result Insert<TModel>(TModel model) where TModel : IUnique {
      return _insertModel(model);
    }

    /// <summary>
    /// Insert a model fully into a table
    /// </summary>
    public Query.Result Insert(System.Type modelType, IUnique model) {
      return typeof(IUnique).IsAssignableFrom(modelType)
        ? _insertModel(model)
        : throw new InvalidCastException($"Cannot cast {modelType} to IUnique");
    }

    Query.Result _insertModel(IUnique model) {
      var modelData = Models[model.GetType()];
      var columnNames = new List<string>();
      var columnValues = new List<object>();
      var jsonNames = new List<string>();
      var jsonValues = new List<object>();

      foreach (var column in modelData.Table) {
        columnNames.Add(column.Name);
        columnValues.Add(column.Get(model));
      }

      foreach (var column in modelData.JsonDataProperties) {
        jsonNames.Add(column.Name);
        jsonValues.Add(column.Get(model));
      }

      return Insert(modelData.Table.Name, columnNames, columnValues, jsonNames, jsonValues, modelData)
        .Build()
        .Execute();
    }

    #endregion

    #endregion

    #region UPDATE

    #region Basic

    /// <summary>
    /// Update existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Update(string tableName, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null, Metadata.Model? model = null)
      => BuildUpdateQuery(
        (Query.Opperation.Initial)OperationTypes[typeof(Query.Opperation.Initial.UPDATE)],
        null,
        model,
        tableName, columnNames?.ToList(), columnValues?.ToList(), jsonPropertyNames?.ToList(), jsonPropertyValues?.ToList()
      );

    /// <summary>
    /// Update existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Update(Table table, IEnumerable<Column> columns, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null, Metadata.Model? model = null)
      => Update(table.Name, columns.Select(c => c.Name), columnValues, jsonPropertyNames, jsonPropertyValues, model);

    /// <summary>
    /// Update existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Update(Table table, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null, Metadata.Model? model = null)
      => Update(table.Name, columnNames, columnValues, jsonPropertyNames, jsonPropertyValues, model);

    /// <summary>
    /// Update existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Update(Table table, System.Type modelType, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null)
      => Update(table.Name, columnNames, columnValues, jsonPropertyNames, jsonPropertyValues, Models[modelType]);

    /// <summary>
    /// Update existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Update(Metadata.Model model, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null)
      => Update(model.Table.Name, columnNames, columnValues, jsonPropertyNames, jsonPropertyValues, model);

    /// <summary>
    /// Update existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Update(System.Type modelType, IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null)
      => Update(Models[modelType], columnNames, columnValues, jsonPropertyNames, jsonPropertyValues);

    /// <summary>
    /// Update existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Update<TModel>(IEnumerable<string> columnNames, IEnumerable<object> columnValues, IEnumerable<string>? jsonPropertyNames = null, IEnumerable<object?>? jsonPropertyValues = null)
      => Update(typeof(TModel), columnNames, columnValues, jsonPropertyNames, jsonPropertyValues);

    #endregion

    #region IUnique

    /// <summary>
    /// Update a model fully in a table
    /// </summary>
    public Query.Result Update<TModel>(TModel model) where TModel : IUnique {
      return _updateModel(model);
    }

    /// <summary>
    /// Update a model fully in a table
    /// </summary>
    public Query.Result Update(System.Type modelType, IUnique model) {
      return typeof(IUnique).IsAssignableFrom(modelType)
        ? _updateModel(model)
        : throw new InvalidCastException($"Cannot cast {modelType} to IUnique");
    }

    Query.Result _updateModel(IUnique model) {
      var modelData = Models[model.GetType()];
      var columnNames = new List<string>();
      var columnValues = new List<object>();
      var jsonNames = new List<string>();
      var jsonValues = new List<object>();

      foreach (var column in modelData.Table) {
        if (column.Name != IdColumnName) {
          columnNames.Add(column.Name);
          columnValues.Add(column.Get(model));
        }
      }

      foreach (var column in modelData.JsonDataProperties) {
        jsonNames.Add(BuildJsonDataPropertySelectColumn(column));
        jsonValues.Add(column.Get(model));
      }

      return Update(modelData.Table.Name, columnNames, columnValues, jsonNames, jsonValues, modelData)
        .WhereColumnEquals(IdColumnName, model.Id)
        .Build()
        .Execute();
    }

    #endregion

    #endregion

    #region DELETE

    #region Basic

    /// <summary>
    /// Delete existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Delete(string tableName, Metadata.Model? model = null)
      => BuildUpdateQuery(
        (Query.Opperation.Initial)OperationTypes[typeof(Query.Opperation.Initial.DELETE)],
        null,
        model,
        tableName
      );

    /// <summary>
    /// Delete existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Delete(Table table, Metadata.Model? model = null)
      => Delete(table.Name, model);

    /// <summary>
    /// Delete existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Delete(Table table, System.Type modelType)
      => Delete(table.Name, Models[modelType]);

    /// <summary>
    /// Delete existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Delete(Metadata.Model model)
      => Delete(model.Table.Name, model);

    /// <summary>
    /// Delete existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Delete(System.Type modelType)
      => Delete(Models[modelType]);

    /// <summary>
    /// Delete existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Delete<TModel>()
      => Delete(typeof(TModel));

    #endregion

    #region IUnique

    /// <summary>
    /// Delete a model fully in a table
    /// </summary>
    public Query.Result Delete<TModel>(TModel model)
      where TModel : IUnique
        => _deleteModel(model, typeof(TModel));

    /// <summary>
    /// Delete a model fully in a table
    /// </summary>
    public Query.Result Delete(System.Type modelType, IUnique model)
      => typeof(IUnique).IsAssignableFrom(modelType)
        ? _deleteModel(model, modelType)
        : throw new InvalidCastException($"Cannot cast {modelType} to IUnique");

    Query.Result _deleteModel(IUnique model, Type modelType)
      => Delete(Models[modelType])
        .WhereColumnEquals(IdColumnName, model.Id)
        .Build()
        .Execute();

    #endregion

    #endregion

    #region UPSERT

    #region Basic

    /// <summary>
    /// Upsert existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Upsert(string tableName, IEnumerable<string> columnNames, IEnumerable<object> columnValues, Metadata.Model? model = null)
      => BuildUpdateQuery(
        (Query.Opperation.Initial)OperationTypes[typeof(Query.Opperation.Initial.UPSERT)],
        null,
        model,
        tableName, columnNames?.ToList(), columnValues?.ToList()
      );

    /// <summary>
    /// Upsert existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Upsert(Table table, IEnumerable<Column> columns, IEnumerable<object> columnValues, Metadata.Model? model = null)
      => Upsert(table.Name, columns.Select(c => c.Name), columnValues, model);

    /// <summary>
    /// Upsert existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Upsert(Table table, IEnumerable<string> columnNames, IEnumerable<object> columnValues, Metadata.Model? model = null)
      => Upsert(table.Name, columnNames, columnValues, model);

    /// <summary>
    /// Upsert existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Upsert(Table table, System.Type modelType, IEnumerable<string> columnNames, IEnumerable<object> columnValues)
      => Upsert(table.Name, columnNames, columnValues, Models[modelType]);

    /// <summary>
    /// Upsert existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Upsert(Metadata.Model model, IEnumerable<string> columnNames, IEnumerable<object> columnValues)
      => Upsert(model.Table.Name, columnNames, columnValues, model);

    /// <summary>
    /// Upsert existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Upsert(System.Type modelType, IEnumerable<string> columnNames, IEnumerable<object> columnValues)
      => Upsert(Models[modelType], columnNames, columnValues);

    /// <summary>
    /// Upsert existing data in a table
    /// </summary>
    public Query.Builder.UpdateOpperation Upsert<TModel>(IEnumerable<string> columnNames, IEnumerable<object> columnValues)
      => Upsert(typeof(TModel), columnNames, columnValues);

    #endregion

    #region IUnique

    /// <summary>
    /// Upsert a model fully in a table
    /// </summary>
    public Query.Result Upsert<TModel>(TModel model) where TModel : IUnique {
      Type modelType = typeof(TModel);
      return _upsertModel(model);
    }

    /// <summary>
    /// Upsert a model fully in a table
    /// </summary>
    public Query.Result Upsert(System.Type modelType, IUnique model) {
      return typeof(IUnique).IsAssignableFrom(modelType)
        ? _upsertModel(model)
        : throw new InvalidCastException($"Cannot cast {modelType} to IUnique");
    }

    Query.Result _upsertModel(IUnique model) {
      var modelData = Models[model.GetType()];
      var columnNames = new List<string>();
      var columnValues = new List<object>();

      foreach (var column in modelData.Table) {
        if (column.Name != IdColumnName) {
          columnNames.Add(column.Name);
          columnValues.Add(column.Get(model));
        }
      }

      foreach (var column in modelData.JsonDataProperties) {
        columnNames.Add(BuildJsonDataPropertySelectColumn(column));
        columnValues.Add(column.Get(model));
      }

      return Upsert(modelData.Table.Name, columnNames, columnValues, modelData)
        .WhereColumnEquals(IdColumnName, model.Id)
        .Build()
        .Execute();
    }

    #endregion

    #endregion

    #endregion

    #region Query Building

    /// <summary>
    /// Helper to make a builder from an initial opperation
    /// </summary>
    protected internal Query.Builder.SelectOpperation BuildSelectQuery(Query.Opperation.Initial opperation, Metadata.Model? model = null, params object?[] parameters)
      => SelectOpperationBuilderType.Make(
        this,
        CreateTokenFor(opperation, null, model, parameters)
      );

    /// <summary>
    /// Helper to make a builder from an initial opperation
    /// </summary>
    protected internal Query.Builder.JoinOpperation BuildJoinQuery(Query.Opperation.Secondary opperationType, Query.Builder.ICanJoin previousOpperation, Metadata.Model? model = null, params object?[] parameters)
      => JoinOpperationBuilderType.Make(
        previousOpperation,
        CreateTokenFor(opperationType, previousOpperation.Tokens.Last(), model, parameters)
      );

    /// <summary>
    /// Helper to make a builder from an initial opperation
    /// </summary>
    protected internal Query.Builder.UpdateOpperation BuildUpdateQuery(Query.Opperation.Initial opperationType, Metadata.Model? model = null, params object?[] parameters)
      => UpdateOpperationBuilderType.Make(
        CreateTokenFor(opperationType, null, model, parameters),
        this
      );

    /// <summary>
    /// Helper to make a builder from an initial opperation
    /// </summary>
    protected internal Query.Builder.SelectClauseOpperation BuildSelectClauseQuery(Query.Opperation.Clause opperationType, Query.IBuilder previousOpperation, Metadata.Model? model = null, params object?[] parameters)
      => SelectClauseOpperationBuilderType.Make(
        previousOpperation,
        CreateTokenFor(opperationType, previousOpperation.Tokens.Last(), model, parameters)
      );

    /// <summary>
    /// Helper to make a builder from an initial opperation
    /// </summary>
    protected internal Query.Builder.ClauseOpperation BuildClauseQuery(Query.Opperation.Clause opperationType, Query.IBuilder previousOpperation, Metadata.Model? model = null, params object?[] parameters)
      => ClauseOpperationBuilderType.Make(
        previousOpperation,
        CreateTokenFor(opperationType, previousOpperation.Tokens.Last(), model, parameters)
      );

    /// <summary>
    /// Helper to make a builder from an initial opperation
    /// </summary>
    protected internal Query.Builder.ClosedOpperation BuildClosedQuery(Query.Opperation opperationType, Query.Builder? previousOpperation = null, Metadata.Model? model = null, params object?[] parameters)
      => ClosedOpperationBuilderType.Make(
        CreateTokenFor(opperationType, previousOpperation?.Tokens.Last(), model, parameters),
        previousOpperation,
        this
      );

    /// <summary>
    /// Helper to make a token
    /// </summary>
    internal protected Query.Builder.Token CreateTokenFor(Query.Opperation opperation, Query.Builder.Token? previousToken, Metadata.Model? model, params object?[] parameters)
      => opperation.Make(model, previousToken, parameters);

    /// <summary>
    /// Can be overriden to sort clauses if some need to be at the end.
    /// </summary>
    protected internal virtual IEnumerable<Query.Builder.Token> SortClauseTokens(IReadOnlyList<Query.Builder.Token> tokens)
      => tokens.OrderBy(t => t.Archetype.Id.Key);

    /// <summary>
    /// Used to serialize values before passing them to the query(if they have the step)
    /// This also preps placeholders.
    /// </summary>
    protected internal object? SerializeQueryValueViaModel(Builder.Token token, string columnName, object? rawValue) {
      if (token.Model.Table.TryToGetColumn(columnName, out var column)) {
        if (rawValue is Placeholder p) {
          p.Serializer = token.Model.Table[columnName].Serializer;
          return p;
        }
        else return token.Model.Table[columnName].Serializer(rawValue);
      }
      else if (token.Model.TryToGetJsonDataProperty(columnName, out var jsonProperty)) {
        if (rawValue is Placeholder p) {
          p.Serializer = jsonProperty.Serializer;
          return p;
        }
        else return jsonProperty.Serializer(rawValue);
      }
      else return rawValue;
    }

    /// <summary>
    /// Used to get the query replacement key.
    /// </summary>
    protected internal virtual string GetValueReplacementKey(int index, bool withPrefix = true)
      => withPrefix ? $":_{index}" : $"_{index}";

    protected internal string BuildJsonDataPropertySelectColumn(JsonDataProperty column)
      => BuildJsonDataPropertySelectColumn(column.Name);

    protected internal virtual string BuildJsonDataPropertySelectColumn(string propertyName, bool withAs = false)
      => JsonDataColumnName + "." + propertyName + (withAs ? (" AS " + propertyName) : "");

    /// <summary>
    /// Used to build the table creation query.
    /// </summary>
    protected virtual string BuildTableCreationQuery(Table table, bool checkExists = true)
      => "CREATE TABLE" + (checkExists ? " IF NOT EXISTS " : " ") + table.Name + " ("
        + BuildQueryTableColumns(table)
        + BuildJsonDataColumn()
      + "\n);\n";

    protected virtual string BuildQueryTableColumns(Table table)
      => string.Join(",", table.Select(c => BuildColumnCreationForTableCreationQuery(c)));

    protected virtual string BuildColumnCreationForTableCreationQuery(Column? c, string? name = null, string? dataType = null)
      => $"\n\t{name ?? c.Name} {dataType ?? c.DataType}";

    protected virtual string BuildJsonDataColumn()
      => "," + BuildColumnCreationForTableCreationQuery(null, JsonDataColumnName, "jsonb");

    /// <summary>
    /// Execute the query to build all tables.
    /// </summary>
    protected void BuildAllTables(bool checkExists = true)
      => _tablesByName.Values.ForEach(t => {
        Query.Result result;
        if (!(result = ExecuteQuery(BuildTableCreationQuery(t, checkExists), null)).Success) {
          throw result.Error ?? throw new Exception($"Could not build table of type: {t.Name}, due to unknown failure");
        }
      });

    /// <summary>
    /// Can be used to drop a table.
    /// </summary>
    protected Query.Result DropTable(Table table, bool checkExists = true)
      => ExecuteQuery($"{DropTableCommandText}{(checkExists ? " IF EXISTS " : " ")}{table.Name};", null);

    #endregion

    #region Query Execution

    /// <summary>
    /// A wrapper for executing an already compiled query with potential placeholder parameters.
    /// </summary>
    public Query.Result Execute(Query query, IReadOnlyList<object>? parameters = null) {
      var compiledQuery = query._build();
      IReadOnlyList<object> compiledParameters = parameters is not null && parameters.Any()
        ? ReplaceQueryParameterPlaceholders(parameters, compiledQuery.parameters)
        : compiledQuery.parameters;

      return ExecuteQuery(compiledQuery.text, compiledParameters);
    }

    /// <summary>
    /// A wrapper for executing a query with debug logging added.
    /// </summary>
    protected internal Query.Result ExecuteQuery(string query, IReadOnlyList<object>? parameters, Query? builderContext = null) {

#if DEBUG
      if (Universe.HasExtraContext<Meep.Tech.XBam.Configuration.ConsoleProgressLogger>()) {
        var logger = Universe.GetExtraContext<Meep.Tech.XBam.Configuration.ConsoleProgressLogger>();
        logger.WriteMessage($"Executing Query!", nameof(SqlContext), verboseNonErrorText: query);
        if ((parameters?.Any() ?? false) && logger.VerboseModeForNonErrors) {
          logger.WriteMessage($"Query Params Provided: \n\t - {string.Join("\n\t - ", parameters ?? Enumerable.Empty<object>())}");
        }
      }
#endif

      var results = ExecuteDatabaseQuery(query, parameters);
      /*var token = builderContext?.Tokens.First();
      if (token?.Archetype is Opperation.Initial.SELECT && token.Parameters.Count == 1 && token.Model is not null) {
        token.resu
      }*/
      SetQueryResultsContext(results, builderContext);

#if DEBUG
      if (Universe.HasExtraContext<Meep.Tech.XBam.Configuration.ConsoleProgressLogger>()) {
        var logger = Universe.GetExtraContext<Meep.Tech.XBam.Configuration.ConsoleProgressLogger>();
        logger.WriteMessage(
          $"Query Executed {(results.Success ? "Successfully" : "With Errors!!")}!",
          nameof(SqlContext),
          isError: !results.Success,
          exception: results.Error,
          verboseNonErrorText: (results.Rows?.Any() ?? false)
            ? "Results: \n"
              + string.Join("\n\t", results.Rows.Select(r =>
                string.Join(',', r.ColumnNames.Select(
                  c => r.TryToGetColumnValue(c, out var v) ? v?.ToString() : "null"))))
            : null
        );
      }
#endif

      return results;
    }

    protected IReadOnlyList<object> ReplaceQueryParameterPlaceholders(IReadOnlyList<object> replacementValues, IReadOnlyList<object> compiledParameters) {
      int placeholderIndex = 0;
      List<object> replacementParameters = new();
      foreach (var parameter in compiledParameters) {
        if (parameter is Placeholder placeholder) {
          replacementParameters.Add(placeholder.Replace(replacementValues[placeholderIndex++]));
        }
        else {
          replacementParameters.Add(parameter);
        }
      }

      return placeholderIndex != replacementValues.Count
        ? throw new ArgumentException($"Number of Placeholders in query does not match number of provided parametets")
        : (IReadOnlyList<object>)replacementParameters;
    }

    /// <summary>
    /// Helper function to set query context on results.
    /// </summary>
    protected void SetQueryResultsContext(Result results, Query? builderContext) {
      results.SqlContext = this;
      results.Query = builderContext;
    }

    /// <summary>
    /// The logic to execute the query.
    /// Implement this using MakeRawCell, Success, and Failure.
    /// Use the wrapper method ExecuteQuery for implementations methods instead of this.
    /// </summary>
    /// <seealso cref="MakeRawCell(string, string, object, Type)"/>
    /// <seealso cref="Success(IEnumerable{IEnumerable{RawCellData}})"/>
    /// <seealso cref="Failure(Exception)"/>
    /// <see cref="MakeRawCell(string, string, object, Type)"/>
    /// <see cref="Success(IEnumerable{IEnumerable{RawCellData}})"/>
    /// <see cref="Failure(Exception)"/>
    protected internal abstract Query.Result ExecuteDatabaseQuery(string query, IReadOnlyList<object>? parameters);

    /// <summary>
    /// Used to mark a query as a failure
    /// </summary>
    /// <see cref="ExecuteQuery(string)"/>
    /// <seealso cref="ExecuteQuery(string)"/>
    protected Query.Result Failure(Exception error)
      => new(null, false, error);

    /// <summary>
    /// Used to mark a query as a success and bundle the results.
    /// </summary>
    /// <see cref="ExecuteQuery(string)"/>
    /// <seealso cref="ExecuteQuery(string)"/>
    protected Query.Result Success(IEnumerable<IEnumerable<RawCellData>>? rows)
      => new(rows, true, null);

    /// <summary>
    /// Helper to raw cell data from query results.
    /// </summary>
    /// <see cref="ExecuteQuery(string)"/>
    /// <seealso cref="ExecuteQuery(string)"/>
    protected RawCellData MakeRawCell(string columnName, string sqlDataTypeName, object value, Type expectedValueType)
      => new(columnName, sqlDataTypeName, value, expectedValueType);

    /// <summary>
    /// Used to deserialize raw db values.
    /// Wrapper for DeserializeDatabaseValue that includes value converters.
    /// </summary>
    internal protected object? DeserializeRawDatabaseValueWithConverters(RawCellData cell, System.Type expectedType) {
      //todo: check for value converters first.
      return DefaultDeserializeRawDatabaseValue(cell, expectedType);
    }

    /// <summary>
    /// Used to deserialize raw db values.
    /// </summary>
    protected virtual object? DefaultDeserializeRawDatabaseValue(RawCellData cell, System.Type expectedType) {
      // handle nulls
      if (cell.Value is System.DBNull or null) {
        return null;
      }

      // handle enums
      if (typeof(Enum).IsAssignableFrom(expectedType)) {
        return Enum.Parse(expectedType, cell.Value.ToString());
      }

      // cast other things
      return cell.Value.CastTo(expectedType);
    }

    /// <summary>
    /// Used to deserialize raw db values.
    /// </summary>
    protected internal virtual object? SerializeToRawDatabaseValue(object value, bool isJsonDataField)
      => !isJsonDataField 
        ? value 
        : (value is IModel model 
          ? model.ToJson(SaveDataSerializer).ToString() 
          : JsonConvert.SerializeObject(value, SaveDataSerializerSettings));

    /// <summary>
    /// Used to deserialize the raw sql value in a row.
    /// </summary>
    internal object? _deserializeRawSqlValue(RawCellData cell, Metadata.Model? forModel = null) {
      if (forModel is not null) {
        if (forModel.TryToGetJsonDataProperty(cell.ColumnName, out var jsonPropertyMetadata)) {
          try {
            return jsonPropertyMetadata.Deserializer(cell);
          }
          catch (Exception e) {
            throw new InvalidOperationException($"Could not Deserialize raw SQL Cell value from Json Data Property: {cell.ColumnName}, in Column: {SqlContext.JsonDataColumnName}, for Model: {forModel.SystemType.ToFullHumanReadableNameString()}.", e);
          }
        }
        else if (forModel.Table.TryToGetColumn(cell.ColumnName, out Column? sqlColumn)) {
          try {
            return sqlColumn!.Deserializer(cell);
          } catch (Exception e) {
            throw new InvalidOperationException($"Could not Deserialize raw SQL Cell value from Column: {cell.ColumnName}, for Model: {forModel.SystemType.ToFullHumanReadableNameString()}.", e);
          }
        }
      }

      try {
        return DeserializeRawDatabaseValueWithConverters(cell, cell.ExpectedValueType);
      } catch (Exception e) {
        throw new InvalidOperationException($"Could not Deserialize raw SQL Cell value from Column: {cell.ColumnName}", e);
      }
    }

    internal JToken _jsonSerialize(object data)
      => JToken.FromObject(data, SaveDataSerializer);

    #endregion

    #region Model Mapping

    /// <summary>
    /// Used to get the data type to use in the sql database for the given property. 
    /// </summary>
    protected internal abstract string GetColumnDataType(Column column);

    /// <summary>
    /// Can be used to check if a property can be auto ported at all.
    /// </summary>
    protected bool PropertyCanBeAutoPorted(PropertyInfo property)
      => property.CanWrite && property.CanRead && typeof(IUnique).IsAssignableFrom(property.PropertyType);

    /// <summary>
    /// Can be used to check if a property should be auto ported at all.
    /// </summary>
    protected virtual bool PropertyShouldBeAutoPorted(PropertyInfo property)
      => PropertyCanBeAutoPorted(property) && property.GetCustomAttribute<AutoPortAttribute>() != null;

    /// <summary>
    /// Can be used to check if a property should be auto ported at all.
    /// </summary>
    protected virtual bool PropertyShouldBeAutoPorted(PropertyInfo property, out AutoPortAttribute? autoPortAttributeData) {
      autoPortAttributeData = null;
      return PropertyCanBeAutoPorted(property) && (autoPortAttributeData = property.GetCustomAttribute<AutoPortAttribute>()) != null;
    }

    /// <summary>
    /// Logic to get the default model constructor to use for a given model type.
    /// </summary>
    protected internal virtual Func<Query.Result.Row, object>? GetModelConstructor(Type modelType, bool hasUniverseField) {
      ConstructorInfo[] ctors = modelType
            .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      Func<Query.Result.Row, object>? ctor = null;

      if (typeof(IModel).IsAssignableFrom(modelType)) {
        ctor = ctors
          .Where(c => c.GetParameters().Length == 0 || typeof(IBuilder).IsAssignableFrom(c.GetParameters().First().ParameterType))
            .Select(c => new Func<Query.Result.Row, object>(row => {
              if (c.GetParameters().Any()) {
                var builder = GetBuilder(Archetypes.Id[row[ArchetypeFactoryColumnName] as string].Archetype);
                row.ColumnNames.Select(c => builder.Add(c, row.GetColumnValue(c, Models[modelType])));

                return _setUniverse(c.Invoke(new object[] { builder }), Universe);
              }
              else {
                return _setUniverse(c.Invoke(new object[0]), Universe);
              }
            }))
            .FirstOrDefault();
      }

      return ctor ?? ctors
         .Where(c => c.GetParameters().Length == 0)
          .Select(c => hasUniverseField 
            ? new Func<Query.Result.Row, object>(_ => _setUniverse(c.Invoke(new object[0]), Universe)) 
            : (_ =>  c.Invoke(new object[0]))
          ).FirstOrDefault()
         ?? ctors
          .Where(c => c.GetParameters().First().ParameterType == typeof(Query.Result.Row))
          .Select(c => hasUniverseField
            ? new Func<Query.Result.Row, object>(r =>  _setUniverse(c.Invoke(new object[] { r }), Universe))
            : r => c.Invoke(new object[] { r })
          ).FirstOrDefault();

    }

    /// <summary>
    /// Generate metadata for a table from a class.
    /// </summary>
    internal protected Metadata.Model? GenerateModelMetadata(System.Type modelType) {
      Table table;
      bool hasUniverseField;
      Type modelBaseType = XBam.Models.GetBaseType(modelType);
      List<JsonDataProperty> jsonDataProperties = new();

      /// if the attribute is not mapped, skip it
      if (modelType.HasAttribute<NotMappedAttribute>()) {
        return null;
      }

      // if it's a model base type we register the new table for it.
      if (modelBaseType == modelType) {

        // if it's already been generated, return null
        if (_models.ContainsKey(modelType)) {
          return null;
        }

        var tableAttribute = modelType.GetCustomAttribute<TableAttribute>();

        /// if the attribute is required but not provided, skip.
        if (Options.ModelDiscoveryOptions.HasFlag(Settings.ModelDiscoveryMethod.TableAttributeIsRequired) && tableAttribute is null) {
          return null;
        }

        string tableName = tableAttribute?.Name
          ?? modelType.ToFullHumanReadableNameString(withNamespace: false, includeGenerics: false)
            .Replace(".", "");
        List<Column> columns = new();

        if (_tablesByName.ContainsKey(tableName)) {
          throw new ArgumentException($"A table with the name: {tableName}, already exists for Base Model of type: {_tablesByName[tableName].BaseModel.SystemType.ToFullHumanReadableNameString()}. Cannot create a table with the same name for base model type: {modelType.ToFullHumanReadableNameString()}");
        }

        // make a column for each valid save data property
        GetSaveDataProperties(modelType, out hasUniverseField)
          .ForEach(f  => 
            columns.Add(new Column(f)));

        // Make and register the table
        table = new(tableName, columns);
      }
      else {
        // make sure the base table has been generated
        if(!TryToGetTable(modelBaseType, out table)) {
          table = GenerateModelMetadata(modelBaseType).Table;
        }

        // get all the valid save data columns
        GetSaveDataProperties(modelType, out hasUniverseField)
          // filter out the ones that already have columns in the table.
          .Where(
          f =>
            !table.HasColumn(f.Name))
          .ForEach(f => 
            jsonDataProperties.Add( new JsonDataProperty(f)));
      }

      Func<Result.Row, object> modelConstructor = GetModelConstructor(modelType, hasUniverseField)
        ?? throw new System.MissingMemberException($"Default SqlContext Model Constructor for Type: {modelType.FullName} not found!");

      var model = new Metadata.Model(
        table,
        jsonDataProperties,
        this,
        modelType,
        modelConstructor
      );

      _registerModel(model, table);
      return model;
    }

    protected IEnumerable<Field> GetSaveDataProperties(Type modelType, out bool hasUniverseField) {
      var fields = _getValidSaveDataColumns(modelType).ToList();

      // model type
      fields.Add(new(null, new ColumnAttribute() {TypeName = "text" }, new ModelTypeFieldAttribute()));

      // archetype
      if (typeof(IModel).IsAssignableFrom(modelType)) {
        if (modelType.IsAssignableToGeneric(typeof(IModel<,>))) {
          fields.Add(new(modelType.GetProperty(nameof(Archetype)), new ColumnAttribute() { TypeName = "text"}, new ArchetypeFieldAttribute()));
        } else if (modelType.IsAssignableToGeneric(typeof(IModel<>))) {
          fields.Add(new(modelType.GetProperty(nameof(IModel.Factory)), new ColumnAttribute() { TypeName = "text" }, new ArchetypeFieldAttribute()));
        }
      }

      // id
      if (typeof(IUnique).IsAssignableFrom(modelType)) {
        var idColumn = fields.FirstOrDefault(f => f.Name == IdColumnName);
        if (idColumn is null) {
          var uniqueIdProperty = typeof(IUnique).GetProperty(nameof(IUnique.Id));
          idColumn = new(uniqueIdProperty, null, new SaveDataAttribute() { PropertyNameOverride = IdColumnName });

          fields.Add(idColumn);
        }
      }

      // components
      if (typeof(IReadableComponentStorage).IsAssignableFrom(modelType)) {
        var componentsColumn = fields.FirstOrDefault(c => c.Name == "Components");
        if (componentsColumn.Property is null) {
          var componentsProperty = typeof(IModel.IReadableComponentStorage).GetProperty(nameof(IModel.IReadableComponentStorage.Components));
          componentsColumn = new(componentsProperty, null, new SaveDataAttribute() { PropertyNameOverride = ComponentsColumnName }, "jsonb");
          fields.Add(componentsColumn);
        }
        else {
          componentsColumn.Data = new SaveDataAttribute() { PropertyNameOverride = ComponentsColumnName };
          componentsColumn._dataType = "jsonb";
        }
      }

      // remove the universe field if there is one
      int universeColumnIndex;
      if ((universeColumnIndex = fields.FindIndex(f => f.Name == nameof(IModel.Universe) && f.Property?.PropertyType == typeof(Universe))) > 0) {
        fields.RemoveAt(universeColumnIndex);
        hasUniverseField = true;
      } else {
        hasUniverseField = false;
      }

      return fields.OrderBy(
        e =>
          e.Name switch {
            IdColumnName => -3,
            ModelTypeColumnName => -2,
            ArchetypeFactoryColumnName => -1,
            ComponentsColumnName => 1,
            JsonDataColumnName => 2,
            _ => 0
          }
        );
    }

    IEnumerable<Field> _getValidSaveDataColumns(Type modelType)
      => // collect the properties from columns
        modelType
          // get all the properties
          .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
          // where does not have NotSaveDataAttribute or NotMapped and Can be written to and read from.
          .Where(
            p =>
              !p.HasAttribute<NotSaveDataAttribute>()
                && !p.HasAttribute<NotMappedAttribute>()
                && p.CanWrite
                && p.CanRead
          // get custom attribute datas
          ).Select(
            p => new Field(
              property: p,
              attribute: p.GetCustomAttribute<ColumnAttribute>() ?? null,
              data: p.GetCustomAttribute<SaveDataAttribute>()
            )
          // where the getter is public or it has a custom attribute
          ).Where(
            e =>
              e.Property.GetMethod.IsPublic
                || e.Attribute is not null
                || e.Data is not null
          );

    /// <summary>
    /// Try to directly deserialize a model to it's class using the given table metadata.
    /// </summary>
    internal object _deserializeToModelUsingMetadata(Query.Result.Row row, Metadata.Model modelData) {
      object model = modelData._modelConstructor(row);

      foreach (Column column in modelData.Table) {
        if (row.TryToGetColumnValue(column.Name, modelData, out var value)) {
          try {
            column.Set(model, value);
          } catch (Exception e) {
            throw new InvalidOperationException($"Could not set value from Column: {column.Name}, on Model: {modelData.SystemType.ToFullHumanReadableNameString()}, due to an internal exception.", e);
          }
        }
      }

      foreach (JsonDataProperty jsonProperty in modelData.JsonDataProperties) {
        if (row.TryToGetColumnValue(jsonProperty.Name, modelData, out var value)) {
          try {
            jsonProperty.Set(model, value);
          } catch (Exception e) {
            throw new InvalidOperationException($"Could not set value from Json Property: {jsonProperty.Name}, from Column: {SqlContext.JsonDataColumnName}, on Model: {modelData.SystemType.ToFullHumanReadableNameString()}, due to an internal exception.", e);
          }
        }
      }

      return model;
    }

    void _registerModel(Metadata.Model model, Table table) {
      _models[model.SystemType] = model;

      if (!_tablesByName.ContainsKey(table.Name)) {
        _tablesByName.Add(table.Name, table);
        _normalizedTableNames.Add(table.Name.ToLower(), _tablesByName.Count - 1);
      }

      _tableNamesByModelType.Add(model.SystemType, table.Name);
    }

    // TODO: make this more efficient potentially by caching the property per type?
    object _setUniverse(object onObject, Universe universe) {
      onObject
        .GetType()
        .GetProperty(nameof(Universe))
          .Set(onObject, universe);

      return onObject;
    }

    #endregion

    #region Universe Context Overrides 

    protected override bool TryToFetchModelByTypeAndId(Type type, string uniqueId, out IUnique? model, out Exception? error) {
      var result = TryToSelect(type, uniqueId);
      if (!result.Success) {
        model = null;
        error = result.Error;

        return false;
      }

      model = (IUnique)result.Rows.First().ToModel();
      bool @return = model != null;
      error = @return ? null : new KeyNotFoundException();
      return @return;
    }

    protected override Action<Universe> OnLoaderInitializationComplete
      => universe => {
        base.OnLoaderInitializationComplete(universe);
        universe.SetExtraContext<SqlContext>(this);
        universe.Loader.Options.PreLoadAssemblies.Insert(0, GetType().Assembly);
        universe.Loader.Options.PreLoadAssemblies.Insert(0, typeof(SqlContext).Assembly);

        // try to load all non xbam model types first.
        if (Options.ModelDiscoveryOptions.HasFlag(Settings.ModelDiscoveryMethod.IncludedListClasses)) {
          Options.IncludedModelTypes
            .ForEach(type => {
              try {
                GenerateModelMetadata(type);
              }
              catch (Exception e) {
                _uninitializedModelTypes.Add(type, e);
              }
            });
        }
      };

    protected override Action<IEnumerable<Assembly>> OnLoaderAssembliesCollected
      => assemblies => {
        if (Options.ModelDiscoveryOptions.HasFlag(Settings.ModelDiscoveryMethod.TableAttributeOnlyClasses)) {
          assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t.TryToGetAttribute<TableAttribute>(out _) && !typeof(IModel).IsAssignableFrom(t) && !Options.IncludedModelTypes.Contains(t))
            .ForEach(type => {
              try {
                GenerateModelMetadata(type);
              }
              catch (Exception e) {
                _uninitializedModelTypes.Add(type, e);
              }
            });
        }
      };

    protected override Action<bool, Type, Exception> OnLoaderModelFullInitializationComplete
      => (success, type, error) => {
        if (success) {
          if (Options.ModelDiscoveryOptions.HasFlag(Settings.ModelDiscoveryMethod.XBamModelTypes) || type.GetCustomAttribute<TableAttribute>() != null) {
            try {
              GenerateModelMetadata(type);
            }
            catch (Exception e) {
              _uninitializedModelTypes.Add(type, e);
            }
          }
        }
      };

    protected override Action OnLoaderAllModificationsComplete =>
      () => {
        int remainingRuns = Options.ModelInitializationAttempts;
        while (remainingRuns-- > 0 && _uninitializedModelTypes.Any()) {
          _uninitializedModelTypes.Keys.ForEach(type => {
            try {
              GenerateModelMetadata(type);
              _uninitializedModelTypes.Remove(type);
            }
            catch (Exception e) {
              _uninitializedModelTypes[type] = e;
            }
          });
        }
      };

    protected override Action OnLoaderFinalizeStart
      => () => {
        if (Options.DropAllTablesBeforeCreatingThem) {
          _tablesByName.Values.ForEach(t => {
            Query.Result result;
            if (!(result = DropTable(t)).Success) {
              throw result.Error ?? throw new Exception($"Could not build table of type: {t.Name}, due to unknown failure");
            }
          });
        }

        BuildAllTables(true);
      };

    #endregion
  }

  internal class ModelTypeFieldAttribute : SaveDataAttribute {

    public override string PropertyNameOverride
      => SqlContext.ModelTypeColumnName;

    public override Getter GetGetterOverride(PropertyInfo property, Universe universe) 
      => m => m.GetType().ToFullHumanReadableNameString();

    public override Setter GetSetterOverride(PropertyInfo property, Universe universe)
      => (_, _) => { };
  }

  internal class ArchetypeFieldAttribute : SaveDataAttribute {

    public override string PropertyNameOverride
      => SqlContext.ArchetypeFactoryColumnName;

    public override Func<object, object> DeserializerFromRawOverride(Universe universe) => raw => {
      var id = ((SqlContext.RawCellData)raw).Value.CastTo<string>();
      return universe.Archetypes.Id[id].Archetype;
    };

    public override Func<object, object> SerializerToRawOverride(Universe universe)
      => raw => ((Archetype)raw).Id.Key;
  }
}