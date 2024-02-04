using System.Diagnostics;

namespace Library
{
    public class LeiReader(string sourceText) : IDisposable
    {
        readonly string? sourceText = sourceText;        
        LeiNode? root = null;
        readonly TextReader reader = new StringReader(sourceText);
        LeiNode? parentNode = null;
        
        public LeiNode? Root => root;

        static class Token
        {
            public static readonly string Parte = "parte ";
            public static readonly string Titulo = "título ";
            public static readonly string Capitulo = "capítulo ";
            public static readonly string Secao = "seção ";
            public static readonly string Artigo = "art. ";
            public static readonly string ParagrafoUnico = "paragráfo ";
            public static readonly char Paragrafo = '§';
        }

        /// <summary>Lê o arquivo de texto que representa uma lei.</summary>
        public void Read()
        {
            root = new LeiNode
            {
                NodeType = LeiNodeType.Raiz,
                Children = []
            };

            parentNode = root;

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

                var lowerLine = line.ToLower();

                var node = new LeiNode
                {
                    Line = line
                };

                if (lowerLine.StartsWith(Token.Parte))
                {
                    ReadParte(node);
                }
                else if (lowerLine.StartsWith(Token.Titulo))
                {
                    ReadTitulo(node);
                }
                else if (lowerLine.StartsWith(Token.Capitulo))
                {
                    ReadCapitulo(node);
                }
                else if (lowerLine.StartsWith(Token.Secao))
                {
                    ReadSecao(node);
                }
                else if (lowerLine.StartsWith(Token.Artigo))
                {
                    ReadArtigo(node);
                }
                else if (lowerLine.StartsWith(Token.Paragrafo) || lowerLine.StartsWith(Token.ParagrafoUnico))
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
                    else if (zero.Contains("gif"))
                    {
                        ReadTexto(node);
                    }
                    else if(zero.Length >= 2 && zero[1] == ')')
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

        void ReadParte(LeiNode node)
        {
            node.NodeType = LeiNodeType.Parte;

            var result = TryAddToParent(node, (x) => x.NodeType < LeiNodeType.Parte);

            if (!result)
            {
                result = TryAddToRoot(node, (x) => x.NodeType < LeiNodeType.Parte);
                if (result) return;
            }            

            node.Parent = parentNode;
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como um título da lei.</summary>
        void ReadTitulo(LeiNode node)
        {
            node.NodeType = LeiNodeType.Titulo;            

            var result = TryAddToParent(node, (x) => x.NodeType < LeiNodeType.Titulo);

            if (!result)
            {
                result = TryAddToRoot(node, (x) => x.NodeType < LeiNodeType.Titulo);
                if (result) return;
            }

            node.Parent = parentNode;
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como um capítulo da lei.</summary>
        void ReadCapitulo(LeiNode node)
        {
            node.NodeType = LeiNodeType.Capitulo;            

            var result = TryAddToParent(node, (x) => x.NodeType < LeiNodeType.Capitulo);

            if (!result)
            {
                result = TryAddToRoot(node, (x) => x.NodeType < LeiNodeType.Capitulo);
                if (result) return;
            }

            node.Parent = parentNode;
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como uma seção da lei.</summary>
        void ReadSecao(LeiNode node)
        {
            node.NodeType = LeiNodeType.Secao;
            
            var result = TryAddToParent(node, (x) => x.NodeType < LeiNodeType.Secao);

            if (!result)
            {
                result = TryAddToRoot(node, (x) => x.NodeType < LeiNodeType.Secao);
                if (result) return;
            }


            node.Parent = parentNode;
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como um artigo da lei.</summary>
        void ReadArtigo(LeiNode node)
        {
            node.NodeType = LeiNodeType.Artigo;

            var result = TryAddToParent(node, (x) => x.NodeType < LeiNodeType.Artigo);

            if (!result)
            {
                result = TryAddToRoot(node, (x) => x.NodeType < LeiNodeType.Artigo);
                if (result) return;
            }

            node.Parent = parentNode;
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como um paragráfo da lei.</summary>
        void ReadParagrafo(LeiNode node)
        {
            node.NodeType = LeiNodeType.Paragrafo;
            
            var result = TryAddToParent(node, (x) => x.NodeType <= LeiNodeType.Artigo);

            if (!result)
            {
                result = TryAddToRoot(node, (x) => x.NodeType <= LeiNodeType.Artigo);
                if (result) return;
            }

            node.Parent = parentNode;
        }

        /// <summary>Lê um nó configurado como um inciso da lei.</summary>
        void ReadInciso(LeiNode node)
        {
            node.NodeType = LeiNodeType.Inciso;            

            var result = TryAddToParent(node, (x) => x.NodeType == LeiNodeType.Artigo);

            if (!result)
            {
                result = TryAddToRoot(node, (x) => x.NodeType <= LeiNodeType.Artigo);
                if (result) return;
            }

            node.Parent = parentNode;  
            parentNode = node;
        }

        /// <summary>Lê um nó configurado como uma alínea da lei.</summary>
        void ReadAlinea(LeiNode node)
        {
            node.NodeType = LeiNodeType.Alinea;            

            var result = TryAddToParent(node, (x) => x.NodeType == LeiNodeType.Inciso);

            if (!result)
            {
                result = TryAddToRoot(node, (x) => x.NodeType <= LeiNodeType.Inciso);
                if (result) return;
            }

            node.Parent = parentNode;
        }

        /// <summary>Lê um nó configurado como um tema de uma seção ou texto da lei.</summary>
        void ReadTexto(LeiNode node)
        {
            if (parentNode != null && parentNode.NodeType <= LeiNodeType.Secao && parentNode.NodeType != LeiNodeType.Raiz)
                node.NodeType = LeiNodeType.Tema;
            else
                node.NodeType = LeiNodeType.Texto;            

            var result = TryAddToParent(node, (x) => x.NodeType <= LeiNodeType.Alinea);

            if (!result)
            {
                result = TryAddToRoot(node, (x) => x.NodeType <= LeiNodeType.Inciso);
                if (result) return;
            }

            node.Parent = parentNode;
        }

        /// <summary> 
        /// Tenta adicionar o nó atual ao nó pai.        
        /// Retorna true caso o nó seja adicionado ao nó pai.
        /// </summary>
        bool TryAddToParent(LeiNode node, Func<LeiNode, bool> func)
        {
            if (parentNode != null && func(parentNode))
            {
                parentNode.Children ??= [];
                parentNode.Children.Add(node);

                return true;
            }

            return false;
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
                    throw new NullReferenceException();

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
