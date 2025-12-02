using UnityEngine;

namespace PhantomBrigade.Data
{
    public static class DebugHelper
    {
        public static bool logToConsole = false;
    
        public static void Log (string message, bool logToConsoleEnd = false)
        {
            if (string.IsNullOrEmpty (message))
                return;
    
            Debug.Log (message);

            #if !PB_MODSDK
            if (logToConsole && Application.isPlaying && QFSW.QC.QuantumConsole.Instance != null)
                QFSW.QC.QuantumConsole.Instance.LogToConsole (message);
            
            if (logToConsoleEnd)
                logToConsole = false;
            #endif
        }
        
        public static void LogWarning (string message, bool logToConsoleEnd = false)
        {
            if (string.IsNullOrEmpty (message))
                return;
    
            Debug.LogWarning (message);
        
            #if !PB_MODSDK
            if (logToConsole && Application.isPlaying && QFSW.QC.QuantumConsole.Instance != null)
                QFSW.QC.QuantumConsole.Instance.LogToConsole ($"<color=yellow>{message}</color>");

            if (logToConsoleEnd)
                logToConsole = false;
            #endif
        }
    }
}