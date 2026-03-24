namespace Core
{
    public interface ISaveData { }

    public interface ISavable
    {
        string SaveID { get; }
        object CaptureState();
        void RestoreState(object state);
    }

    public interface ISavable<T> : ISavable where T : ISaveData
    {
        T CaptureSaveData();
        void RestoreSaveData(T data);

        object ISavable.CaptureState() => CaptureSaveData();
        void ISavable.RestoreState(object state) => RestoreSaveData((T)state);
    }
}
