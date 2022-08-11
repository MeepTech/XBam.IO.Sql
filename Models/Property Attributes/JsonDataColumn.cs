using System;

namespace Meep.Tech.XBam.IO.Sql {

  /// <summary>
  /// Indicates this member property should be a json property in the '_data' db column.
  /// The property name can be overriden by an AutoBuildAttribute or by a ColumnAttrubte, with the column attribute taking precidence. 
  /// </summary>
  public class JsonDataColumnAttribute 
    : Attribute {}
}
