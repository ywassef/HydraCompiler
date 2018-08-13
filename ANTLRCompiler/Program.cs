using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ANTLRCompiler
{
	class Program
	{
		static void Main(string[] args)
		{
			/*
			Console.Write("> ");
			string input = Console.ReadLine();
			*/

			var input = File.ReadAllText(args[0]);

			var inputStream = new AntlrInputStream(new StringReader(input));
			var lexer = new CminusLexer(inputStream);
			var tokens = new CommonTokenStream(lexer);
			var parser = new CminusParser(tokens);
			var tree = parser.programa();

			//Console.WriteLine(tree.ToStringTree(parser));
			//Console.WriteLine();

			var visitor = new Visitor(parser);
			visitor.Visit(tree);
			DicionarioEscopos escopos = visitor.GetDicionarioEscopos();
			Sintetizador sintetizador = new Sintetizador(parser, escopos);
			sintetizador.Visit(tree);

			ConversorBinario.Converter("teste");

			//Console.WriteLine(ImprimeBonitinho(tree.ToStringTree(parser)));

			Console.ReadKey();
		}

		static string ImprimeBonitinho(string stringtree)
		{

			var tabCount = 0;
			var output = new StringBuilder();

			string[] tokens = stringtree.Split(' ');
			foreach (string token in tokens)
			{
				string stringtabs = "";
				for (var i = 0; i < tabCount; i++)
					stringtabs += "    ";

				output.AppendLine(stringtabs + (token.Length == 1 ? token
																  : token[0] == ')' ? ")"
																					: token.Replace("(", "").Replace(")", "")));

				tabCount += token.Length == 1 ? 0
											  : token.Contains('(') ? 1 : 0;
				tabCount -= token.Length == 1 ? 0
											  : token.Count(c => c == ')');


			}

			return output.ToString();

		}
	}
}
