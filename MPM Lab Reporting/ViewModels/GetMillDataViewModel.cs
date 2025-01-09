using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using MPM_Lab_Reporting.Helpers;
using MPM_Lab_Reporting.Messages;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MPM_Lab_Reporting.Views;

namespace MPM_Lab_Reporting.ViewModels
{
    public partial class GetMillDataViewModel : ObservableObject
    {
        private readonly Tools _tools;
        private readonly IMessenger _messenger;
        private readonly DatabaseHelper _databaseHelper;
        private IPublicClientApplication _pca;
        private ICommand _getXRFMillP22ConReportCommand;
        private ICommand _getXRFMillCompositeReportCommand;
        private string _reportTitle = string.Empty;
        private DateTime _selectedDate;
        private bool _isLoading;

        public GetMillDataViewModel(Tools tools, IMessenger messenger, IPublicClientApplication pca)
        {
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _pca = pca;
            _databaseHelper = new DatabaseHelper(_tools);
            _getXRFMillP22ConReportCommand = new AsyncRelayCommand(ExecuteGetXRFMillP22ConReportAsync);
            _getXRFMillCompositeReportCommand = new AsyncRelayCommand(ExecuteGetXRFMillCompositeReportAsync);
            SelectedDate = DateTime.Today;
            NavigateCommand = new Command(OnNavigate);
            InitializeAsync().ConfigureAwait(false);

        }
        public async Task InitializeAsync()
        {
            try
            {
                await _tools.SqlSetupAsync(_pca);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in InitializeAsync: {ex.Message}");
                throw;
            }
        }

        #region Variables
        public string GetXRFMillP22ConReportButtonContent => "Get Mill P22 Con Report";
        public string GetXRFMillCompositeReportButtonContent => "Get Mill Composite Report";

        public string ReportTitle
        {
            get => _reportTitle;
            set
            {
                _reportTitle = value;
                OnPropertyChanged();
                _messenger.Send(new ReportTitleMessage(_reportTitle));
            }
        }
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        #endregion

        #region Report Methods
        private async Task ExecuteGetXRFMillP22ConReportAsync()
        {
            ReportTitle = "Mill P22 Con Report";
            _messenger.Send(new ReportTitleMessage(ReportTitle));
            OnNavigate();
            await _databaseHelper.ExecuteDataGridCommandAsync(GetXRFMillP22ConReport);
        }

        private async Task ExecuteGetXRFMillCompositeReportAsync()
        {
            ReportTitle = "Mill Composite Report";
            _messenger.Send(new ReportTitleMessage(ReportTitle));
            OnNavigate();
            await _databaseHelper.ExecuteDataGridCommandAsync(() => GetXRFMillCompositeReport(100));
        }

        private async void GetXRFMillP22ConReport()
        {
            Debug.WriteLine("GetXRFMillP22ConReport executed");
            var dataTable = await _databaseHelper.ExecuteDatabaseCommandAsync("GetXRFMillP22ConReport", new[] { new SqlParameter("@StartingDate", SelectedDate) }, SetLoading);
            if (dataTable != null)
            {
                ProcessXRFMillP22ConReportData(dataTable);
                _messenger.Send(new DataTableMessage(dataTable));

            }
        }
        private async void GetXRFMillCompositeReport(int count)
        {
            Debug.WriteLine("GetXRFMillCompositeReport executed");
            var dataTable = await _databaseHelper.ExecuteDatabaseCommandAsync("GetXRFMillCompositeReport", null, SetLoading);
            if (dataTable != null)
            {
                var limitedDataTable = dataTable.AsEnumerable().Take(count).CopyToDataTable();
                _messenger.Send(new DataTableMessage(limitedDataTable));
            }
        }

        private void SetLoading(bool isLoading)
        {
            IsLoading = isLoading;
            _messenger.Send(new LoadingMessage(isLoading));
        }
        #endregion

        #region Report Transformations
        private void ProcessXRFMillP22ConReportData(DataTable dataTable)
        {
            if (dataTable.Rows.Count > 2)
            {
                // Clone the last two rows for the summary
                DataRow lastRow1 = dataTable.Rows[dataTable.Rows.Count - 2];
                DataRow lastRow2 = dataTable.Rows[dataTable.Rows.Count - 1];

                if (lastRow1.ItemArray != null && lastRow2.ItemArray != null)
                {
                    DataRow summaryRow1 = dataTable.NewRow();
                    summaryRow1.ItemArray = lastRow1.ItemArray.Clone() as object[];
                    DataRow summaryRow2 = dataTable.NewRow();
                    summaryRow2.ItemArray = lastRow2.ItemArray.Clone() as object[];

                    // Remove the last two rows from the main data table
                    dataTable.Rows.RemoveAt(dataTable.Rows.Count - 1);
                    dataTable.Rows.RemoveAt(dataTable.Rows.Count - 1);

                    // Insert the summary rows at the top of the main data table
                    dataTable.Rows.InsertAt(summaryRow2, 0);
                    dataTable.Rows.InsertAt(summaryRow1, 0);
                }
            }
        }
        #endregion

        #region Commands
        public ICommand? NavigateCommand { get; }

        public ICommand GetXRFMillP22ConReportCommand => _getXRFMillP22ConReportCommand ??= new AsyncRelayCommand(ExecuteGetXRFMillP22ConReportAsync);
        public ICommand GetXRFMillCompositeReportCommand => _getXRFMillCompositeReportCommand ??= new AsyncRelayCommand(ExecuteGetXRFMillCompositeReportAsync);

        #endregion

        private async void OnNavigate()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.Navigation.PushAsync(new GridDataView());
                }
                else
                {
                    Debug.WriteLine("MainPage is null.");
                }
            });
        }
    }
}
