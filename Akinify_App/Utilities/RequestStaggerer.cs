using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akinify_App {

	/// <summary>
	/// Ensures a consistent delay between each consecutive request to not overload the API
	/// </summary>
	public class RequestStaggerer {
		private int m_CurrentWait = 0;
		public int GetNextDelay() {
			m_CurrentWait += 200;
			return m_CurrentWait;
		}

		public void Reset() {
			m_CurrentWait = 0;
		}
	}
}
