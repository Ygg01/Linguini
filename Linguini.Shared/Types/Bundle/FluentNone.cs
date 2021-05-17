namespace Linguini.Shared.Types.Bundle
{
    public record FluentNone : IFluentType
    {
        public static readonly FluentNone None = new();

        private FluentNone()
        {
        }

        public IFluentType Copy()
        {
            return None;
        }

        public string AsString()
        {
            return "{???}";
        }

        public override string ToString()
        {
            return AsString();
        }
    }
}
