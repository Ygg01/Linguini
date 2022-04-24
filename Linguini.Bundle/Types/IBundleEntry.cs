namespace Linguini.Bundle.Types
{
    public class FluentFunction
    {
        public ExternalFunction Function;
        private FluentFunction(ExternalFunction function)
        {
            Function = function;
        }

        public static implicit operator FluentFunction(ExternalFunction ef) => new FluentFunction(ef);
    }
}
