namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {

      /// <summary>
      /// A builder for an opperation that cannot add more tokens.
      /// </summary>
      public partial class ClosedOpperation : Builder {

        protected ClosedOpperation(IBuilder<Builder> builder)
          : base(builder) { }
      }
    }
  }
}