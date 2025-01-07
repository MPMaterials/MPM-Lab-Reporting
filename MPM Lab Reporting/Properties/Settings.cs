using Microsoft.Maui.Storage;

namespace MPM_Lab_Reporting.Properties
{
    public static class Settings
    {
        private const string TenantIDKey = "TenantID";
        private const string ClientIDKey = "ClientID";
        private const string SqlSvrKey = "SqlSvr";
        private const string SqlDBKey = "SqlDB";

        public static string TenantID
        {
            get => Preferences.Get(TenantIDKey, "ef4fd1da-41d8-4fe3-bf7f-e0cb2c96b78d");
            set => Preferences.Set(TenantIDKey, value);
        }

        public static string ClientID
        {
            get => Preferences.Get(ClientIDKey, "e1d3cd6d-1ea9-4617-8c86-3db97421267e");
            set => Preferences.Set(ClientIDKey, value);
        }

        public static string SqlSvr
        {
            get => Preferences.Get(SqlSvrKey, "MPMSQLHA.root.mpmaterials.com");
            set => Preferences.Set(SqlSvrKey, value);
        }

        public static string SqlDB
        {
            get => Preferences.Get(SqlDBKey, "MPALAB01");
            set => Preferences.Set(SqlDBKey, value);
        }
    }
}
