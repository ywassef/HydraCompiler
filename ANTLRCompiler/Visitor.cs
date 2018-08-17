using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;

namespace ANTLRCompiler
{
	class Visitor : CminusBaseVisitor<object>
	{

		int nivel = 0;
		int indice = 0;
		bool temMain = false;
		bool emExpressao = false;
		TabelaSimbolos Tabela = new TabelaSimbolos();
		DicionarioEscopos Escopos = new DicionarioEscopos();
		CminusParser parser;

		public Visitor(CminusParser parser)
		{
			this.parser = parser;
		}

		public DicionarioEscopos GetDicionarioEscopos()
		{
			return Escopos;
		}

		public override object VisitFundeclaracao([NotNull] CminusParser.FundeclaracaoContext context)
		{

			var ID = context.ID().GetText();

			if (ID == "main")
				temMain = true;

			if (!Tabela.Declarado(ID, nivel))
				Tabela.Insere(new Simbolo(ID, Simbolo.Classe.funcao, context.tipoespecificador().GetText(), nivel, 0, false, 0));
			else
				Console.WriteLine("Linha " + context.start.Line + ": Erro ao declarar a função \'" + ID + "\'.\n\tVerifique se já há alguma outra variável/função com o mesmo nome.");
			nivel++;

			VisitChildren(context);
			indice = 0;

			Escopos.Insere(ID, Tabela.RetornaEscopo(nivel));

			Tabela.Elimina(nivel);
			nivel--;

			return null;
		}

		public override object VisitInteirodeclaracao([NotNull] CminusParser.InteirodeclaracaoContext context)
		{

			var ID = context.ID().GetText();

			if (context.tipoespecificador().GetText() == "void")
				Console.WriteLine("Linha " + context.start.Line + ": Declaração da variável \'" + ID + "\' como void não permitida.");

			if (!Tabela.Declarado(ID, nivel))
			{
				Tabela.Insere(new Simbolo(ID, Simbolo.Classe.inteiro, context.tipoespecificador().GetText(),nivel, 1, false, indice));
				indice++;
			}
			else
				Console.WriteLine("Linha " + context.start.Line + ": Erro ao declarar a variável \'" + ID + "\'.\n\tVerifique se já há algum outro variável/função com o mesmo nome.");

			VisitChildren(context);

			return null;
		}

		public override object VisitParamlista([NotNull] CminusParser.ParamlistaContext context)
		{
			VisitChildren(context);

			return null;
		}

		public override object VisitVetordeclaracao([NotNull] CminusParser.VetordeclaracaoContext context)
		{

			var ID = context.ID().GetText();

			if (context.tipoespecificador().GetText() == "void")
				Console.WriteLine("Linha " + context.start.Line + ": Declaração da variável \'" + ID + "\' como void não permitida.");

			if (!Tabela.Declarado(ID, nivel))
			{
				Tabela.Insere(new Simbolo(ID, Simbolo.Classe.vetor, context.tipoespecificador().GetText(), nivel, int.Parse(context.NUM().GetText()), false, indice));
				indice += int.Parse(context.NUM().GetText());
			}
			else
				Console.WriteLine("Linha " + context.start.Line + ": Erro ao declarar a variável \'" + ID + "\'.\n\tVerifique se já há algum outro variável/função com o mesmo nome.");

			VisitChildren(context);

			return null;
		}

		public override object VisitInteiroparam([NotNull] CminusParser.InteiroparamContext context)
		{

			var ID = context.ID().GetText();

			if (!Tabela.Declarado(ID, nivel))
			{
				Tabela.Insere(new Simbolo(ID, Simbolo.Classe.inteiro, context.tipoespecificador().GetText(), nivel, 1, true, indice));
				indice++;
			}
			else
				Console.WriteLine("Linha " + context.start.Line + ": Erro ao declarar o parâmetro \'" + ID + "\'.\n\tVerifique se já há alguma outra variável/função com o mesmo nome.");

			VisitChildren(context);

			return null;
		}

		public override object VisitVetorparam([NotNull] CminusParser.VetorparamContext context)
		{
			
			var ID = context.ID().GetText();

			if (!Tabela.Declarado(ID, nivel))
			{
				Tabela.Insere(new Simbolo(ID, Simbolo.Classe.vetor, context.tipoespecificador().GetText(), nivel, 1, true, indice));
				indice++;
			}
			else
				Console.WriteLine("Linha " + context.start.Line + ": Erro ao declarar o parâmetro \'" + ID + "\'.\n\tVerifique se já há alguma outra variável/função com o mesmo nome.");

			VisitChildren(context);

			return null;
		}

		public override object VisitPrograma([NotNull] CminusParser.ProgramaContext context)
		{

			VisitChildren(context);

			if (temMain == false)
				Console.WriteLine("Linha " + 0 + ": Programa sem função main.");

			Escopos.Insere("0000", Tabela.RetornaVariaveisGlobais());

			Escopos.ImprimeEscopo();

			return null;
		}

		public override object VisitVarvetor([NotNull] CminusParser.VarvetorContext context)
		{

			var ID = context.ID().GetText();

			if (!Tabela.Declarado(ID, nivel))
				Console.WriteLine("Linha " + context.start.Line + ": Erro ao procurar a variável \'" + ID + "\'.\n\tVariável não declarada");

			VisitChildren(context);

			return null;
		}

		public override object VisitVarnormal([NotNull] CminusParser.VarnormalContext context)
		{

			var ID = context.ID().GetText();

			if (!Tabela.Declarado(ID, nivel))
				Console.WriteLine("Linha " + context.start.Line + ": Erro ao procurar a variável \'" + ID + "\'.\n\tVariável não declarada");

			VisitChildren(context);

			return null;
		}

		public override object VisitAtivacao([NotNull] CminusParser.AtivacaoContext context)
		{

			var ID = context.ID().GetText();

			if (Tabela.Declarado(ID, nivel))
			{
				if (emExpressao && Tabela.Busca(ID).tipo == "void")
					Console.WriteLine("Linha " + context.start.Line + ": A função \'" + ID + "\' retorna void, logo não é possível usá-la em uma variável int.\n");
			}
			else if(ID != "input" && ID != "output")
				Console.WriteLine("Linha " + context.start.Line + ": Erro ao chamar a função \'" + ID + "\'.\n\tFunção não declarada");

			VisitChildren(context);

			return null;
		}

		public override object VisitExpressaoatribuicao([NotNull] CminusParser.ExpressaoatribuicaoContext context)
		{

			emExpressao = true;

			VisitChildren(context);

			emExpressao = false;

			return null;
		}

	}
}
