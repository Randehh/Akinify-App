using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Akinify_App {
	public class ProgressBarWrapper {

		private ProgressBar m_ProgressBar;
		private TextBlock m_ProgressText;
		private double m_TargetValue = 0;
		private Action m_OnUpdate;
		private List<ProgressBarWrapperTask> m_Tasks = new List<ProgressBarWrapperTask>();

		public bool IsCompleted => m_ProgressBar.Maximum != 0 && m_TargetValue == m_ProgressBar.Maximum;

		public ProgressBarWrapper(ProgressBar progressBar, TextBlock progressText, Action onUpdate) {
			m_ProgressBar = progressBar;
			m_ProgressText = progressText;
			m_OnUpdate = onUpdate;
			m_ProgressBar.Maximum = 0;

			DispatcherTimer dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += OnTick;
			dispatcherTimer.Interval = new TimeSpan(1L);
			dispatcherTimer.Start();
		}

		public void Reset() {
			m_Tasks.Clear();
			m_ProgressBar.Maximum = 0;
			m_TargetValue = -1;
		}

		public ProgressBarWrapperTask AddTask(int weight, int sectionCount) {
			m_ProgressBar.Maximum += weight;
			ProgressBarWrapperTask newTask = new ProgressBarWrapperTask(this, weight, sectionCount);
			m_Tasks.Add(newTask);
			return newTask;
		}

		public void Update() {
			float totalProgress = 0;
			foreach(ProgressBarWrapperTask task in m_Tasks) {
				totalProgress += task.Progress;
			}
			m_TargetValue = totalProgress;
			m_ProgressText.Text = (int)((m_TargetValue / m_ProgressBar.Maximum) * 100) + "%";
			m_OnUpdate();
		}

		private void OnTick(object sender, EventArgs e) {
			m_ProgressBar.Value = Lerp(m_ProgressBar.Value, m_TargetValue, 0.01f);
		}

		private double Lerp(double firstFloat, double secondFloat, float by) {
			return firstFloat * (1 - by) + secondFloat * by;
		}
	}
}
