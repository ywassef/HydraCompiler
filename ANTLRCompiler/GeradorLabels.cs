using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLRCompiler
{
	class GeradorLabels
	{
		int index;
		
		public GeradorLabels()
		{
			index = 0;
		}

		public string GerarLabel()
		{
			string label =  index.ToString("000");
			index++;
			return "L" + label;
		}

		public string GerarLabelFuncao()
		{
			string label = index.ToString("000");
			index++;
			return "F" + label;
		}
	}
}
