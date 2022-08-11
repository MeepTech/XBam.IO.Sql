namespace Meep.Tech.XBam.IO.Sql {
  public static class SqlContextExtensions {

    /// <summary>
    /// Get the universe's sql context
    /// </summary>
    public static SqlContext Sql(this Universe universe)
      => universe.GetExtraContext<SqlContext>();
  }
}