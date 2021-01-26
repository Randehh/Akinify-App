using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akinify_App {
	public class VisualLogger {

		public Action<string> OnUpdated = delegate { };

		private StringBuilder m_StringBuilder = new StringBuilder();

		public VisualLogger() {

		}

		public void AddLine(string s) {
			m_StringBuilder.Append("\n");
			m_StringBuilder.Append(s);
			OnUpdated(m_StringBuilder.ToString());
		}
	}
}
