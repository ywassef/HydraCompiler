using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLRCompiler
{
	class DicionarioEscopos
	{
		Dictionary<string, Funcao> Escopos = new Dictionary<string, Funcao>();
		GeradorLabels labels = new GeradorLabels();

		public void Insere(string nome_funcao, TabelaSimbolos escopo)
		{
			Funcao temp = new Funcao(nome_funcao, escopo.RetornaTamanho(), escopo, labels.GerarLabelFuncao());
			Escopos.Add(nome_funcao, temp);
		}

		public string retornaLabelFuncao(string nome)
		{
			return Escopos[nome].retornaLabel();
		}

		public int retornaTamanhoFuncao(string nome)
		{
			return Escopos[nome].retornaTamanho();
		}

		public int retornaTotalParamsFuncao(string nome)
		{
			return Escopos[nome].retornaTotalParams();
		}

		public void ImprimeEscopo()
		{
			foreach (var par in this.Escopos)
			{
				par.Value.ImprimeFuncao();
			}
		}

		public int retornaIndiceVar(string nome_funcao, string nome_var)
		{
			if (nome_funcao == "input" || nome_funcao == "output")
				return 0;
			return Escopos[nome_funcao].retornaIndiceVar(nome_var);
		}

		public bool ehVetor (string nome_funcao, string nome_var)
		{
			if (nome_funcao == "input" || nome_funcao == "output")
				return false;
			return Escopos[nome_funcao].ehVetor(nome_var);
		}

		public bool ehVetorParam(string nome_funcao, string nome_var)
		{
			return Escopos[nome_funcao].ehVetorParam(nome_var);
		}

	}

	class Funcao
	{
		int tamanho;
		string nome, label;
		TabelaSimbolos variaveis;

		public Funcao (string nome, int tamanho, TabelaSimbolos variaveis, string label)
		{
			this.nome = nome;
			this.tamanho = tamanho;
			this.variaveis = variaveis;
			this.label = label;
			CriaIndices();
		}

		public void CriaIndices()
		{
			int i = 0;
			foreach (var par in variaveis.Tabela)
			{
				par.Value.indice = i;
				if (par.Value.classe == Simbolo.Classe.vetor)
					i += par.Value.tamanho;
				else
					i++;
			}
		}

		public string retornaLabel()
		{
			return label;
		}

		public bool ehVetor(string nome)
		{
			return variaveis.ehVetor(nome);
		}

		public bool ehVetorParam(string nome)
		{
			return variaveis.ehVetorParam(nome);
		}

		public int retornaTamanho()
		{
			return tamanho;
		}

		public int retornaTotalParams()
		{
			return variaveis.TotalParams();
		}

		public int retornaIndiceVar(string nome)
		{
			return variaveis.RetornaIndice(nome);
		}

		public void ImprimeFuncao()
		{
			Console.WriteLine("Função: " + nome + "\tTamanho: " + tamanho + "\tLabel: " + label);
			Console.WriteLine("------------------------------------------------------------------------------");
			variaveis.ImprimeTabela();
		}
	}

}
