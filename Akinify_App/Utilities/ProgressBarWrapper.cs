using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Akinify_App {
	public class ProgressBarWrapper {

		private ProgressBar m_ProgressBar;
		private TextBlock m_ProgressText;
		private double m_TargetValue = 0;
		private Action m_OnTaskCompleted;

		public bool IsCompleted => m_ProgressBar.Maximum != 0 && m_TargetValue == m_ProgressBar.Maximum;

		public ProgressBarWrapper(ProgressBar progressBar, TextBlock progressText, Action onTaskCompleted) {
			m_ProgressBar = progressBar;
			m_ProgressText = progressText;
			m_OnTaskCompleted = onTaskCompleted;
			m_ProgressBar.Maximum = 0;

			DispatcherTimer dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += OnTick;
			dispatcherTimer.Interval = new TimeSpan(1L);
			dispatcherTimer.Start();
		}

		public void Reset() {
			m_ProgressBar.Maximum = 0;
			m_TargetValue = 0;
		}

		public void AddTask(int weight) {
			m_ProgressBar.Maximum += weight;
		}

		public void CompleteTask(int weight) {
			m_TargetValue += weight;
			m_ProgressText.Text = (int)((m_TargetValue / m_ProgressBar.Maximum) * 100) + "%";
			m_OnTaskCompleted();
		}

		private void OnTick(object sender, EventArgs e) {
			m_ProgressBar.Value = Lerp(m_ProgressBar.Value, m_TargetValue, 0.1f);
		}

		private double Lerp(double firstFloat, double secondFloat, float by) {
			return firstFloat * (1 - by) + secondFloat * by;
		}
	}
}
