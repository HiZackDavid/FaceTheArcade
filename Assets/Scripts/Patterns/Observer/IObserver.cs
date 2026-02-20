using UnityEngine;

public interface IObserver<T> 
{
    void OnNotify(T value);
}
