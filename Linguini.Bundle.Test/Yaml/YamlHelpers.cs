using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using YamlDotNet.RepresentationModel;

namespace Linguini.Bundle.Test.Yaml
{
    public static class YamlHelpers
    {
        // To fetch nodes by key name with YAML, we NEED a YamlScalarNode.
        // We use a thread local one to avoid allocating one every fetch, since we just replace the inner value.
        // Obviously thread local to avoid threading issues.
        private static readonly ThreadLocal<YamlScalarNode> FetchNode =
            new(() => new YamlScalarNode());
        
        [Pure]
        public static bool TryGetNode<T>(this YamlMappingNode mapping, string key, [NotNullWhen(true)] out T? returnNode) where T : YamlNode
        {
            if (mapping.Children.TryGetValue(_getFetchNode(key), out var node))
            {
                returnNode = (T) node;
                return true;
            }

            returnNode = null;
            return false;
        }
        private static YamlScalarNode _getFetchNode(string key)
        {
            var node = FetchNode.Value!;
            node.Value = key;
            return node;
        }
    }
}