using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using System.IO;

namespace ANTLRCompiler
{
	class Sintetizador : CminusBaseVisitor<object>
	{
		DicionarioEscopos escopos;
		CminusParser parser;
		StringBuilder assembly = new StringBuilder();
		GerenciadorRegistradores registradores = new GerenciadorRegistradores();
		GeradorLabels labels = new GeradorLabels();
		string escopo_atual;
		bool isIO = false;

		public Sintetizador(CminusParser parser, DicionarioEscopos escopos)
		{
			this.parser = parser;
			this.escopos = escopos;
		}

		public override object VisitPrograma([NotNull] CminusParser.ProgramaContext context)
		{

			//Console.WriteLine("VisitPrograma");


			assembly.AppendLine("mov r31, " + (escopos.retornaTamanhoFuncao("0000") - 1));
			//assembly.AppendLine("rm = " + (escopos.retornaTamanhoFuncao("0000") - 1));
			//assembly.AppendLine("mov r29, 50");
			//assembly.AppendLine("rx = MEM_SIZE");
			assembly.AppendLine("jmp MAIN");

			VisitChildren(context);

			File.WriteAllText("teste.temp", assembly.ToString());

			return null;
		}

		public override object VisitFundeclaracao([NotNull] CminusParser.FundeclaracaoContext context)
		{

			//Console.WriteLine("VisitFundeclaracao");

			var ID = context.ID().GetText();

			escopo_atual = ID;

			if (((string)ID).Equals("main"))
			{
				assembly.AppendLine("MAIN:");
				assembly.AppendLine("add r29, 0, 69");
				assembly.AppendLine("copy r63, r29");
				assembly.AppendLine("add r31, r31 " + escopos.retornaTamanhoFuncao(ID));
			}


			else if (ID != "input" && ID != "output")
			{
				assembly.AppendLine(escopos.retornaLabelFuncao(ID) + ":");
				//assembly.AppendLine("rm = rm + " + escopos.retornaTamanhoFuncao(ID));
				assembly.AppendLine("add r31, r31 " + escopos.retornaTamanhoFuncao(ID));

				//desempilhando os parametros
				int num_params = escopos.retornaTotalParamsFuncao(ID);

				string reg = registradores.NextDataReg();
				string mem_reg = registradores.NextMemReg();

				for (int i = num_params - 1; i >= 0; i--)
				{
					assembly.AppendLine("add r29, r29, 1");
					//assembly.AppendLine("rx = rx + 1");
					assembly.AppendLine("sub " + reg + ", r31, " + i);
					assembly.AppendLine("copy " + mem_reg + ", " + reg);
					//assembly.AppendLine(reg + " = rm - " + i);
					assembly.AppendLine("copy r63, r29");
					assembly.AppendLine("copy [" + mem_reg + "]" + ", [r63]");
					//assembly.AppendLine("[" + reg + "]" + " = [rx]");
				}

				registradores.FreeDataRegister(reg);
				registradores.FreeMemRegister(mem_reg);

				var children = VisitChildren(context);

				if (ID != "main")
				{
					//assembly.AppendLine("rm = rm - " + escopos.retornaTamanhoFuncao(ID));
					assembly.AppendLine("sub r31, r31 " + escopos.retornaTamanhoFuncao(ID));

					string reg_temp = registradores.NextDataReg();
					assembly.AppendLine("add r29, r29, 1");
					assembly.AppendLine("copy r63, r29");
					assembly.AppendLine("copy " + reg_temp + ", [r63]");

					//assembly.AppendLine("rx = rx + 1");
					assembly.AppendLine("jmpr " + reg_temp);

					registradores.FreeDataRegister(reg_temp);
				}

				return children;
			}

			return VisitChildren(context);
		}

		public override object VisitRetornovalor([NotNull] CminusParser.RetornovalorContext context)
		{
			//Console.WriteLine("VisitRetornovalor");

			var expressaodecl = Visit(context.expressaodecl());

			//assembly.AppendLine("ry = " + expressaodecl);
			if (((string)expressaodecl).IndexOf("r") == -1)
				assembly.AppendLine("mov r30, " + expressaodecl);
			else
				assembly.AppendLine("copy r30, " + expressaodecl);


			return expressaodecl;
		}

		public override object VisitExpressaodecl([NotNull] CminusParser.ExpressaodeclContext context)
		{
			var expressao = Visit(context.expressao());
			return expressao;
		}

		public override object VisitIteracaodecl([NotNull] CminusParser.IteracaodeclContext context)
		{
			string before_while = labels.GerarLabel();
			string out_while = labels.GerarLabel();


			//Console.WriteLine("Entrou no while de " + escopo_atual);

			assembly.AppendLine(before_while + ":");

			var expressao = Visit(context.expressao());

			string reg = registradores.NextDataReg();
			assembly.AppendLine("mov " + reg + ", " + out_while);
			registradores.FreeDataRegister(reg);

			assembly.AppendLine("jmprc " + reg + ", " + expressao);

			var statement = Visit(context.statement());

			assembly.AppendLine("jmp " + before_while);

			assembly.AppendLine(out_while + ":");

			if (((string)expressao).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)expressao));
				registradores.FreeDataRegister(((string)expressao));
			}

			return statement;
		}

		public override object VisitSelecaoif([NotNull] CminusParser.SelecaoifContext context)
		{
			//Console.WriteLine("VisitSelecaoif");

			var expressao = Visit(context.expressao());

			string label_if = labels.GerarLabel();

			string reg = registradores.NextDataReg();
			assembly.AppendLine("mov " + reg + ", " + label_if);
			registradores.FreeDataRegister(reg);

			assembly.AppendLine("jmprc " + reg + ", " + expressao);

			var statement = Visit(context.statement());

			assembly.AppendLine(label_if + ":");

			if (((string)expressao).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)expressao));
				registradores.FreeDataRegister(((string)expressao));
			}

			return statement;

		}

		public override object VisitSelecaoifelse([NotNull] CminusParser.SelecaoifelseContext context)
		{
			//Console.WriteLine("VisitSelecaoifelse");

			var expressao = Visit(context.expressao());

			string label_if = labels.GerarLabel();
			string label_else = labels.GerarLabel();
			string reg = registradores.NextDataReg();


			assembly.AppendLine("mov " + reg + ", " + label_if);
			assembly.AppendLine("jmprc " + reg + ", " + expressao);
			registradores.FreeDataRegister(reg);

			var statementif = Visit(context.statement(0));

			assembly.AppendLine("jmp " + label_else);

			assembly.AppendLine(label_if + ":");

			var statementelse = Visit(context.statement(1));

			assembly.AppendLine(label_else + ":");

			if (((string)expressao).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)expressao));
				registradores.FreeDataRegister(((string)expressao));
			}

			return null;

		}

		public override object VisitExpressao([NotNull] CminusParser.ExpressaoContext context)
		{
			//Console.WriteLine("VisitExpressao");
			return VisitChildren(context);
		}
		
		public override object VisitExpressaoatribuicao([NotNull] CminusParser.ExpressaoatribuicaoContext context)
		{
			//Console.WriteLine("VisitExpressaoatribuicao");
			var expressao = Visit(context.expressao());
			var variavel = Visit(context.var());

			if (((string)expressao).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)expressao));
				registradores.FreeDataRegister(((string)expressao));
			}

			//assembly.AppendLine(variavel + " = " + expressao);
			if (((string)expressao).IndexOf("r") == -1)
				assembly.AppendLine("mov " + variavel + ", " + expressao);
			else
				assembly.AppendLine("copy " + variavel + ", " + expressao);

			registradores.FreeMemRegister(((string)variavel));
			registradores.FreeDataRegister(((string)variavel));
			registradores.FreeMemRegister(((string)expressao));
			registradores.FreeDataRegister(((string)expressao));

			return null;
		}


		public override object VisitExpressaosimples([NotNull] CminusParser.ExpressaosimplesContext context)
		{
			//Console.WriteLine("VisitExpressaosimples");
			return VisitChildren(context);
		}

		public override object VisitSimplesexpressaorelacional([NotNull] CminusParser.SimplesexpressaorelacionalContext context)
		{
			//Console.WriteLine("VisitSimplesexpressaorelacional");

			var somaexpressao0 = Visit(context.somaexpressao(0));
			var relacional = Visit(context.relacional());
			var somaexpressao1 = Visit(context.somaexpressao(1));

			string reg = registradores.NextDataReg();

			if (((string)somaexpressao0).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)somaexpressao0));
				registradores.FreeDataRegister(((string)somaexpressao0));
			}

			if (((string)somaexpressao1).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)somaexpressao1));
				registradores.FreeDataRegister(((string)somaexpressao1));
			}

			switch ((string)relacional)
			{
				case "<":
					assembly.AppendLine("slt " + reg + ", " + somaexpressao0 + ", " + somaexpressao1);
					//assembly.AppendLine(reg + " = " + somaexpressao0 + " < " + somaexpressao1);
					break;
				case ">":
					//assembly.AppendLine(reg + " = " + somaexpressao1 + " < " + somaexpressao0);
					assembly.AppendLine("slt " + reg + ", " + somaexpressao1 + ", " + somaexpressao0);
					break;
				case "<=":
					assembly.AppendLine("slt " + reg  + ", " + somaexpressao1 + ", " + somaexpressao0);
					//assembly.AppendLine(reg + " = " + somaexpressao1 + " < " + somaexpressao0);
					assembly.AppendLine("not " + reg + ", " + reg);
					//assembly.AppendLine(reg + " = ~" + reg);
					break;
				case ">=":
					assembly.AppendLine("slt " + reg + ", " + somaexpressao0 + ", " + somaexpressao1);
					//assembly.AppendLine(reg + " = " + somaexpressao0 + " < " + somaexpressao1);
					assembly.AppendLine("not " + reg + ", " + reg);
					//assembly.AppendLine(reg + " = ~" + reg);
					break;
				case "==":
					//assembly.AppendLine(reg + " = " + somaexpressao0 + " == " + somaexpressao1);
					assembly.AppendLine("seq " + reg + ", " + somaexpressao0 + ", " + somaexpressao1);
					break;
				case "!=":
					assembly.AppendLine("seq " + reg + ", " + somaexpressao1 + ", " + somaexpressao0);
					//assembly.AppendLine(reg + " = " + somaexpressao1 + " == " + somaexpressao0);
					//assembly.AppendLine(reg + " = ~" + reg);
					assembly.AppendLine("not " + reg + ", " + reg);
					break;
				default:
					break;
			}
			
			return reg;
		}

		public override object VisitRelacional([NotNull] CminusParser.RelacionalContext context)
		{
			//Console.WriteLine("VisitRelacional");
			return context.GetText();
		}

		public override object VisitSimplesexpressaosomaexpressao([NotNull] CminusParser.SimplesexpressaosomaexpressaoContext context)
		{
			//Console.WriteLine("VisitSimplesexpressaosomaexpressao");
			return VisitChildren(context);
		}

		public override object VisitSomaexpressaotermo([NotNull] CminusParser.SomaexpressaotermoContext context)
		{
			//Console.WriteLine("VisitSomaexpressaotermo");
			return Visit(context.termo());
		}

		public override object VisitSomaexpressaosomatermo([NotNull] CminusParser.SomaexpressaosomatermoContext context)
		{
			//Console.WriteLine("VisitSomaexpressaosomatermo");
			var somaexpressao = Visit(context.somaexpressao());
			var soma = Visit(context.soma());
			var termo = Visit(context.termo());

			string reg = registradores.NextDataReg();

			if (((string)somaexpressao).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)somaexpressao));
				registradores.FreeDataRegister(((string)somaexpressao));
			}

			if (((string)termo).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)termo));
				registradores.FreeDataRegister(((string)termo));
			}

			//assembly.AppendLine(reg + " = " + somaexpressao + " " +  soma + " " + termo);
			if ((string)soma == "+")
				assembly.AppendLine("add " + reg + ", " + somaexpressao + ", " + termo);
			else if ((string)soma == "-")
				assembly.AppendLine("sub " + reg + ", " + somaexpressao + ", " + termo);

			return reg;
		}

		public override object VisitSoma([NotNull] CminusParser.SomaContext context)
		{
			//Console.WriteLine("VisitSoma");
			return context.GetText();
		}

		public override object VisitTermofator([NotNull] CminusParser.TermofatorContext context)
		{
			//Console.WriteLine("VisitTermofator");
			return Visit(context.fator());
		}

		public override object VisitTermomultfator([NotNull] CminusParser.TermomultfatorContext context)
		{
			//Console.WriteLine("VisitTermomultfator");
			var termo = Visit(context.termo());
			var mult = Visit(context.mult());
			var fator = Visit(context.fator());

			string reg = registradores.NextDataReg();

			if (((string)termo).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)termo));
				registradores.FreeDataRegister(((string)termo));
			}

			if (((string)fator).IndexOf("r") != -1)
			{
				registradores.FreeMemRegister(((string)fator));
				registradores.FreeDataRegister(((string)fator));
			}

			//assembly.AppendLine(reg + " = " + termo + " " + mult + " " + fator);
			if ((string)mult == "*")
				assembly.AppendLine("mult " + reg + ", " + termo + ", " + fator);
			else if ((string)mult == "/")
				assembly.AppendLine("div " + reg + ", " + termo + ", " + fator);

			return reg;
		}

		public override object VisitMult([NotNull] CminusParser.MultContext context)
		{
			//Console.WriteLine("VisitMult");
			return context.GetText();
		}

		public override object VisitFatornum([NotNull] CminusParser.FatornumContext context)
		{
			//Console.WriteLine("VisitFatornum");
			return context.NUM().GetText();
		}

		public override object VisitFatorexpressao([NotNull] CminusParser.FatorexpressaoContext context)
		{
			//Console.WriteLine("VisitFatorexpressao");
			return Visit(context.expressao());
		}

		public override object VisitFatorvar([NotNull] CminusParser.FatorvarContext context)
		{
			//Console.WriteLine("VisitFatorvar");
			return Visit(context.var());
		}

		public override object VisitFatorativacao([NotNull] CminusParser.FatorativacaoContext context)
		{
			//Console.WriteLine("VisitFatorativacao");
			return Visit(context.ativacao());
		}

		public override object VisitAtivacao([NotNull] CminusParser.AtivacaoContext context)
		{
			//Console.WriteLine("Visitativacao");
			//assembly.AppendLine("\nVisirativacao");

			//assembly.AppendLine("movr [rx], PC+2");

			//assembly.AppendLine("rx = rx - 1");
			string reg2 = registradores.NextDataReg();
			registradores.FreeDataRegister(reg2);

			if(context.ID().GetText() == "input")
			{
				string reg3 = registradores.NextDataReg();
				assembly.AppendLine("input " + reg3);
				return reg3;
			}

			if (context.ID().GetText() == "output")
			{
				isIO = true;
				var termos = Visit(context.arglista());
				isIO = false;
				assembly.AppendLine("output " + termos);
				string[] termo_free = ((string)termos).Split(',');
				registradores.FreeDataRegister(termo_free[0]);
				return termo_free[0];
			}

			string reg_pc = registradores.NextMemReg();
			assembly.AppendLine("copy " + reg_pc + ", r63");
			assembly.AppendLine("sub r29, r29, 1");
			assembly.AppendLine("copy r63, r29");

			var args = Visit(context.arglista());

			assembly.AppendLine("movr [" + reg_pc + "], 1");
			registradores.FreeMemRegister(reg_pc);
			//assembly.AppendLine("add " + reg2 + ", [r63], 1");
			assembly.AppendLine("jmp " +  escopos.retornaLabelFuncao(context.ID().GetText()));

			string reg = registradores.NextDataReg();

			assembly.AppendLine("copy " + reg + ", r30");
			//assembly.AppendLine(reg + " = ry");

			return reg;
		}

		public override object VisitListaargs([NotNull] CminusParser.ListaargsContext context)
		{
			//Console.WriteLine("Visitlistaargs");
			//assembly.AppendLine("\nVisitlistaargs");


			var expressao = Visit(context.expressao());

			if (isIO)
			{
				Console.WriteLine("\t\tentrou no isIO");
				return expressao;
			}

			var arglista = Visit(context.arglista());
			if (((string)expressao).IndexOf('r') == -1)
				assembly.AppendLine("mov [r63], " + expressao);
			else
				assembly.AppendLine("copy [r63], " + expressao);

			//assembly.AppendLine("[rx] = " + expressao);

			assembly.AppendLine("sub r29, r29, 1");
			assembly.AppendLine("copy r63, r29");

			//assembly.AppendLine("rx = rx - 1");

			registradores.FreeDataRegister((string)expressao);
			registradores.FreeMemRegister((string)expressao);

			return arglista;
		}

		public override object VisitUnicoarg([NotNull] CminusParser.UnicoargContext context)
		{
			//Console.WriteLine("VisitUnicoarg");
			//assembly.AppendLine("\nVisirunicoarg");

			var expressao = Visit(context.expressao());

			if (isIO)
				return expressao;

			if (((string)expressao).IndexOf('r') == -1)
				assembly.AppendLine("mov [r63], " + expressao);
			else
				assembly.AppendLine("copy [r63], " + expressao);
			//assembly.AppendLine("[rx] = " + expressao);

			assembly.AppendLine("sub r29, r29, 1");
			assembly.AppendLine("copy r63, r29");

			//assembly.AppendLine("rx = rx - 1");

			//Console.WriteLine("antes de liberar os regs");


			registradores.FreeDataRegister((string)expressao);
			registradores.FreeMemRegister((string)expressao);

			//Console.WriteLine("liberou os regs");

			return expressao;
		}

		public override object VisitVar([NotNull] CminusParser.VarContext context)
		{
			//Console.WriteLine("VisitVar");
			return VisitChildren(context);
		}

		public override object VisitVarnormal([NotNull] CminusParser.VarnormalContext context)
		{
			//Console.WriteLine("VisitVarnormal");
			//assembly.AppendLine("\nVisirvarnormal");

			var variavel = context.ID().GetText();

			string reg = registradores.NextDataReg();
			string mem_reg = registradores.NextMemReg();
			string var_reg = "[" + mem_reg + "]";

			int indice_funcao = escopos.retornaIndiceVar(escopo_atual, variavel);
			int indice_global = escopos.retornaIndiceVar("0000", variavel);

			if (indice_global == -1)
			{
				//Console.WriteLine("visitou var " + variavel + " com indice " + indice_funcao);
				//Console.WriteLine("Visitando var " + variavel + "com indice funcao de " + indice_funcao);
				//assembly.AppendLine("//" + escopo_atual + indice_funcao + variavel);
				assembly.AppendLine("sub " + reg + ", r31" + ", " + indice_funcao);
				assembly.AppendLine("copy " + mem_reg + ", " + reg);
				registradores.FreeDataRegister(reg);
				//assembly.AppendLine(reg + " = rm - " + indice_funcao);
			}
			else
			{
				assembly.AppendLine("mov " + mem_reg + ", " + indice_global);
				//assembly.AppendLine(reg + " = " + indice_global);
			}

			if (escopos.ehVetor(escopo_atual, variavel) && !escopos.ehVetorParam(escopo_atual, variavel))
				return reg;
			else
				return var_reg;
		}

		public override object VisitVarvetor([NotNull] CminusParser.VarvetorContext context)
		{
			//Console.WriteLine("VisitVarvetor");
			//assembly.AppendLine("\nVisirvarvetor");

			var variavel = context.ID().GetText();
			var expressao = Visit(context.expressao());

			string reg = registradores.NextDataReg();
			string mem_reg = registradores.NextMemReg();
			string var_reg = "[" + mem_reg + "]";


			int indice_funcao = escopos.retornaIndiceVar(escopo_atual, variavel);
			int indice_global = escopos.retornaIndiceVar("0000", variavel);


			if (indice_global == -1)
			{
				if (escopos.ehVetorParam(escopo_atual, variavel))
				{
					//Console.WriteLine("\t\tentrou em vetor param");
					assembly.AppendLine("sub " + reg + ", r31, " + indice_funcao);
					assembly.AppendLine("copy " + mem_reg + ", " + reg);
					//assembly.AppendLine(reg + " = rm - " + indice_funcao);
					string reg2 = registradores.NextDataReg();
					assembly.AppendLine("copy " + reg2 + ", [" + mem_reg + "]");
					//assembly.AppendLine(reg2 + " = [" + reg + "]");
					assembly.AppendLine("sub " + reg2 + ", " + reg2 + ", " + expressao);
					assembly.AppendLine("copy " + mem_reg + ", " + reg2);
					//assembly.AppendLine(reg2 + " = " + reg2 + " + " + expressao);
					registradores.FreeDataRegister(reg2);
					registradores.FreeDataRegister(reg);
					return "[" + mem_reg + "]";
				}
				else
				{
					assembly.AppendLine("add " + reg + ", " + indice_funcao + ", " + expressao);
					//assembly.AppendLine(reg + " = " + indice_funcao + " + " + expressao);
					assembly.AppendLine("sub " + reg + ", r31, " + reg);
					registradores.FreeDataRegister(reg);
					//assembly.AppendLine(reg + " = rm - " + reg);
				}
			}
				
			else
			{
				assembly.AppendLine("add " + reg + ", " + indice_global + ", " + expressao);
				//assembly.AppendLine(reg + " = " + indice_global + " + " + expressao);
			}

			assembly.AppendLine("copy " + mem_reg + ", " + reg);
			return var_reg;
		}
	}
}
