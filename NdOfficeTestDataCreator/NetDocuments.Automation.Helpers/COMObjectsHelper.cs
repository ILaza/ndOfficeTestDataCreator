using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Automation;

namespace NetDocuments.Automation.Helpers
{
    /// <summary>
    /// Helper class for operate with COM objects.
    /// </summary>
    public class COMObjectsHelper : System.IDisposable
    {
        private const int RPC_E_SERVERCALL_RETRYLATER = -2147417846;

        private List<object> COMObjects = new List<object>();

        /// <summary>
        /// Registers new COM object with registrator functor,
        /// holds all registered objects in internal collection and releases them in the end of helper live.
        /// </summary>
        /// <typeparam name="T">COM object type.</typeparam>
        /// <param name="registrator">Functor that register COM object.</param>
        /// <returns>COM object instance or null in case when registration failed.</returns>
        public T Register<T>(Func<T> registrator) where T : class
        {
            return Wait.ForResult<T>(() =>
            {
                try
                {
                    var obj = registrator?.Invoke();
                    if (obj != null)
                    {
                        COMObjects.Add(obj);
                    }
                    return obj;
                }
                // Note: catch application busy exception. We should wait for some time.
                catch (COMException ex) when (ex.ErrorCode == RPC_E_SERVERCALL_RETRYLATER)
                {
                    return null;
                }
            },
            retryRateDelayMilliSeconds: 100);
        }

        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

        public List<T> GetAllComInstancesFromROT<T>(COMObjectsHelper comHelper)
        {
            var objects = new List<T>();

            CreateBindCtx(0, out var bindCtx);

            if (bindCtx == null)
            {
                return objects;
            }
            comHelper.Register(() => bindCtx);

            bindCtx.GetRunningObjectTable(out var rot);

            if (rot == null)
            {
                return objects;
            }
            comHelper.Register(() => rot);

            rot.EnumRunning(out var enumMoniker);
            comHelper.Register(() => enumMoniker);
            enumMoniker.Reset();

            var fetched = IntPtr.Zero;
            var moniker = comHelper.Register(() => new IMoniker[1]);

            while (enumMoniker.Next(1, moniker, fetched) == 0)
            {
                moniker[0].GetDisplayName(bindCtx, null, out var displayName);

                var hresult = rot.GetObject(moniker[0], out var obj);

                // Note: In case when retrieval operation was not succeed just skip further actions.
                if (hresult != 0)
                {
                    Console.WriteLine($"Retrieve COM object failed with result: {hresult}");
                    continue;
                }

                comHelper.Register(() => obj);

                try
                {
                    if (obj is T)
                    {
                        objects.Add((T)obj);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not get instance of COM object because of: {ex.Message}");
                    Console.WriteLine(ex);
                }
            }

            return objects;
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            COMObjects.ForEach(TypeHelper.ReleaseCOMObject);
            COMObjects.Clear();
        }

        public static List<T> GetActiveInteropApp<T>(COMObjectsHelper comHelper, string interopAppName)
        {
            return Wait.ForResult(() =>
            {
                try
                {
                    var comObject = Marshal.GetActiveObject(interopAppName);
                    if (comObject == null)
                    {
                        return null;
                    }

                    comHelper.Register(() => comObject);
                    return new List<T> { (T)comObject };
                }
                catch (ElementNotAvailableException ex)
                {
                    Console.WriteLine($"Exception thrown during getting '{interopAppName}' active interop object.");
                    Console.WriteLine(ex);

                    return null;
                }
                catch (COMException ex)
                {
                    Console.WriteLine($"Exception thrown during getting '{interopAppName}' active interop object.");
                    Console.WriteLine(ex);

                    var comInstancesFromROT = comHelper.GetAllComInstancesFromROT<T>(comHelper);

                    if (comInstancesFromROT.Count == 0)
                    {
                        return null;
                    }

                    return comInstancesFromROT;
                }
            },
            timeoutMilliSeconds: 60000,
            retryRateDelayMilliSeconds: 5000);
        }
    }
}
