﻿using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class Get4000LotsViewModel : ObservableObject
    {
        private readonly Tools _tools;
        private readonly IMessenger _messenger;
        private readonly DatabaseHelper _databaseHelper;
        private IPublicClientApplication _pca;
        private ICommand _get4000LotCertBillReportCommand;
        private string _reportTitle = string.Empty;
        private bool _isLoading;
        private string _lotNumber = string.Empty;

        public Get4000LotsViewModel(Tools tools, IMessenger messenger, IPublicClientApplication pca)
        {
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _pca = pca;
            _databaseHelper = new DatabaseHelper(_tools);
            _get4000LotCertBillReportCommand = new AsyncRelayCommand(ExecuteGet4000LotCertBillReportAsync);
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
        public string Get4000LotCertBillReportButtonContent => "Get 4000 Lot Cert Bill";
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
        public string LotNumber
        {
            get => _lotNumber;
            set
            {
                _lotNumber = value;
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
        private async Task ExecuteGet4000LotCertBillReportAsync()
        {
            ReportTitle = "4000 Lot Cert Bill";
            _messenger.Send(new ReportTitleMessage(ReportTitle));
            OnNavigate();
            await _databaseHelper.ExecuteDataGridCommandAsync(Get4000LotCertBillReport);
        }

        private async void Get4000LotCertBillReport()
        {
            Debug.WriteLine("Get4000LotCertBillReport executed");
            var dataTable = await _databaseHelper.ExecuteDatabaseCommandAsync("Get4000LotCertBillReport", new[] { new SqlParameter("@LotNumber", LotNumber) }, SetLoading);
            if (dataTable != null)
            {
                Process4000LotCertBillReportData(dataTable);
                _messenger.Send(new DataTableMessage(dataTable));

            }
        }

        private void SetLoading(bool isLoading)
        {
            IsLoading = isLoading;
            _messenger.Send(new LoadingMessage(isLoading));
        }
        #endregion

        #region Report Transformations
        private void Process4000LotCertBillReportData(DataTable dataTable)
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

        public ICommand Get4000LotCertBillReportCommand => _get4000LotCertBillReportCommand ??= new AsyncRelayCommand(ExecuteGet4000LotCertBillReportAsync);

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
