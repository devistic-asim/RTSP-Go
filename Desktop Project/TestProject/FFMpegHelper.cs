using System;
using System.IO;
using System.Runtime.InteropServices;

using FFmpeg.AutoGen;

namespace TestProject
{
    /// <summary>
    /// FFMPEG Helper
    /// </summary>
    public static class FFMpegHelper
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Import
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Private

        #region Setting the DLL directory - SetDllDirectory(directoryPath)

        /// <summary>
        /// Setting the DLL directory
        /// </summary>
        /// <param name="directoryPath">directory path</param>
        /// <returns>processing result</returns>
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string directoryPath);

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// LD_LIBRARY_PATH
        /// </summary>
        private const string LD_LIBRARY_PATH = "LD_LIBRARY_PATH";

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Public

        #region registration - Register()

        /// <summary>
        /// registration
        /// </summary>
        public static void Register()
        {
            switch(Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT      :
                case PlatformID.Win32S       :
                case PlatformID.Win32Windows :
                {
                    string currentDirectoryPath = Environment.CurrentDirectory;

                    while(currentDirectoryPath != null)
                    {
                        string dllDirectoryPath = Path.Combine(currentDirectoryPath, "FFMpegDLL");

                        if(Directory.Exists(dllDirectoryPath))
                        {
                            Register(dllDirectoryPath);

                            return;
                        }

                        currentDirectoryPath = Directory.GetParent(currentDirectoryPath)?.FullName;
                    }

                    break;
                }
                case PlatformID.Unix   :
                case PlatformID.MacOSX :
                {
                    string dllDirectoryPath = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);

                    Register(dllDirectoryPath);

                    break;
                }
            }
        }

        #endregion

        #region Get the error message- GetErrorMessage(errorCode)

        /// <summary>
        /// Get the error message
        /// </summary>
        /// <param name="errorCode">error code</param>
        /// <returns>error message</returns>
        public static unsafe string GetErrorMessage(int errorCode)
        {
            int bufferSize = 1024;

            byte* buffer = stackalloc byte[bufferSize];

            ffmpeg.av_strerror(errorCode, buffer, (ulong)bufferSize);

            string message = Marshal.PtrToStringAnsi((IntPtr)buffer);

            return message;
        }

        #endregion
        #region throw exception on error - ThrowExceptionIfError(error)

        /// <summary>
        /// throw exception on error
        /// </summary>
        /// <param name="errorCode">error code</param>
        /// <returns>error code</returns>
        public static int ThrowExceptionIfError(this int errorCode)
        {
            if(errorCode < 0)
            {
                throw new ApplicationException(GetErrorMessage(errorCode));
            }

            return errorCode;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////// Private

        #region registration - Register(dllDirectoryPath)

        /// <summary>
        /// registration
        /// </summary>
        /// <param name="dllDirectoryPath">DLL directory path</param>
        private static void Register(string dllDirectoryPath)
        {
            switch(Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT      :
                case PlatformID.Win32S       :
                case PlatformID.Win32Windows :

                    SetDllDirectory(dllDirectoryPath);

                    break;

                case PlatformID.Unix   :
                case PlatformID.MacOSX :

                    string currentValue = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);

                    if(string.IsNullOrWhiteSpace(currentValue) == false && currentValue.Contains(dllDirectoryPath) == false)
                    {
                        string newValue = currentValue + Path.PathSeparator + dllDirectoryPath;

                        Environment.SetEnvironmentVariable(LD_LIBRARY_PATH, newValue);
                    }

                    break;
            }
        }

        #endregion
    }
}