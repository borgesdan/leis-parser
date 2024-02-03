namespace Library
{
    public class LeiReader(string sourceText) : IDisposable
    {
        readonly string? sourceText = sourceText;        
        LeiNode? root = null;
        readonly TextReader reader = new StringReader(sourceText);
        LeiNode? parentNode = null;
        
        public LeiNode? Root => root;

        /// <summary>Lê o arquivo de texto que representa uma lei.</summary>
        public void Read()
        {
            root = new LeiNode();
            root.NodeType = LeiNodeType.Raiz;
            root.Children = new List<LeiNode>();

            if (string.IsNullOrWhiteSpace(sourceText))
                return;

            ReadLines(reader);            

            return;
        }

        /// <summary>Lê as linhas de arquivo de texto que representa uma lei.</summary>
        void ReadLines(TextReader reader)
        {
            string? line;

            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var node = new LeiNode
                {
                    Line = line
                };

                if (line.StartsWith("Título"))
                {
                    ReadTitulo(node);
                }
                else if (line.StartsWith("Capítulo"))
                {
                    ReadCapitulo(node);
                }
                else if (line.StartsWith("Seção"))
                {
                    ReadSecao(node);
                }
                else if (line.StartsWith("Art."))
                {
                    ReadArtigo(node);
                }
                else if (line.StartsWith("Parágrafo") || line.StartsWith('§'))
                {
                    ReadParagrafo(node);
                }
                else
                {
                    var split = line.Split('-');
                    var zero = split[0].Trim();                    

                    if (zero.Length <= 4 &&
                        (zero.StartsWith('I')
                        || zero.StartsWith('V')
                        || zero.StartsWith('X')))
                    {
                        ReadInciso(node);
                    }
                    else if(zero.Contains(')'))
                    {
                        ReadAlinea(node);
                    }
                    else
                    {
                        ReadTexto(node);
                    }
                }
            }
        }

        /// <summary>Lê um nó configurado como um título da lei.</summary>
        void ReadTitulo(LeiNode node)
        {
            node.NodeType = LeiNodeType.Titulo;

            if (root == null)
                throw new ArgumentNullException();

            root.Children ??= [];

            root.Children.Add(node);
            parentNode = node;            
        }

        /// <summary>Lê um nó configurado como um capítulo da lei.</summary>
        void ReadCapitulo(LeiNode node)
        {
            node.NodeType = LeiNodeType.Capitulo;

            if (parentNode != null && parentNode.NodeType < LeiNodeType.Capitulo)
            {
                parentNode.Children ??= [];
                parentNode.Children.Add(node);
            }
            else
            {
                var result = TryAddToRoot(node, (x) => x.NodeType <= LeiNodeType.Titulo);
                if (result) return;
            }

            node.Parent = parentNode;
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como uma seção da lei.</summary>
        void ReadSecao(LeiNode node)
        {
            node.NodeType = LeiNodeType.Secao;

            if (parentNode != null && parentNode.NodeType < LeiNodeType.Secao)
            {
                parentNode.Children ??= [];
                parentNode.Children.Add(node);
            }
            else
            {                
                var result = TryAddToRoot(node, (x) => x.NodeType <= LeiNodeType.Capitulo);
                if (result) return;
            }

            node.Parent = parentNode;
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como um artigo da lei.</summary>
        void ReadArtigo(LeiNode node)
        {
            node.NodeType = LeiNodeType.Artigo;
            
            if (parentNode != null && parentNode.NodeType < LeiNodeType.Artigo)
            {
                parentNode.Children ??= [];
                parentNode.Children.Add(node);
            }
            else
            {                
                var result = TryAddToRoot(node, (x) => x.NodeType < LeiNodeType.Artigo);
                if (result) return;
            }

            node.Parent = parentNode;
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como um paragráfo da lei.</summary>
        void ReadParagrafo(LeiNode node)
        {
            node.NodeType = LeiNodeType.Paragrafo;

            if (parentNode != null && parentNode.NodeType <= LeiNodeType.Artigo)
            {
                parentNode.Children ??= [];
                parentNode.Children.Add(node);          
            }
            else
            {
                var result = TryAddToRoot(node, (x) => x.NodeType <= LeiNodeType.Artigo);
                if (result) return;
            }

            node.Parent = parentNode;
        }

        /// <summary>Lê um nó configurado como um inciso da lei.</summary>
        void ReadInciso(LeiNode node)
        {
            node.NodeType = LeiNodeType.Inciso;

            if (parentNode != null && parentNode.NodeType == LeiNodeType.Artigo)
            {
                parentNode.Children ??= [];
                parentNode.Children.Add(node);           
            }
            else
            {
                var result = TryAddToRoot(node, (x) => x.NodeType <= LeiNodeType.Artigo);
                if (result) return;
            }

            node.Parent = parentNode;  
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como uma alínea da lei.</summary>
        void ReadAlinea(LeiNode node)
        {
            node.NodeType = LeiNodeType.Alinea;

            if (parentNode != null && parentNode.NodeType == LeiNodeType.Inciso)
            {
                parentNode.Children ??= [];
                parentNode.Children.Add(node);
            }
            else
            {
                var result = TryAddToRoot(node, (x) => x.NodeType <= LeiNodeType.Inciso);
                if (result) return;
            }                

            node.Parent = parentNode;
        }

        /// <summary>Lê um nó configurado como um tema de uma seção ou texto da lei.</summary>
        void ReadTexto(LeiNode node)
        {
            if (parentNode != null && parentNode.NodeType <= LeiNodeType.Secao)
                node.NodeType = LeiNodeType.Tema;
            else
                node.NodeType = LeiNodeType.Texto;

            if (parentNode != null)
            {
                parentNode.Children ??= [];
                parentNode.Children.Add(node);
            }
            else
            {
                if (root == null)
                    throw new ArgumentNullException();

                root.Children ??= [];
                root.Children.Add(node);
            }

            node.Parent = parentNode;
        }        

        /// <summary> 
        /// Tenta adicionar o nó atual ao nó raiz encontrado pela função lambda,
        /// caso não encontre adiciona o nó a lista de nós.
        /// Retorna true caso o nó seja adicionado a raiz.
        /// </summary>
        bool TryAddToRoot(LeiNode node, Func<LeiNode, bool> func)
        {
            var finder = parentNode?.FindRoot(func);

            if (finder == null)
            {
                if (root == null)
                    throw new ArgumentNullException();

                root.Children ??= [];
                root.Children.Add(node);
            }
            else
            {
                finder.Children ??= [];
                finder.Children.Add(node);
                node.Parent = finder;
                parentNode = node;
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            reader.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
