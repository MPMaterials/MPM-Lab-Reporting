using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MPM_Lab_Reporting.Helpers;

namespace MPM_Lab_Reporting.Helpers
{
    public class DatabaseHelper
    {
        private readonly Tools _tools;

        public DatabaseHelper(Tools tools)
        {
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
        }

        public async Task<DataTable> ExecuteDatabaseCommandAsync(string storedProcedure, SqlParameter[]? parameters, Action<bool> setLoading)
        {
            DataTable dataTable = new DataTable();
            try
            {
                setLoading(true);
                await _tools.EnsureConnectionOpenAsync();

                SqlCommand cmd = new SqlCommand(storedProcedure, _tools.Conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dataTable);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception in ExecuteDatabaseCommandAsync: {e.Message}");
                ErrorCatch.Error(e);
                throw;
            }
            finally
            {
                _tools.Conn?.Close();
                setLoading(false);
            }

            return dataTable;
        }

        public async Task ExecuteDataGridCommandAsync(Action action)
        {
            try
            {
                await Task.Run(action);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ExecuteDataGridCommandAsync: {ex.Message}");
                throw;
            }
        }
    }
}
