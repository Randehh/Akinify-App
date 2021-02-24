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
		public static int STAGGER_TIME = 200;

		private int m_CurrentWait = 0;
		public int CurrentWait {
			get { return m_CurrentWait; }
			private set { m_CurrentWait = value; }
		}

		public int GetNextDelay() {
			CurrentWait += STAGGER_TIME;
			return CurrentWait;
		}

		public void Reset() {
			CurrentWait = 0;
		}
	}
}
