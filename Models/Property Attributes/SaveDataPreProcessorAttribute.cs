using System;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.XBam.IO.Sql {
  /// <summary>
  /// Used to add a pre processor to a data field for when the data is about to be saved to the DB.
  /// </summary>
  public class SaveDataPreProcessorAttribute
    : Attribute {
    internal PreProcessor _preProcessor;

    public delegate object PreProcessor(object originalObject);

    /// <summary>
    /// The member name of the PreProcessor delegate property or method.
    /// </summary>
    public string PreProcessorMemberName {
      get;
    }

    /// <summary>
    /// Mark a Data Property as having a Pre Processor function.
    /// </summary>
    /// <param name="PreProcessorMemberName"></param>
    public SaveDataPreProcessorAttribute([NotNull] string PreProcessorMemberName) {
      this.PreProcessorMemberName = PreProcessorMemberName
        ?? throw new ArgumentNullException(nameof(PreProcessorMemberName));
    }
  }
}
