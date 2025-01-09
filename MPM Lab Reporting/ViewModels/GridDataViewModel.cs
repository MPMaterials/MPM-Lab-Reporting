using System;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Input;
using MPM_Lab_Reporting.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using Microsoft.Identity.Client;
using MPM_Lab_Reporting.Views;
using DocumentFormat.OpenXml.Drawing;
using Syncfusion.Maui.DataGrid.Exporting;
using Syncfusion.Pdf;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Pdf.Grid;
using CommunityToolkit.Mvvm.Input;
using MPM_Lab_Reporting.Messages;
using Syncfusion.Maui.Data;

namespace MPM_Lab_Reporting.ViewModels
{
    public class GridDataViewModel : ObservableObject, ISupportIncrementalLoading
    {
        private readonly Tools _tools;
        private readonly IMessenger _messenger;
        private IPublicClientApplication _pca;
        private DataTable _gridDataTable = new DataTable();
        private ICommand _exportCommand;
        private ICommand _exportToPDFCommand;
        private ICommand _get4000LotsFlyoutCommand;
        private ICommand _getMillDataFlyoutCommand;
        private ICommand _clearDataGridCommand;
        private ICommand _loadAllItemsCommand;
        private ICommand _loadMoreItemsCommand;
        private string _reportTitle = string.Empty;
        private DateTime _selectedDate;
        private int _errorCounter = 0;
        private string _searchButtonContent = string.Empty;
        private bool _hasMoreItems = true;
        private int _itemsPerPage = 100;
        private DatabaseHelper _databaseHelper;

        public GridDataViewModel(Tools tools, IMessenger messenger, IPublicClientApplication pca)
        {
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _pca = pca;
            _databaseHelper = new DatabaseHelper(_tools);
            _get4000LotsFlyoutCommand = new AsyncRelayCommand(Open4000LotsFlyoutAsync);
            _getMillDataFlyoutCommand = new AsyncRelayCommand(OpenMillDataFlyoutAsync);
            _exportToPDFCommand = new Command(ExportToPDF);
            NavigateCommand = new Command(OnNavigate);
            GridDataTable = new DataTable();
            InitializeAsync().ConfigureAwait(false);

            _messenger.Register<DataTableMessage>(this, (r, m) =>
            {
                GridDataTable = m.Value;
            });
            _messenger.Register<LoadingMessage>(this, (r, m) =>
            {
                IsLoading = m.IsLoading;
            });
            _messenger.Register<ReportTitleMessage>(this, (r, m) =>
            {
                ReportTitle = m.ReportTitle;
            });
        }

        public DataTable GridDataTable
        {
            get => _gridDataTable;
            set
            {
                _gridDataTable = value;
                OnPropertyChanged();
            }
        }
        private SfDataGrid? _dataGrid;
        public SfDataGrid? DataGrid
        {
            get => _dataGrid;
            set
            {
                _dataGrid = value;
                OnPropertyChanged();
            }
        }

        #region Initialize
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
        #endregion

        #region Variables
        public bool HasMoreItems => _hasMoreItems;
        public string Get4000LotsButtonContent => "4000 Lots";
        public string GetReports1ButtonContent => "Reports 1";
        public string GetReports2ButtonContent => "Reports 2";
        public string GetReports3ButtonContent => "Reports 3";
        public string GetMillDataButtonContent => "Mill Data";
        public string GetMineDataButtonContent => "Mine Data";
        public string GetQCSampleDataButtonContent => "QC Sample Data";

        public string ExportButtonContent => "export to csv";

        public string ReportTitle
        {
            get => _reportTitle;
            set
            {
                _reportTitle = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        public string SearchButtonContent
        {
            get => _searchButtonContent;
            set
            {
                _searchButtonContent = value;
                OnPropertyChanged();
            }
        }

        #endregion
        private void SetLoading(bool isLoading)
        {
            IsLoading = isLoading;
        }
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
        public void LoadMoreItemsAsync(uint count)
        {
            _ = LoadMoreItemsInternalAsync(count);
        }
        private async Task LoadMoreItemsInternalAsync(uint count)
        {
            try
            {
                // Load the next set of items
                var newItems = await LoadDataAsync((int)count);
                foreach (var item in newItems)
                {
                    var newRow = GridDataTable.NewRow();
                    newRow.ItemArray = item.ItemArray;
                    GridDataTable.Rows.Add(newRow);
                }

                // Check if there are more items to load
                _hasMoreItems = newItems.Count >= _itemsPerPage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in LoadMoreItemsAsync: {ex.Message}");
                _hasMoreItems = false;
            }
        }
        private async void LoadAllItems()
        {
            try
            {
                var allItems = await LoadDataAsync(int.MaxValue);
                foreach (var item in allItems)
                {
                    var newRow = GridDataTable.NewRow();
                    newRow.ItemArray = item.ItemArray;
                    GridDataTable.Rows.Add(newRow);
                }

                _hasMoreItems = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in LoadAllItems: {ex.Message}");
            }
        }

        private async Task<List<System.Data.DataRow>> LoadDataAsync(int count)
        {
            var dataTable = await _databaseHelper.ExecuteDatabaseCommandAsync("GetXRFMillCompositeReport", null, SetLoading);
            return dataTable.AsEnumerable().Skip(GridDataTable.Rows.Count).Take(count).ToList();
        }

        private void ExportToPDF()
        {
            MemoryStream stream = new MemoryStream();
            PdfDocument pdfDoc = new PdfDocument();
            PdfPage pdfPage = pdfDoc.Pages.Add();
            PdfGrid pdfGrid = new PdfGrid();
            pdfGrid.DataSource = GridDataTable;
            pdfGrid.Draw(pdfPage, new Syncfusion.Drawing.PointF(0, 0));
            pdfDoc.Save(stream);
            pdfDoc.Close(true);
            SaveService saveService = new();
            saveService.SaveAndView("ExportFeature.pdf", "application/pdf", stream);
        }

        public IAsyncRelayCommand Get4000LotsFlyoutCommand => (IAsyncRelayCommand)_get4000LotsFlyoutCommand;
        public IAsyncRelayCommand GetMillDataFlyoutCommand => (IAsyncRelayCommand)_getMillDataFlyoutCommand;


        private async Task Open4000LotsFlyoutAsync()
        {
            await Shell.Current.GoToAsync(nameof(Get4000LotsView));
        }
        private async Task OpenMillDataFlyoutAsync()
        {
            await Shell.Current.GoToAsync(nameof(GetMillDataView));
        }



        private void ClearDataGrid()
        {
            GridDataTable.Clear();
        }


        #region Commands
        public ICommand ExportToPDFCommand => _exportToPDFCommand ??= new Command(ExportToPDF);
        public ICommand? NavigateCommand { get; }
        public ICommand LoadMoreItemsCommand => new RelayCommand<uint>(LoadMoreItemsAsync);
        public ICommand LoadAllItemsCommand => new RelayCommand(LoadAllItems);
        public ICommand ClearDataGridCommand => _clearDataGridCommand ??= new RelayCommand(ClearDataGrid);

        #endregion

    }
}
