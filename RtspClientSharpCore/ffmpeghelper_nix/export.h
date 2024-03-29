#pragma once

#ifdef _WINDLL
#define DllExport(rettype)  extern "C" __declspec(dllexport) rettype __cdecl
#else
#define DllExport(rettype)  extern "C" __attribute__((cdecl)) rettype
#endif

extern "C" int create_video_decoder(int codec_id, void **handle);
extern "C"  int set_video_decoder_extradata(void *handle, void *extradata, int extradataLength);
extern "C"  int decode_video_frame(void *handle, void *rawBuffer, int rawBufferLength, int *frameWidth, int *frameHeight, int *framePixelFormat);
extern "C"  int scale_decoded_video_frame(void *handle, void *scalerHandle, void *scaledBuffer, int scaledBufferStride);
extern "C"  void remove_video_decoder(void *handle);

extern "C"  int create_video_scaler(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int sourcePixelFormat, 
	int scaledWidth, int scaledHeight, int scaledPixelFormat, int quality, void **handle);
extern "C"  void remove_video_scaler(void *handle);

extern "C"  int create_audio_decoder(int codec_id, int bits_per_coded_sample, void **handle);
extern "C"  int set_audio_decoder_extradata(void *handle, void *extradata, int extradataLength);
extern "C"  int decode_audio_frame(void *handle, void *rawBuffer, int rawBufferLength, int *sampleRate, int *bitsPerSample, int *channels);
extern "C"  int get_decoded_audio_frame(void *handle, void **outBuffer, int *outDataSize);
extern "C"  void remove_audio_decoder(void *handle);

extern "C"  int create_audio_resampler(void *decoderHandle, int outSampleRate, int outBitsPerSample, int outChannels, void **handle);
extern "C"  int resample_decoded_audio_frame(void *decoderHandle, void *resamplerHandle, void **outBuffer, int *outDataSize);
extern "C"  void remove_audio_resampler(void *handle);