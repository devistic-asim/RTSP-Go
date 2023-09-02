using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

using FFmpeg.AutoGen;

namespace TestProject
{
    /// <summary>
    /// video stream decoder
    /// </summary>
    public sealed unsafe class VideoStreamDecoder : IDisposable
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// format context
        /// </summary>
        private readonly AVFormatContext* formatContext;

        /// <summary>
        /// stream index
        /// </summary>
        private readonly int streamIndex;

        /// <summary>
        /// codec context
        /// </summary>
        private readonly AVCodecContext* codecContext;

        /// <summary>
        /// packet
        /// </summary>
        private readonly AVPacket* packet;

        /// <summary>
        /// frame
        /// </summary>
        private readonly AVFrame* frame;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Property
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region codec name - CodecName

        /// <summary>
        /// codec name
        /// </summary>
        public string CodecName { get; }

        #endregion
        #region frame size - FrameSize

        /// <summary>
        /// frame size
        /// </summary>
        public Size FrameSize { get; }

        #endregion
        #region pixel format - PixelFormat

        /// <summary>
        /// pixel format
        /// </summary>
        public AVPixelFormat PixelFormat { get; }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Constructor
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region constructor - VideoStreamDecoder(url)

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="url">URL</param>
        public VideoStreamDecoder(string url)
        {
            this.formatContext = ffmpeg.avformat_alloc_context();

            AVFormatContext* formatContext = this.formatContext;
            
            ffmpeg.avformat_open_input(&formatContext, url, null, null).ThrowExceptionIfError();

            ffmpeg.avformat_find_stream_info(this.formatContext, null).ThrowExceptionIfError();

            AVStream* stream = null;

            for(var i = 0; i < this.formatContext->nb_streams; i++)
            {
                if(this.formatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    stream = this.formatContext->streams[i];

                    break;
                }
            }

            if(stream == null)
            {
                throw new InvalidOperationException("Could not found video stream.");
            }

            this.streamIndex = stream->index;

            this.codecContext = stream->codec;

            AVCodecID codecID = this.codecContext->codec_id;

            AVCodec* codec = ffmpeg.avcodec_find_decoder(codecID);

            if(codec == null)
            {
                throw new InvalidOperationException("Unsupported codec.");
            }

            ffmpeg.avcodec_open2(this.codecContext, codec, null).ThrowExceptionIfError();

            CodecName = ffmpeg.avcodec_get_name(codecID);

            FrameSize = new System.Windows.Size(this.codecContext->width, this.codecContext->height);

            PixelFormat = this.codecContext->pix_fmt;

            this.packet = ffmpeg.av_packet_alloc();

            this.frame = ffmpeg.av_frame_alloc();
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region Get context information dictionary - GetContextInfoDictionary()

        /// <summary>
        /// Get context information dictionary
        /// </summary>
        /// <returns>context information dictionary</returns>
        public IReadOnlyDictionary<string, string> GetContextInfoDictionary()
        {
            AVDictionaryEntry* dictionaryEntry = null;

            Dictionary<string, string> resultDictionary = new Dictionary<string, string>();

            while((dictionaryEntry = ffmpeg.av_dict_get(this.formatContext->metadata, "", dictionaryEntry, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                string key   = Marshal.PtrToStringAnsi((IntPtr)dictionaryEntry->key  );
                string value = Marshal.PtrToStringAnsi((IntPtr)dictionaryEntry->value);

                resultDictionary.Add(key, value);
            }

            return resultDictionary;
        }

        #endregion
        #region Attempt to decode next frame - TryDecodeNextFrame(frame)

        /// <summary>
        /// Attempt to decode next frame
        /// </summary>
        /// <param name="frame">frame</param>
        /// <returns>processing result</returns>
        public bool TryDecodeNextFrame(out AVFrame frame)
        {
            ffmpeg.av_frame_unref(this.frame);

            int errorCode;

            do
            {
                try
                {
                    do
                    {
                        errorCode = ffmpeg.av_read_frame(this.formatContext, this.packet);

                        if(errorCode == ffmpeg.AVERROR_EOF)
                        {
                            frame = *this.frame;

                            return false;
                        }

                        errorCode.ThrowExceptionIfError();
                    }
                    while(this.packet->stream_index != this.streamIndex);

                    ffmpeg.avcodec_send_packet(this.codecContext, this.packet).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(this.packet);
                }

                errorCode = ffmpeg.avcodec_receive_frame(this.codecContext, this.frame);
            }
            while(errorCode == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            errorCode.ThrowExceptionIfError();

            frame = *this.frame;

            return true;
        }

        #endregion
        #region freeing up resources - Dispose()

        /// <summary>
        /// freeing up resources
        /// </summary>
        public void Dispose()
        {
            ffmpeg.av_frame_unref(this.frame);

            ffmpeg.av_free(this.frame);

            ffmpeg.av_packet_unref(this.packet);

            ffmpeg.av_free(this.packet);

            ffmpeg.avcodec_close(this.codecContext);

            AVFormatContext* formatContext = this.formatContext;

            ffmpeg.avformat_close_input(&formatContext);
        }

        #endregion
    }
}