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
			if (m_StringBuilder.Length != 0) {
				m_StringBuilder.Append("\n");
			}
			m_StringBuilder.Append($"{GetTimeString()} - {s}");
			OnUpdated(m_StringBuilder.ToString());
		}

		public string GetTimeString() {
			StringBuilder sb = new StringBuilder();
			DateTime time = DateTime.Now;
			sb.Append(time.Hour < 10 ? $"0{time.Hour}:" : $"{time.Hour}:");
			sb.Append(time.Minute < 10 ? $"0{time.Minute}:" : $"{time.Minute}:");
			sb.Append(time.Second < 10 ? $"0{time.Second}" : $"{time.Second}");
			return sb.ToString();
		}
	}
}
