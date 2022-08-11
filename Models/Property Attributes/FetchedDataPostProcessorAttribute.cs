using System;
using System.Diagnostics.CodeAnalysis;

namespace Meep.Tech.XBam.IO.Sql {
  /// <summary>
  /// Used to add a post processor to a data field for after the data is cast to the expected type from the DB.
  /// </summary>
  public class FetchedDataPostProcessorAttribute
    : Attribute {
    internal PostProcessor _postProcessor;

    public delegate object PostProcessor(object castDataValue);

    /// <summary>
    /// The member name of the PreProcessor delegate property or method.
    /// </summary>
    public string PostProcessorMemberName {
      get;
    }

    /// <summary>
    /// Mark a Data Property as having a Pre Processor function.
    /// </summary>
    /// <param name="PostProcessorMemberName"></param>
    public FetchedDataPostProcessorAttribute([NotNull] string PostProcessorMemberName) {
      this.PostProcessorMemberName = PostProcessorMemberName
        ?? throw new ArgumentNullException(nameof(PostProcessorMemberName));
    }
  }
}
