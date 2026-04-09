namespace InternetBankCalculator;

public class MyLoadableSource : ILoadable
{
    public event EventHandler? IsExecutingChanged;

    private bool _isExecuting;
    public bool IsExecuting
    {
        get => _isExecuting;
        set
        {
            if (_isExecuting != value)
            {
                _isExecuting = value;
                IsExecutingChanged?.Invoke(this, new());
            }
        }
    }

    // No Execute method needed; IsExecuting is controlled by app logic
}