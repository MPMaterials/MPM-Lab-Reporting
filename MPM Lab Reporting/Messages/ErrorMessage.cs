using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPM_Lab_Reporting.Messages
{
    public class ErrorMessage : ValueChangedMessage<bool>
    {
        public ErrorMessage(bool hasError) : base(hasError) { }
    }
}
