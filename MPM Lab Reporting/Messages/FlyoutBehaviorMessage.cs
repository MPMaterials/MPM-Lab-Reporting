using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Maui.Controls;

namespace MPM_Lab_Reporting.Messages
{
    public class FlyoutBehaviorMessage : ValueChangedMessage<FlyoutBehavior>
    {
        public FlyoutBehaviorMessage(FlyoutBehavior value) : base(value)
        {
        }
    }
}
