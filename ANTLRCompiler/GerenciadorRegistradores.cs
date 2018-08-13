using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLRCompiler
{
	class GerenciadorRegistradores
	{
		bool[] DataRegisters = new bool[32];
		bool[] MemRegisters = new bool[32];

		public GerenciadorRegistradores()
		{
			for (int i = 0; i < 32; i++)
			{
				DataRegisters[i] = false;
				MemRegisters[i] = false;
			}
			DataRegisters[29] = true; //rx - ponteiro da pilha
			DataRegisters[30] = true; //ry - valor de retorno
			DataRegisters[31] = true; //rm - endereço base de memória
			MemRegisters[31] = true; //mem reg que vai ser usado pra fazer acesso
		}

		public string NextDataReg()
		{
			for (int i = 0; i < 32; i++)
			{
				if (DataRegisters[i] == false)
				{
					DataRegisters[i] = true;
					return "r" + i;
				}
			}

			Console.WriteLine("Não há mais Data Registers disponíveis");
			return null;
		}

		public string NextMemReg()
		{
			for (int i = 0; i < 32; i++)
			{
				if (MemRegisters[i] == false)
				{
					MemRegisters[i] = true;
					return "r" + (i + 32);
				}
			}

			Console.WriteLine("Não há mais Memory Registers disponíveis");
			return null;
		}

		public void FreeDataRegister(string reg)
		{
			//Console.WriteLine("reg param: " + reg);
			if (reg.IndexOf("[") != -1)
			{
				reg = reg.Replace('[', ' ');
				reg = reg.Replace(']', ' ');
			}

			int num = int.Parse(reg.Replace('r', ' '));

			if (num < 32)
			{
				if (DataRegisters[num] == true)
					DataRegisters[num] = false;
				//else
					//Console.WriteLine("O Data Registers já estava disponível");
			}
		}

		public void FreeMemRegister(string reg)
		{
			//Console.WriteLine("reg param mem: " + reg);
			if (reg.IndexOf("[") != -1)
			{
				reg = reg.Replace('[', ' ');
				reg = reg.Replace(']', ' ');
			}
			int num = int.Parse(reg.Replace('r', ' '));
			if (32 <= num  && num < 64)
			{
				if (MemRegisters[num - 32] == true)
					MemRegisters[num - 32] = false;
				//else
					//Console.WriteLine("O Mem Registers já estava disponível");
			}
			
		}
	}
}
