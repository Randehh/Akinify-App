using System;

namespace Akinify_App {
	public class ProgressBarWrapperTask {

		public float Progress => ((float)m_SectionsCompleted / m_SectionCount) * m_TaskWeight;

		private ProgressBarWrapper m_Parent;
		private float m_TaskWeight = 0;
		private int m_SectionCount = 0;

		private int m_SectionsCompleted = 0;

		public ProgressBarWrapperTask(ProgressBarWrapper parent, float taskWeight, int sectionCount) {
			m_Parent = parent;
			m_TaskWeight = taskWeight;
			m_SectionCount = sectionCount;
		}

		public void UpdateSectionCount(int sectionCount) {
			m_SectionCount = sectionCount;
			CompleteSection(0);
		}

		public void CompleteSection(int sectionCount = 1) {
			m_SectionsCompleted = Math.Min(m_SectionsCompleted + sectionCount, m_SectionCount);
			m_Parent.Update();
		}

		public void ForceComplete() {
			CompleteSection(m_SectionCount);
		}
	}
}
