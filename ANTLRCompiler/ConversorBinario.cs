using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ANTLRCompiler
{
	class ConversorBinario
	{
		public enum opcodes {
			add = 0, sub = 1, mult = 2,
			div = 3, not = 9, slt = 12,
			seq = 13, jmpr = 14, jmprc = 15, input = 16,
			output = 17, copy = 25, beq = 22,
			jmp = 23, mov = 24, movr = 26
		};

		public static Dictionary<string, int> labels = new Dictionary<string, int>();
		public static StringBuilder binary_code = new StringBuilder();


		public static string RetornaOperando(string optype, string[] operandos, int num_op)
		{
			string op;
			if (labels.ContainsKey(operandos[num_op]))
			{
				int value_jump = labels[operandos[num_op]];
				string temp = Convert.ToString(value_jump, 2);
				string jmp_address = temp.Length < 8 ? temp.PadLeft(8, '0') : temp;
				return jmp_address;
			}
			if(operandos[0] == "jumpr")
			{
				string temp = operandos[2].Replace('[', ' ');
				temp = temp.Replace(']', ' ');
				temp = temp.Replace('r', ' ');
				temp = temp.Replace(',', ' ');
				string num_reg = Convert.ToString(int.Parse(temp), 2);
				string jmp_reg = (num_reg.Length < 6 ? num_reg.PadLeft(6, '0') : num_reg);
				return "00" + jmp_reg;
			}
			if ((operandos[0] == "mov" || operandos[0] == "movr") && num_op == 2)
			{
				string imm = Convert.ToString(int.Parse(operandos[2]), 2);
				string imm_out = operandos[2].IndexOf('-') == -1 ? imm.PadLeft(16, '0') : imm.PadLeft(16, '1');
				if (imm_out.Length > 16)
					imm_out = imm_out.Substring(16);

				return imm_out;
			}
			if (optype == "1")
			{
				string temp2;
				if (operandos[num_op].IndexOf('[') != -1)
				{
					string temp = operandos[num_op].Replace('[', ' ');
					temp2 = temp.Replace(']', ' ');
					op = "11";
				}
				else
				{
					temp2 = operandos[num_op];
					op = "00";
				}

				string temp4 = temp2.Replace(',', ' ');
				string temp3 = temp4.Replace('r', ' ');
				string num_reg = Convert.ToString(int.Parse(temp3), 2);
				op = op + (num_reg.Length < 6 ? num_reg.PadLeft(6, '0') : num_reg);
			}
			else
			{
				//Console.WriteLine(operandos[num_op] + ' ' + optype + ' ' + num_op);
				string maybe_label = operandos[num_op].Replace(",", "");
				if (labels.ContainsKey(maybe_label))
				{
					int value_jump = labels[maybe_label];
					string temp = Convert.ToString(value_jump, 2);
					string jmp_address = temp.Length < 8 ? temp.PadLeft(8, '0') : temp;
					return jmp_address;
				}
				//Console.WriteLine("ka " + operandos[0] + ' ' + num_op + ' ' + operandos[num_op] + ' ' + optype + "contains key: " + labels.ContainsKey(operandos[1]));
				int op_imm = int.Parse(operandos[num_op].Replace(',', ' '));
				op = Convert.ToString(op_imm, 2);
				op = operandos[1].IndexOf('-') == -1 ? op.PadLeft(8, '0') : op.PadLeft(8, '1');
			}

			return op;
		}

		public static void Converter(string filename)
		{
			var input = File.ReadAllText(filename + ".temp");
			input = input.Replace("\r", "");
			string[] instrucoes = input.Split('\n');

			for (int j = 0; j < instrucoes.Length; j++) {
				if (instrucoes[j].Contains(":"))
				{
					string label = instrucoes[j].Replace(":", "");
					labels.Add(label, j*2);
				}
			}

			int i = 0;
			foreach(var instrucao in instrucoes)
			{
				binary_code.AppendLine("//" + instrucao);
				string[] operandos = instrucao.Split(' ');
				if(operandos.Length == 1)
				{
					if(operandos[0].IndexOf("Vis") == -1)
					{
						binary_code.AppendLine("inst_memory[" + i + "] = 16'B1111100000000000;");
						binary_code.AppendLine("inst_memory[" + (i + 1) + "] = 0;");
						binary_code.AppendLine();
						i += 2;
					}
				}
				else
				{
					if (operandos[0] == "output")
					{
						//Console.WriteLine(instrucao + "esse cu de merda de output tem " + operandos[0] + ' ' + operandos[1] + ' ' + operandos[2]);
						string optype1 = operandos[1].IndexOf('r') == -1 ? "0" : "1";
						string reg1 = RetornaOperando(optype1, operandos, 1);
						//Console.WriteLine(reg1 + ' ' + optype1);
						//Console.WriteLine(reg2 + ' ' + optype2);
						binary_code.AppendLine("inst_memory[" + i + "] = 16'B100010" + optype1 + "000000001;");
						binary_code.AppendLine("inst_memory[" + (i + 1) + "] = 16'B" + reg1 + "00000000;");
						binary_code.AppendLine();
						i += 2;
					}
					else if (operandos[0] == "jmp")
					{
						string jmp_address = RetornaOperando("0", operandos, 1);
						binary_code.AppendLine("inst_memory[" + i + "] = 16'B1011100000000000;");
						binary_code.AppendLine("inst_memory[" + (i + 1) + "] = 16'B" + jmp_address.PadLeft(16, '0') + ";");
						binary_code.AppendLine();
						i += 2;
					}
					else if(operandos[0] == "jmpr")
					{
						string jmp_address = RetornaOperando("1", operandos, 1);
						binary_code.AppendLine("inst_memory[" + i + "] = 16'B0111001000000000;");
						binary_code.AppendLine("inst_memory[" + (i + 1) + "] = 16'B" + jmp_address + "00000000" + ";");
						binary_code.AppendLine();
						i += 2;
					}
					else if (operandos[0] == "jmprc")
					{
						string jmp_address = RetornaOperando("1", operandos, 1);
						string condition = RetornaOperando("1", operandos, 2);
						binary_code.AppendLine("inst_memory[" + i + "] = 16'B0111101100000000;");
						binary_code.AppendLine("inst_memory[" + (i + 1) + "] = 16'B" + jmp_address + condition + ";");
						binary_code.AppendLine();
						i += 2;
					}
					else if(operandos[0] == "mov" || operandos[0] == "movr")
					{
						string optype1 = operandos[1].IndexOf('r') == -1 ? "0" : "1";
						string op1 = RetornaOperando(optype1, operandos, 1);
						string imm = RetornaOperando("0", operandos, 2);
						if(operandos[0] == "mov")
							binary_code.AppendLine("inst_memory[" + i + "] = 16'B11000" + optype1 + "00" + op1 + ";");
						else
							binary_code.AppendLine("inst_memory[" + i + "] = 16'B11010" + optype1 + "00" + op1 + ";");

						binary_code.AppendLine("inst_memory[" + (i+1) + "] = 16'B" + imm.PadLeft(16, '0') + ";");
						binary_code.AppendLine();
						i += 2;
					}
					else
					{
						string optype1 = operandos[1].IndexOf('r') == -1 ? "0" : "1";
						string op1 = RetornaOperando(optype1, operandos, 1);
						string optype2, optype3;
						string op2, op3;

						if (operandos.Length > 2)
						{
							optype2 = operandos[2].IndexOf('r') == -1 ? "0" : "1";
							op2 = RetornaOperando(optype2, operandos, 2);
						}
						else
						{
							optype2 = "0";
							op2 = "00000000";
						}

						if (operandos.Length > 3)
						{
							optype3 = operandos[3].IndexOf('r') == -1 ? "0" : "1";
							op3 = RetornaOperando(optype3, operandos, 3);
						}
						else
						{
							optype3 = "0";
							op3 = "00000000";
						}
						int opcode = (int)Enum.Parse(typeof(opcodes), operandos[0]);
						string opcode_s = Convert.ToString(opcode, 2);
						opcode_s = opcode_s.Length < 5 ? opcode_s.PadLeft(5, '0') : opcode_s;
						binary_code.AppendLine("inst_memory[" + i + "] = 16'B" + opcode_s + optype1 + optype2 + optype3	+ op1 + ";");
						binary_code.AppendLine("inst_memory[" + (i + 1) + "] = 16'B" + op2 + op3 + ";");
						binary_code.AppendLine();
						i += 2;
					}
				}
			}

			string filename_out = filename + ".obj";
			File.WriteAllText(filename_out, binary_code.ToString());

		}

	}
}
