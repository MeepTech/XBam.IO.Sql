namespace Meep.Tech.XBam.IO.Sql {

  public partial class Query {

    public abstract partial class Builder {

      /// <summary>
      /// A builder for an opperation that can only add where tokens.
      /// </summary>
      public partial class UpdateOpperation : Builder, ICanWhere {

        protected UpdateOpperation(IBuilder<Builder> builder)
          : base(builder) { }
      }
    }
  }
}