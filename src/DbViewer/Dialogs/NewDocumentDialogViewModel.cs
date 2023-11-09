using Prism.Commands;
using Prism.Services.Dialogs;
using ReactiveUI;
using System;

namespace DbViewer.Dialogs
{
	public class NewDocumentDialogViewModel : ReactiveObject, IDialogAware
	{
		public NewDocumentDialogViewModel()
		{
			CloseCommand = new DelegateCommand(() => RequestClose?.Invoke(null));
			CreateCommand = new DelegateCommand(ExecuteCreate);
		}

		public event Action<IDialogParameters> RequestClose;

		public DelegateCommand CloseCommand { get; }
		public DelegateCommand CreateCommand { get; }

		public string DocumentKey
		{
			get => _documentKey;
			set => this.RaiseAndSetIfChanged(ref _documentKey, value);
		}

		public string DocumentBody
		{
			get => _documentBody;
			set => this.RaiseAndSetIfChanged(ref _documentBody, value);
		}

		public bool CanCloseDialog() => true;

		public void OnDialogClosed()
		{
		}

		public void OnDialogOpened(IDialogParameters parameters)
		{

		}

		private void ExecuteCreate()
		{
			var dialogResult = new DialogParameters
			{
				{ DialogNames.DocumentKeyParam, DocumentKey },
				{ DialogNames.DocumentBodyParam, DocumentBody }
			};

			RequestClose?.Invoke(dialogResult);
		}

		private string _documentKey;
		private string _documentBody;
	}
}
