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

namespace MPM_Lab_Reporting.ViewModels
{
    public class GridDataViewModel : ObservableObject
    {
        private readonly Tools _tools;
        private readonly IMessenger _messenger;
        private IPublicClientApplication _pca;
        private DataTable _gridDataTable = new DataTable();
        private ICommand _getXRFMillP22ConReportCommand;
        private ICommand _getRmaGridCommand;
        private ICommand _getRecGridCommand;
        private ICommand _getDecomGridCommand;
        private ICommand _exportCommand;
        private ICommand _exportToPDFCommand;
        private ICommand _get4000LotsFlyoutCommand;
        private string _reportTitle = string.Empty;
        private DateTime _selectedDate;
        private int _errorCounter = 0;
        private string _searchButtonContent = string.Empty;
        private bool _searchTermTextBoxVisible;

        private ObservableCollection<string> _searchParameterCollection = new ObservableCollection<string>();


        public GridDataViewModel(Tools tools, IMessenger messenger, IPublicClientApplication pca)
        {
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _pca = pca;
            //_getRmaGridCommand = new Command(async () => await ExecuteDataGridCommand(GetRmaGrid));
            //_getRecGridCommand = new Command(async () => await ExecuteDataGridCommand(GetRecGrid));
            //_getDecomGridCommand = new Command(async () => await ExecuteDataGridCommand(GetDecomGrid));
            _exportCommand = new Command(async () => await ExportGridClick());
            _get4000LotsFlyoutCommand = new AsyncRelayCommand(Open4000LotsFlyoutAsync);
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

        public bool SearchTermTextBoxVisible
        {
            get => _searchTermTextBoxVisible;
            set => SetProperty(ref _searchTermTextBoxVisible, value);
        }

        #endregion

        #region ObservableCollections


        public ObservableCollection<string> SearchParameterCollection
        {
            get => _searchParameterCollection;
            set => SetProperty(ref _searchParameterCollection, value);
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

        private void UpdateVisibilityProperties()
        {
        }

        private async Task ExportGridClick()
        {
            try
            {
                await Task.Run(ExportGrid);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ExportGridClick: {ex.Message}");
                throw;
            }
        }
        private async void ExportGrid()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } }, // or "public.csv"
                    { DevicePlatform.Android, new[] { "text/csv" } },
                    { DevicePlatform.WinUI, new[] { ".csv" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text" } }, // or "public.csv"
                    { DevicePlatform.Tizen, new[] { "text/csv" } },
                });
                var options = new PickOptions
                {
                    PickerTitle = "Save Grid Data",
                    FileTypes = customFileType
                };

                var result = await FilePicker.Default.PickAsync(options);

                if (result != null)
                {
                    string excelFilePath = result.FullPath;
                    await _tools.ShowMessageAsync("Information", "Export Complete");
                }
                else
                {
                    await _tools.ShowMessageAsync("Information", "Export Canceled");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ExportGrid: {ex.Message}");
                throw;
            }
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

        private async Task Open4000LotsFlyoutAsync()
        {
            await Shell.Current.GoToAsync(nameof(Get4000LotsView));
        }

        private ICommand _clearDataGridCommand;

        public ICommand ClearDataGridCommand => _clearDataGridCommand ??= new RelayCommand(ClearDataGrid);

        private void ClearDataGrid()
        {
            GridDataTable.Clear();
        }

        private void GetRmaGrid()
        {
            _errorCounter = 0;
            //ExecuteDatabaseCommand("RMAReport", null);
        }

        private void GetRecGrid()
        {
            _errorCounter = 0;
            //ExecuteDatabaseCommand("RECReport", new[] { new SqlParameter("@Rec", "RECYCLED       ") });
        }

        private void GetDecomGrid()
        {
            _errorCounter = 0;
            //ExecuteDatabaseCommand("DecomReport", null);
        }

        #endregion

        #region Commands
        //public ICommand GetXRFMillP22ConReportCommand => _getXRFMillP22ConReportCommand ??= new Command(async () => await ExecuteDataGridCommand(GetXRFMillP22ConReport));
        //public ICommand GetRmaGridCommand => _getRmaGridCommand ??= new Command(async () => await ExecuteDataGridCommand(GetRmaGrid));
        //public ICommand GetRecGridCommand => _getRecGridCommand ??= new Command(async () => await ExecuteDataGridCommand(GetRecGrid));
        //public ICommand GetDecomGridCommand => _getDecomGridCommand ??= new Command(async () => await ExecuteDataGridCommand(GetDecomGrid));

        public ICommand ExportCommand => _exportCommand ??= new Command(async () => await ExportGridClick());
        public ICommand ExportToPDFCommand => _exportToPDFCommand ??= new Command(ExportToPDF);

        public ICommand? NavigateCommand { get; }
        #endregion

    }
}
