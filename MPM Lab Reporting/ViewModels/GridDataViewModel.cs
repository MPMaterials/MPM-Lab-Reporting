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
//using Microsoft.UI.Xaml.Controls;
using DocumentFormat.OpenXml.Drawing;
using Syncfusion.Maui.DataGrid.Exporting;
using Syncfusion.Pdf;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Pdf.Grid;

namespace MPM_Lab_Reporting.ViewModels
{
    public class GridDataViewModel : ObservableObject
    {
        private readonly Tools _tools;
        private readonly IMessenger _messenger;
        private IPublicClientApplication _pca;
        private int _errorCounter = 0;
        private DataTable _gridDataTable = new DataTable();
        private ICommand _gridFilterFlyoutCommand;
        private ICommand _getDataGridCommand;
        private ICommand _getRmaGridCommand;
        private ICommand _getRecGridCommand;
        private ICommand _getDecomGridCommand;
        private ICommand _exportCommand;
        private ICommand _gridFilterCommand;
        private ICommand _exportToPDFCommand;
        private string _searchText = string.Empty;
        private string _searchParameter = string.Empty;
        private string _reportTitle = string.Empty;

        private string _searchButtonContent = string.Empty;
        private bool _searchTermTextBoxVisible;

        private ObservableCollection<string> _searchParameterCollection = new ObservableCollection<string>();


        public GridDataViewModel(Tools tools, IMessenger messenger, IPublicClientApplication pca)
        {
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _pca = pca;
            _gridFilterFlyoutCommand = new Command(OpenGridFilterFlyout);
            _getDataGridCommand = new Command(async () => await ExecuteDataGridCommand(GetDataGrid));
            _getRmaGridCommand = new Command(async () => await ExecuteDataGridCommand(GetRmaGrid));
            _getRecGridCommand = new Command(async () => await ExecuteDataGridCommand(GetRecGrid));
            _getDecomGridCommand = new Command(async () => await ExecuteDataGridCommand(GetDecomGrid));
            _exportCommand = new Command(async () => await ExportGridClick());
            _gridFilterCommand = new Command(async () => await ExecuteDataGridCommand(SearchGrid));
            _exportToPDFCommand = new Command(ExportToPDF);
            NavigateCommand = new Command(OnNavigate);
            GridDataTable = new DataTable();
            InitializeAsync().ConfigureAwait(false);
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
                await OnLoadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in InitializeAsync: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Variables
        public string GetDataButtonContent => "Get Mill P22 Con Report";
        public string GetRmaButtonContent => "RMA Report";
        public string GetRecButtonContent => "Recycled Report";
        public string GetDecomButtonContent => "Decomissioned Report";
        public string ExportButtonContent => "export to csv";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

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
        public string SearchParameter
        {
            get => _searchParameter;
            set
            {
                if (SetProperty(ref _searchParameter, value))
                {
                    SearchText = string.Empty;
                    switch (_searchParameter)
                    {
                        case "Finance Report Summaries":
                            SearchButtonContent = "Export to Excel";
                            break;
                        case "Building":
                            break;
                        case "Location":
                           
                            break;
                        // Add other cases as needed
                        default:
                            SearchButtonContent = "Search";
                            break;
                    }

                    UpdateVisibilityProperties();
                }
            }
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
        private async Task OnLoadAsync()
        {
            SearchParameterCollection = new ObservableCollection<string>
            {
                "Asset Type",
                "Building",
                "Custodian",
                "Description",
                "Department",
                "Location",
                "Manufacturer",
                "Model",
                "Serial Number",
                "Status",
                "Purchase Order",
                "Year Purchased",
                "Finance Report Breakdown",
                "Finance Report Summaries"
            };

            SearchTermTextBoxVisible = false;

            SearchButtonContent = "Search";

            await LoadDataInBackgroundAsync();
        }

        private async Task LoadDataInBackgroundAsync()
        {
            await Task.Run(async () =>
            {

            });
        }
        private void UpdateVisibilityProperties()
        {
            SearchTermTextBoxVisible = SearchParameter.Equals("Description") || SearchParameter.Equals("Serial Number") || SearchParameter.Equals("Purchase Order") || SearchParameter.Equals("Year Purchased");
        }
        private async void ExecuteDatabaseCommand(string storedProcedure, SqlParameter[]? parameters)
        {
            _gridDataTable = new DataTable();
            _errorCounter = 0;
            IsLoading = true;
            try
            {
                await _tools.EnsureConnectionOpenAsync();
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                SqlCommand cmd = new SqlCommand(storedProcedure, _tools.Conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(_gridDataTable);

                switch (storedProcedure){
                    case "GetXRFMillP22ConReport":
                        ReportTitle = "Mill P22 Con Report";
                        if (_gridDataTable.Rows.Count > 2)
                        {
                            // Clone the last two rows for the summary
                            System.Data.DataRow summaryRow1 = _gridDataTable.NewRow();
                            summaryRow1.ItemArray = _gridDataTable.Rows[_gridDataTable.Rows.Count - 2].ItemArray.Clone() as object[];
                            System.Data.DataRow summaryRow2 = _gridDataTable.NewRow();
                            summaryRow2.ItemArray = _gridDataTable.Rows[_gridDataTable.Rows.Count - 1].ItemArray.Clone() as object[];

                            // Remove the last two rows from the main data table
                            _gridDataTable.Rows.RemoveAt(_gridDataTable.Rows.Count - 1);
                            _gridDataTable.Rows.RemoveAt(_gridDataTable.Rows.Count - 1);

                            // Insert the summary rows at the top of the main data table
                            _gridDataTable.Rows.InsertAt(summaryRow2, 0);
                            _gridDataTable.Rows.InsertAt(summaryRow1, 0);
                        }
                        GridDataTable = _gridDataTable;

                        if (_gridDataTable.Rows.Count == 0)
                        {
                            // Handle the case where no data is returned
                            Debug.WriteLine("No data returned from the stored procedure.");
                            await _tools.ShowMessageAsync("No Data", "No entries found.");
                        }
                        else
                        {
                            OnPropertyChanged(nameof(GridDataTable));
                        }
                        break;
                    case "GetFinanceReport":
                        var result = await FilePicker.Default.PickAsync(new PickOptions
                        {
                            PickerTitle = "Save Finance Report",
                            FileTypes = FilePickerFileType.Pdf
                        });
                        if (result != null)
                        {
                            string excelFilePath = result.FullPath;
                            await _tools.ShowMessageAsync("Information", "Export Complete");
                        }
                        else
                        {
                            await _tools.ShowMessageAsync("Information", "Export Canceled");
                        }
                        break;
                    default:
                        GridDataTable = _gridDataTable;
                        if (_gridDataTable.Rows.Count == 0)
                        {
                            // Handle the case where no data is returned
                            Debug.WriteLine("No data returned from the stored procedure.");
                            await _tools.ShowMessageAsync("No Data", "No entries found.");
                        }
                        else
                        {
                            OnPropertyChanged(nameof(GridDataTable));
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception in ExecuteDatabaseCommand: {e.Message}");
                ErrorCatch.Error(e);
                _errorCounter++;
            }
            finally
            {
                _tools.Conn?.Close();
                IsLoading = false;
            }
        }
        private async Task ExecuteDataGridCommand(Action action)
        {
            try
            {
                await Task.Run(action);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ExecuteDataGridCommand: {ex.Message}");
                throw;
            }
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


        private async void SearchGrid()
        {
            SqlParameter[] parameters = Array.Empty<SqlParameter>();
            string storedProcedure = string.Empty;

            switch (SearchParameter)
            {
                case "Building":
                    storedProcedure = "SearchGrid_Building";
                    parameters = new[] { new SqlParameter("@Building", SearchText) };
                    break;
                case "Location":
                    storedProcedure = "SearchGrid_Location";
                    parameters = new[]
{
                        new SqlParameter("@Location", SearchText)
                    };
                    break;
                case "Description":
                    storedProcedure = "SearchGrid_Description";
                    parameters = new[] { new SqlParameter("@Description", SearchText) };
                    break;
                case "Department":
                    storedProcedure = "SearchGrid_Department";
                    parameters = new[] { new SqlParameter("@Description", SearchText) };
                    break;
                case "Manufacturer":
                    storedProcedure = "SearchGrid_Manufacturer";
                    parameters = new[] { new SqlParameter("@Manufacturer", SearchText) };
                    break;
                case "Model":
                    storedProcedure = "SearchGrid_Model";
                    parameters = new[] { new SqlParameter("@Model", SearchText) };
                    break;
                case "Asset Type":
                    storedProcedure = "SearchGrid_AssetType";
                    parameters = new[] { new SqlParameter("@AssetType", SearchText) };
                    break;
                case "Status":
                    storedProcedure = "SearchGrid_Status";
                    parameters = new[] { new SqlParameter("@Description", SearchText) };
                    break;
                case "Custodian":
                    storedProcedure = "SearchGrid_Custodian";
                    string[] custodian = SearchText.Split(' ');
                    string custodianRemainder = custodian[1];
                    string[] custRemain = custodianRemainder.Split('\t');
                    string custodianId = custRemain[1];
                    parameters = new[] { new SqlParameter("@CustodianID", custodianId) };
                    break;
                case "Serial Number":
                    storedProcedure = "SearchGrid_SerialNumber";
                    parameters = new[] { new SqlParameter("@SerialNumber", SearchText) };
                    break;
                case "Purchase Order":
                    storedProcedure = "SearchGrid_PurchaseOrder";
                    parameters = new[] { new SqlParameter("@PurchaseOrder", SearchText) };
                    break;
                case "Year Purchased":
                    storedProcedure = "SearchGrid_AcquisitionDate";
                    parameters = new[] { new SqlParameter("@AcquisitionDate", SearchText) };
                    break;
                case "Finance Report Breakdown":
                    storedProcedure = "SearchGrid_FinanceReport";
                    parameters = new[]
                    {
                        new SqlParameter("@AssetType", SearchText)
                    };
                    break;
                case "Finance Report Summaries":
                    storedProcedure = "GetFinanceReport";
                    parameters = new[]
                    {
                        new SqlParameter("@AssetType", SearchText)
                    };
                    break;
            }
            await Task.Run(() => ExecuteDatabaseCommand(storedProcedure, parameters));
            // Navigate back to GridDataView.xaml
            OnNavigate();
        }

        private async void OpenGridFilterFlyout()
        {
            IsGridFilterFlyoutOpen = true;
            await Shell.Current.GoToAsync("GridFilterView");
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

        private void GetDataGrid()
        {
            Debug.WriteLine("GetDataGridCommand executed");
            ExecuteDatabaseCommand("GetXRFMillP22ConReport", new[] { new SqlParameter("@StartingDate", "1/2/2025") });
        }

        private void GetRmaGrid()
        {
            _errorCounter = 0;
            ExecuteDatabaseCommand("RMAReport", null);
        }

        private void GetRecGrid()
        {
            _errorCounter = 0;
            ExecuteDatabaseCommand("RECReport", new[] { new SqlParameter("@Rec", "RECYCLED       ") });
        }

        private void GetDecomGrid()
        {
            _errorCounter = 0;
            ExecuteDatabaseCommand("DecomReport", null);
        }


        #endregion

        #region Commands
        public ICommand GridFilterFlyoutCommand => _gridFilterFlyoutCommand ??= new Command(OpenGridFilterFlyout);
        public ICommand GetDataGridCommand => _getDataGridCommand ??= new Command(async () => await ExecuteDataGridCommand(GetDataGrid));
        public ICommand GetRmaGridCommand => _getRmaGridCommand ??= new Command(async () => await ExecuteDataGridCommand(GetRmaGrid));
        public ICommand GetRecGridCommand => _getRecGridCommand ??= new Command(async () => await ExecuteDataGridCommand(GetRecGrid));
        public ICommand GetDecomGridCommand => _getDecomGridCommand ??= new Command(async () => await ExecuteDataGridCommand(GetDecomGrid));

        public ICommand ExportCommand => _exportCommand ??= new Command(async () => await ExportGridClick());
        public ICommand GridFilterCommand => _gridFilterCommand ??= new Command(async () => await ExecuteDataGridCommand(SearchGrid));
        public ICommand ExportToPDFCommand => _exportToPDFCommand ??= new Command(ExportToPDF);

        public ICommand? NavigateCommand { get; }
        #endregion

        #region GridFilterFlyout
        private bool _isGridFilterFlyoutOpen;
        public bool IsGridFilterFlyoutOpen
        {
            get => _isGridFilterFlyoutOpen;
            set
            {
                if (value.Equals(_isGridFilterFlyoutOpen)) return;
                _isGridFilterFlyoutOpen = value;
                OnPropertyChanged(nameof(IsGridFilterFlyoutOpen));
            }
        }
        #endregion
    }
}
