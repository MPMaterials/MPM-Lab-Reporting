using System.Data;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MPM_Lab_Reporting.Messages
{
    public class DataTableMessage : ValueChangedMessage<DataTable>
    {
        public DataTableMessage(DataTable value) : base(value) { }
    }
}
