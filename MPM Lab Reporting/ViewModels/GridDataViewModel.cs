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
        private string _searchText = string.Empty;
        private string _searchParameter = string.Empty;
        private string _searchTextYear = string.Empty;
        private string _searchButtonContent = string.Empty;
        private bool _searchTermTextBoxVisible;
        private bool _buildingComboVisible;
        private bool _modelComboVisible;
        private bool _gridlocationComboBoxVisible;
        private bool _deptComboVisible;
        private bool _manComboVisible;
        private bool _statusComboVisible;
        private bool _assetTypeComboVisible;
        private bool _custodianComboVisible;
        private bool _yearsComboVisible;
        private string _building = string.Empty;
        private string _model = string.Empty;
        private string _gridlocation = string.Empty;
        private string _department = string.Empty;
        private string _manufacturer = string.Empty;
        private string _status = string.Empty;
        private string _assetType = string.Empty;
        private string _custodian = string.Empty;
        private string _year = string.Empty;
        private ObservableCollection<string> _searchParameterCollection = new ObservableCollection<string>();
        private ObservableCollection<string> _buildings = new ObservableCollection<string>();
        private ObservableCollection<string> _models = new ObservableCollection<string>();
        private ObservableCollection<string> _gridlocations = new ObservableCollection<string>();
        private ObservableCollection<string> _departments = new ObservableCollection<string>();
        private ObservableCollection<string> _manufacturerNames = new ObservableCollection<string>();
        private ObservableCollection<string> _statuses = new ObservableCollection<string>();
        private ObservableCollection<string> _assetTypes = new ObservableCollection<string>();
        private ObservableCollection<string> _custodians = new ObservableCollection<string>();
        private ObservableCollection<string> _years = new ObservableCollection<string>();

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
        public string Year
        {
            get => _year;
            set
            {
                _year = value;
                OnPropertyChanged(nameof(Year));
                SearchTextYear = Year;
            }
        }
        public bool YearsComboVisible
        {
            get => _yearsComboVisible;
            set
            {
                _yearsComboVisible = value;
                OnPropertyChanged(nameof(YearsComboVisible));
            }
        }
        public string SearchTextYear
        {
            get => _searchTextYear;
            set
            {
                _searchTextYear = value;
                OnPropertyChanged();
            }
        }
        public string Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged(nameof(Model));
                SearchText = Model;
            }
        }
        public bool ModelComboVisible
        {
            get => _modelComboVisible;
            set
            {
                _modelComboVisible = value;
                OnPropertyChanged(nameof(ModelComboVisible));
            }
        }
        public bool GridLocationComboBoxVisible
        {
            get => _gridlocationComboBoxVisible;
            set
            {
                _gridlocationComboBoxVisible = value;
                OnPropertyChanged(nameof(GridLocationComboBoxVisible));
            }
        }
        public string GridLocation
        {
            get => _gridlocation;
            set
            {
                _gridlocation = value;
                OnPropertyChanged(nameof(GridLocation));
                SearchText = GridLocation;
            }
        }
        public bool DeptComboVisible
        {
            get => _deptComboVisible;
            set
            {
                _deptComboVisible = value;
                OnPropertyChanged(nameof(DeptComboVisible));
            }
        }
        public string Department
        {
            get => _department;
            set
            {
                _department = value;
                OnPropertyChanged(nameof(Department));
                SearchText = Department;

            }
        }
        private string ManufacturerId { get; set; } = string.Empty;
        public bool ManComboVisible
        {
            get => _manComboVisible;
            set
            {
                _manComboVisible = value;
                OnPropertyChanged(nameof(ManComboVisible));
            }
        }
        public string Manufacturer
        {
            get => _manufacturer;
            set
            {
                _manufacturer = value;
                OnPropertyChanged(nameof(Manufacturer));
                SearchText = Manufacturer;
            }
        }
        public bool StatusComboVisible
        {
            get => _statusComboVisible;
            set
            {
                _statusComboVisible = value;
                OnPropertyChanged(nameof(StatusComboVisible));
            }
        }
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                SearchText = Status;

            }
        }
        public bool AssetTypeComboVisible
        {
            get => _assetTypeComboVisible;
            set
            {
                _assetTypeComboVisible = value;
                OnPropertyChanged(nameof(AssetTypeComboVisible));
            }
        }
        public string AssetType
        {
            get => _assetType;
            set
            {
                if (SetProperty(ref _assetType, value))
                {
                    SearchText = AssetType;
                    OnPropertyChanged(nameof(AssetType));
                }
            }
        }
        public bool CustodianComboVisible
        {
            get => _custodianComboVisible;
            set
            {
                _custodianComboVisible = value;
                OnPropertyChanged(nameof(CustodianComboVisible));
            }
        }
        public string Custodian
        {
            get => _custodian;
            set
            {
                _custodian = value;
                OnPropertyChanged(nameof(Custodian));
                SearchText = Custodian;
            }
        }
        public bool BuildingComboVisible
        {
            get => _buildingComboVisible;
            set => SetProperty(ref _buildingComboVisible, value);
        }
        public string Building
        {
            get => _building;
            set
            {
                if (SetProperty(ref _building, value))
                {
                    SearchText = Building;
                }
            }
        }
        public bool SearchTermTextBoxVisible
        {
            get => _searchTermTextBoxVisible;
            set => SetProperty(ref _searchTermTextBoxVisible, value);
        }
        #endregion

        #region ObservableCollections
        public ObservableCollection<string> Models
        {
            get => _models;
            set => SetProperty(ref _models, value);
        }
        public ObservableCollection<string> GridLocations
        {
            get => _gridlocations;
            set
            {
                _gridlocations = value;
                OnPropertyChanged(nameof(GridLocations));
            }
        }
        public ObservableCollection<string> Departments
        {
            get => _departments;
            set
            {
                _departments = value;
                OnPropertyChanged(nameof(Departments));
            }
        }
        public ObservableCollection<string> Manufacturers
        {
            get => _manufacturerNames;
            set
            {
                _manufacturerNames = value;
                OnPropertyChanged(nameof(Manufacturers));
            }
        }
        public ObservableCollection<string> Statuses
        {
            get => _statuses;
            set
            {
                _statuses = value;
                OnPropertyChanged(nameof(Statuses));
            }
        }
        public ObservableCollection<string> AssetTypes
        {
            get => _assetTypes;
            set
            {
                _assetTypes = value;
                OnPropertyChanged(nameof(AssetTypes));
            }
        }
        public ObservableCollection<string> Custodians
        {
            get => _custodians;
            set
            {
                _custodians = value;
                OnPropertyChanged(nameof(Custodians));
            }
        }
        public ObservableCollection<string> Years
        {
            get => _years;
            set
            {
                _years = value;
                OnPropertyChanged(nameof(Years));
            }
        }
        public ObservableCollection<string> Buildings
        {
            get => _buildings;
            set
            {
                _buildings = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> SearchParameterCollection
        {
            get => _searchParameterCollection;
            set => SetProperty(ref _searchParameterCollection, value);
        }
        private async Task<ObservableCollection<string>> LoadComboBoxDataAsync(string storedProcedure, string columnName, SqlParameter parameter = null!)
        {
            return await Task.Run(() =>
            {
                var collection = new ObservableCollection<string>();

                try
                {
                    if (_tools?.Conn == null)
                    {
                        throw new InvalidOperationException("Database connection is not initialized.");
                    }

                    if (_tools.Conn?.State != ConnectionState.Open)
                    {
                        _tools.Conn?.Close();
                        _tools.Conn?.Open();
                    }

                    SqlDataAdapter da = new SqlDataAdapter(storedProcedure, _tools.Conn)
                    {
                        SelectCommand = { CommandType = CommandType.StoredProcedure }
                    };

                    if (parameter != null)
                    {
                        da.SelectCommand.Parameters.Add(parameter);
                    }

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    DataView view = dt.DefaultView;
                    view.Sort = $"{columnName} ASC";
                    DataTable sortedTable = view.ToTable();
                    for (int i = 0; i < sortedTable.Rows.Count; i++)
                    {
                        var item = sortedTable.Rows[i][columnName]?.ToString();
                        if (item != null && !collection.Contains(item))
                        {
                            collection.Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorCatch.Error(e);
                    _messenger.Send(new ValueChangedMessage<string>("AlertTrue"));
                }
                finally
                {
                    _tools?.Conn?.Close();
                }

                return collection;
            });
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

            BuildingComboVisible = false;
            SearchTermTextBoxVisible = false;
            GridLocationComboBoxVisible = false;
            ManComboVisible = false;
            StatusComboVisible = false;
            AssetTypeComboVisible = false;
            CustodianComboVisible = false;
            ModelComboVisible = false;
            YearsComboVisible = false;
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
            BuildingComboVisible = SearchParameter.Equals("Building") || SearchParameter.Equals("Location");
            SearchTermTextBoxVisible = SearchParameter.Equals("Description") || SearchParameter.Equals("Serial Number") || SearchParameter.Equals("Purchase Order") || SearchParameter.Equals("Year Purchased");
            GridLocationComboBoxVisible = SearchParameter.Equals("Location");
            ManComboVisible = SearchParameter.Equals("Manufacturer") || SearchParameter.Equals("Model");
            StatusComboVisible = SearchParameter.Equals("Status");
            AssetTypeComboVisible = SearchParameter.Equals("Asset Type") || SearchParameter.Equals("Finance Report Breakdown") || SearchParameter.Equals("Finance Report Summaries");
            CustodianComboVisible = SearchParameter.Equals("Custodian");
            ModelComboVisible = SearchParameter.Equals("Model");
            DeptComboVisible = SearchParameter.Equals("Department");
            YearsComboVisible = SearchParameter.Equals("Finance Report Breakdown") || SearchParameter.Equals("Finance Report Summaries");
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

                if (storedProcedure == "GetFinanceReport")
                {
                    var result = await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Save Finance Report",
                        FileTypes = FilePickerFileType.Pdf
                    });

                    if (result != null)
                    {
                        string excelFilePath = result.FullPath;
                        TransformAndExportFinanceReportDataToExcel(_gridDataTable, excelFilePath);
                        await _tools.ShowMessageAsync("Information", "Export Complete");
                    }
                    else
                    {
                        await _tools.ShowMessageAsync("Information", "Export Canceled");
                    }
                }
                else
                {
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
                    TransformAndExportFinanceReportDataToExcel(_gridDataTable, excelFilePath);
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
        private void TransformAndExportFinanceReportDataToExcel(DataTable originalTable, string excelFilePath)
        {
            try
            {
                var groupedData = originalTable.AsEnumerable()
                    .GroupBy(row => new
                    {
                        Year = row.Field<DateTime>("Purchase Date").Year,
                        AssetType = AssetType, // Correctly reference the AssetType field
                        CompanyCode = row.Field<string>("Company Code")
                    })
                    .Select(g => new
                    {
                        BulkDescriptor = g.First().Field<string>("Bulk Descriptor"),
                        CompanyCode = g.Key.CompanyCode,
                        AssetDescription = $"{g.Key.Year} {g.Key.AssetType} (Qty {g.Count()})",
                        Value = g.Sum(row => row.Field<decimal?>("Purchase Cost") ?? 0), // Handle DBNull values
                        PO = string.Join(", ", g.Select(row => row.Field<string>("PO Number")).Distinct())
                    });

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Finance Report");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Bulk Descriptor";
                    worksheet.Cell(1, 2).Value = "Asset Description";
                    worksheet.Cell(1, 3).Value = "Company Code";
                    worksheet.Cell(1, 4).Value = "Value";
                    worksheet.Cell(1, 5).Value = "PO";

                    // Format headers
                    var headerRange = worksheet.Range("A1:E1");
                    headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Green;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

                    // Add data
                    int row = 2;
                    foreach (var item in groupedData)
                    {
                        worksheet.Cell(row, 1).Value = item.BulkDescriptor;
                        worksheet.Cell(row, 2).Value = item.AssetDescription;
                        worksheet.Cell(row, 3).Value = item.CompanyCode;
                        worksheet.Cell(row, 4).Value = item.Value;
                        worksheet.Cell(row, 5).Value = item.PO;
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(excelFilePath);
                    _errorCounter = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in TransformAndExportFinanceReportDataToExcel: {ex.Message}");
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
                        new SqlParameter("@Building", Building),
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
                        new SqlParameter("@Year", SearchTextYear),
                        new SqlParameter("@AssetType", SearchText)
                    };
                    break;
                case "Finance Report Summaries":
                    storedProcedure = "GetFinanceReport";
                    parameters = new[]
                    {
                        new SqlParameter("@Year", SearchTextYear),
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
