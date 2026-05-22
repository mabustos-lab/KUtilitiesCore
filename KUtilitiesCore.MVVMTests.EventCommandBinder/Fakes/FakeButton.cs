using System;
 
namespace KUtilitiesCore.MVVMTests.EventCommandBinder.Fakes
{
    public class FakeButton
    {
        public event EventHandler? Click;
        public string ClickProperty { get; set; } = string.Empty;

        public void SimulateClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }
}
