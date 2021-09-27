using System;

namespace Akinify_App {
	public class TextInputDialogVM : BaseVM {

		public Action OnRequestClose { get; set; } = delegate { };

		private bool m_TextInputSuccesful = false;

		private string m_HeaderText = "";
		public string HeaderText {
			get => m_HeaderText;
			set => SetProperty(ref m_HeaderText, value);
		}

		private string m_Description = "";
		public string Description {
			get => m_Description;
			set => SetProperty(ref m_Description, value);
		}

		private string m_InputText = "";
		public string InputText {
			get => m_InputText;
			set => SetProperty(ref m_InputText, value);
		}

		public string ResultText => m_TextInputSuccesful ? InputText : "";

		public SimpleCommand ConfirmCommand { get; set; }

		public TextInputDialogVM(string headerText, string description, Func<string, bool> prerequisite = null) {
			HeaderText = headerText;
			Description = description;
			ConfirmCommand = new SimpleCommand((_) => {
				m_TextInputSuccesful = true;
				OnRequestClose();
			},
			() => {
				return prerequisite != null ? prerequisite(InputText) : true;
			});
		}
	}
}
