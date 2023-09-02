using System;
using System.Runtime.InteropServices;
using System.Windows;

using FFmpeg.AutoGen;

namespace TestProject
{
    /// <summary>
    /// video frame converter
    /// </summary>
    public sealed unsafe class VideoFrameConverter : IDisposable
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// target size
        /// </summary>
        private readonly Size targetSize;

        /// <summary>
        /// context
        /// </summary>
        private readonly SwsContext* context;

        /// <summary>
        /// buffer handle
        /// </summary>
        private readonly IntPtr buferHandle;

        /// <summary>
        /// temporary frame data
        /// </summary>
        private readonly byte_ptrArray4 temporaryFrameData;

        /// <summary>
        /// temporary frame data
        /// </summary>
        private readonly int_array4 temporaryFrameLineSize;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Constructor
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region constructor - VideoFrameConverter(sourceSize, sourcePixelFormat, targetSize, targetPixelFormat)

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sourceSize">source size</param>
        /// <param name="sourcePixelFormat">source pixel format</param>
        /// <param name="targetSize">target size</param>
        /// <param name="targetPixelFormat">target pixel format</param>
        public VideoFrameConverter(Size sourceSize, AVPixelFormat sourcePixelFormat, Size targetSize, AVPixelFormat targetPixelFormat)
        {
            this.targetSize = targetSize;

            this.context = ffmpeg.sws_getContext
            (
                (int)sourceSize.Width,
                (int)sourceSize.Height,
                sourcePixelFormat,
                (int)targetSize.Width,
                (int)targetSize.Height,
                targetPixelFormat,
                ffmpeg.SWS_FAST_BILINEAR,
                null,
                null,
                null
            );

            if(this.context == null)
            {
                throw new ApplicationException("Could not initialize the conversion context.");
            }

            int bufferSize = ffmpeg.av_image_get_buffer_size(targetPixelFormat, (int)targetSize.Width, (int)targetSize.Height, 1);

            this.buferHandle = Marshal.AllocHGlobal(bufferSize);

            this.temporaryFrameData = new byte_ptrArray4();

            this.temporaryFrameLineSize = new int_array4();

            ffmpeg.av_image_fill_arrays
            (
                ref this.temporaryFrameData,
                ref this.temporaryFrameLineSize,
                (byte*)this.buferHandle,
                targetPixelFormat,
                (int)targetSize.Width,
                (int)targetSize.Height,
                1
            );
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region to convert - Convert(sourceFrame)

        /// <summary>
        /// to convert
        /// </summary>
        /// <param name="sourceFrame">source frame</param>
        /// <returns>frame</returns>
        public AVFrame Convert(AVFrame sourceFrame)
        {
            ffmpeg.sws_scale
            (
                this.context,
                sourceFrame.data,
                sourceFrame.linesize,
                0,
                sourceFrame.height,
                this.temporaryFrameData,
                this.temporaryFrameLineSize
            );

            byte_ptrArray8 targetFrameData = new byte_ptrArray8();

            targetFrameData.UpdateFrom(this.temporaryFrameData);

            int_array8 targetFrameLineSize = new int_array8();

            targetFrameLineSize.UpdateFrom(this.temporaryFrameLineSize);

            return new AVFrame
            {
                data     = targetFrameData,
                linesize = targetFrameLineSize,
                width    = (int)this.targetSize.Width,
                height   = (int)this.targetSize.Height
            };
        }

        #endregion

        #region freeing up resources - Dispose()

        /// <summary>
        /// freeing up resources
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(this.buferHandle);

            ffmpeg.sws_freeContext(this.context);
        }

        #endregion
    }
}