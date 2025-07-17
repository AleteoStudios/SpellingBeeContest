using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();
    private static UnityMainThreadDispatcher instance;

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            // Busca si ya existe en la escena
            instance = FindObjectOfType<UnityMainThreadDispatcher>();

            // Si no existe, créalo
            if (instance == null)
            {
                GameObject obj = new GameObject("UnityMainThreadDispatcher");
                instance = obj.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(obj);
            }
        }

        return instance;
    }

    void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue().Invoke();
            }
        }
    }

    /// <summary>
    /// Encola una acción para que se ejecute en el hilo principal.
    /// </summary>
    public void Enqueue(Action action)
    {
        if (action == null) return;

        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    /// <summary>
    /// Método estático de conveniencia.
    /// </summary>
    public static void EnqueueToMainThread(Action action)
    {
        Instance().Enqueue(action);
    }
}
