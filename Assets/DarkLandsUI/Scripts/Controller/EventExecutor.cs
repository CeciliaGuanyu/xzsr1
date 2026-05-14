using UnityEngine.Events;

namespace DarkLandsUI.Scripts.Controller
{
    public class EventExecutor : UnityEngine.MonoBehaviour
    {
        public UnityEvent eventToExecute;

        public void ExecuteUnityEvent()
        {
            eventToExecute.Invoke();
        }
    }
}