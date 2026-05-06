namespace Main;

public class HysteresisState<T> where T : Enum
{
    private readonly List<(T State, float Threshold)> _thresholds;
    private readonly float _buffer;
    private T _currentState;

    public T CurrentState => _currentState;

    public HysteresisState(IEnumerable<(T State, float Threshold)> thresholds, float buffer = 0.1f)
    {
        _thresholds = thresholds.OrderBy(x => x.Threshold).ToList();
        _buffer = buffer;
        _currentState = _thresholds.First().State;
    }

    public bool Update(float input)
    {
        T previousState = _currentState;
        int currentIndex = _thresholds.FindIndex(x => x.State.Equals(_currentState));

        if (currentIndex < _thresholds.Count - 1)
        {
            if (input >= _thresholds[currentIndex + 1].Threshold + _buffer)
            {
                _currentState = _thresholds[currentIndex + 1].State;
            }
        }

        if (currentIndex > 0)
        {
            if (input < _thresholds[currentIndex].Threshold - _buffer)
            {
                _currentState = _thresholds[currentIndex - 1].State;
            }
        }

        return !EqualityComparer<T>.Default.Equals(previousState, _currentState);
    }
}