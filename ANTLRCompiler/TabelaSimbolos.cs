using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLRCompiler
{
	class TabelaSimbolos
	{

		public Dictionary<string, Simbolo> Tabela = new Dictionary<string, Simbolo>();

		public Simbolo Busca(string id)
		{
			return Tabela[id];
		}

		public void Elimina(int nivel)
		{

			var listaIds = new List<string>();

			foreach (var par in Tabela)
				if (par.Value.nivel == nivel)
					listaIds.Add(par.Key);

			foreach (var id in listaIds)
				Tabela.Remove(id);

		}

		public bool Insere(Simbolo simbolo)
		{

			if (!Declarado(simbolo.id, simbolo.nivel))
			{
				Tabela.Add(simbolo.id, simbolo);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool Declarado(string id, int nivel)
		{
			if (Tabela.ContainsKey(id))
				return Tabela[id].nivel <= nivel;
			else
				return false;
		}

		public void ImprimeTabela()
		{
			foreach(var par in this.Tabela)
			{
				Console.WriteLine("Classe: " + par.Value.classe + " ID: " + par.Value.id + " Tipo: " + par.Value.tipo +
								  " Escopo: " + " " + par.Value.nivel + " Tamanho: " + par.Value.tamanho + " Índice: " 
								  + par.Value.indice + " Param: " + par.Value.param);
			}
			Console.WriteLine();
		}

		public TabelaSimbolos RetornaEscopo (int nivel)
		{
			TabelaSimbolos escopo = new TabelaSimbolos();

			foreach (var par in Tabela) {
				if (par.Value.nivel == nivel)
					escopo.Insere(par.Value);
			}

			return escopo;
		}

		public TabelaSimbolos RetornaVariaveisGlobais()
		{
			TabelaSimbolos escopo = new TabelaSimbolos();

			foreach (var par in Tabela)
			{
				if (par.Value.nivel == 0 && par.Value.classe != Simbolo.Classe.funcao)
					escopo.Insere(par.Value);
			}

			return escopo;
		}

		public int RetornaTamanho()
		{
			int tamanho = 0;

			foreach (var par in Tabela)
			{
				tamanho += par.Value.tamanho;
			}

			return tamanho;
		}

		public int TotalParams()
		{
			int temp = 0;
			foreach(var par in Tabela)
			{
				if (par.Value.param)
					temp++;
			}
			return temp;
		}

		public int RetornaIndice(string nome_var)
		{
			foreach(var par in Tabela)
			{
				if (par.Value.id == nome_var)
					return par.Value.indice;
			}
			return -1;
		}

		public bool ehVetor(string nome_var)
		{
			foreach (var par in Tabela)
			{
				if (par.Value.id == nome_var && par.Value.classe == Simbolo.Classe.vetor)
					return true;
			}
			return false;
		}

		public bool ehVetorParam(string nome_var)
		{
			foreach (var par in Tabela)
			{
				if (par.Value.id == nome_var && par.Value.classe == Simbolo.Classe.vetor && par.Value.param)
					return true;
			}
			return false;
		}

	}

	class Simbolo
	{

		public enum Classe {inteiro, vetor, funcao};

		public string id;
		public Classe classe;
		public int tamanho;
		public string tipo;
		public int nivel;
		public int indice;
		public bool param;

		public Simbolo(string id, Classe classe, string tipo, int nivel, int tamanho, bool param, int indice)
		{
			this.id = id;
			this.classe = classe;
			this.tipo = tipo;
			this.nivel = nivel;
			this.tamanho = tamanho;
			this.param = param;
			this.indice = indice;
		}

	}

}
