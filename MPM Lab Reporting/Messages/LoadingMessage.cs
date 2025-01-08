using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPM_Lab_Reporting.Messages
{
    public class LoadingMessage
    {
        public bool IsLoading { get; }

        public LoadingMessage(bool isLoading)
        {
            IsLoading = isLoading;
        }
    }
}
