using System;
using System.Collections.Generic;

namespace Meep.Tech.XBam.IO.Sql {

  public abstract partial class SqlContext {
    public class Settings {

      /// <summary>
      /// How the context finds classes to use as table/model sources.
      /// </summary>
      public enum ModelDiscoveryMethod {
        /// <summary>
        /// Include IModel types from XBam
        /// </summary>
        XBamModelTypes = 1,
        /// <summary>
        /// Include classes that only use the [TableAttribute]
        /// </summary>
        TableAttributeOnlyClasses = 2,
        /// <summary>
        /// Include classes in the "IncludedModelTypes" list.
        /// </summary>
        IncludedListClasses = 4,
        /// <summary>
        /// If this is included, only classes in the sources that also have the [TableAttribute] are included.
        /// </summary>
        TableAttributeIsRequired = 8
      }

      /// <summary>
      /// How this finds classes to use as table/model sources.
      /// </summary>
      public ModelDiscoveryMethod ModelDiscoveryOptions { get; }

      /// <summary>
      /// The included model types list.
      /// </summary>
      public HashSet<System.Type> IncludedModelTypes {
        get;
        private set;
      } = new();

      /// <summary>
      /// How many loops should be run to try to initialize all models.
      /// This helps with dependency trees.
      /// </summary>
      public int ModelInitializationAttempts {
        get;
        init;
      } = 10;

      /// <summary>
      /// If all tables should be dropped before creating them again.
      /// This is useful for testing
      /// </summary>
      public bool DropAllTablesBeforeCreatingThem {
        get;
        init;
      } = false;

      /// <summary>
      /// Make a new set of SqlContext Settings
      /// </summary>
      /// <param name="xBamModelsMustOptInWithTableAttribute"></param>
      public Settings(ModelDiscoveryMethod modelDiscoveryMethod = ModelDiscoveryMethod.XBamModelTypes | ModelDiscoveryMethod.TableAttributeOnlyClasses | ModelDiscoveryMethod.IncludedListClasses, HashSet<System.Type>? includedModelTypes = null) {
        ModelDiscoveryOptions = modelDiscoveryMethod;
        IncludedModelTypes = includedModelTypes ?? IncludedModelTypes ?? new();
      }
    }
  }
}