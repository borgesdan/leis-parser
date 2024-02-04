using System.Text;

namespace Library
{
    public class LeiNode
    {
        public LeiNode() { }

        public LeiNode(string line, LeiNodeType lineType)
        {
            Line = line;
            NodeType = lineType;
        }

        public string Line { get; set; } = string.Empty;
        public LeiNodeType NodeType { get; set; }
        public List<LeiNode>? Children { get; set; } = null;
        public LeiNode? Parent { get; set; } = null;
        public bool Revogado { get; set; }
        public bool Suspenso { get; set; }

        static int tabCount = 0;

        public LeiNode? FindRoot(LeiNodeType nodeType)
        {
            LeiNode? root = this;

            while (root != null)
            {
                if (root.NodeType == nodeType)
                    return root;

                root = root.Parent;
            }

            return null;
        }

        public LeiNode? FindRoot(Func<LeiNode, bool> func)
        {
            LeiNode? root = this;

            while (root != null)
            {
                var result = func(root);

                if (result)
                    return root;

                root = root.Parent;
            }

            return null;
        }               
    }
}
