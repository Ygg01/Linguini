using NUnit.Framework;

namespace PluralRule.Test
{
    public class TestSG
    {
        [Test]
        public void TestGenerator()
        {
            HelloWorldGenerated.HelloWorld.SayHello();
        }
    }
}
