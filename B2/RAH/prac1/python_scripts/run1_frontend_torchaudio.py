################################################################################
# Script 1:                                                                    #
# FRONTEND TORCHAUDIO                                                          #
################################################################################

import torchaudio
import scipy.signal
import numpy as np
import matplotlib.pyplot as plt
import matplotlib
matplotlib.use('Agg')

# 1. Examine .wav files ########################################################

# uncomment according to the file you want to use
wav = 'data/data1/train/train_00001_m_01_0.wav' # wavfile1
# wav = 'data/wav/audio1.wav'                     # wavfile2 
# wav = 'data/wav/audio2.wav'                     # wavfile3

## - Waveform ------------------------------------------------------------------
##   read and plot the waveform of a wav file.

x, fs = torchaudio.load(wav)
print(x.shape)
plt.plot(x.t().numpy())
plt.savefig('out/run1_frontend_torchaudio/waveform.png')

## - Power spectrogram ---------------------------------------------------------
##   Extract the power spectrogram of a wav file with a 25ms window and 10ms 
##   shift using the Short-Time Fourier Transform (STFT).

specgram = torchaudio.transforms.Spectrogram(
    n_fft=512, win_length=25*16, hop_length=10*16)(x)  # 25ms window, 10ms shift
print(specgram.shape)
plt.imshow(specgram.log()[0].numpy(), cmap='jet', aspect='auto',origin='lower') 
plt.savefig('out/run1_frontend_torchaudio/spectrogram.png')

## - Mel-spectrogram -----------------------------------------------------------
##   Extract the mel-spectrogram of a wav file with a 25ms window and 10ms shift
##   using the Short-Time Fourier Transform (STFT) and a mel filterbank with 80 
##   filters.

melspecgram = torchaudio.transforms.MelSpectrogram(
    n_fft=512, win_length=25*16, hop_length=10*16, n_mels=80)(x) # 25ms window, 
                                                                 # 10ms shift
print(melspecgram.shape)
plt.imshow(melspecgram.log()[0].numpy(), cmap='jet', aspect='auto', 
           origin='lower')
plt.savefig('out/run1_frontend_torchaudio/melspectrogram.png')

## - MFCC ----------------------------------------------------------------------
##   Extract the mel-frequency cepstral coefficients (MFCC) of a wav file with a
##   25ms window and 10ms shift using the Short-Time Fourier Transform (STFT)

mfcc = torchaudio.transforms.MFCC(
    melkwargs={"n_fft": 25*16, "hop_length": 10*16, "n_mels": 48, # 25ms window, 
               "center": False}, n_mfcc=40, log_mels=True)(x)     # 10ms shift
print(mfcc.shape)
plt.imshow(mfcc[0,1:].numpy(), cmap='jet', aspect='auto',origin='lower')
plt.savefig('out/run1_frontend_torchaudio/mfcc.png')

## - Real cepstrum -------------------------------------------------------------
##   Extract the real cepstrum of a wav file with a 25ms window and 10ms shift 
##   using the Short-Time Fourier Transform (STFT).

f, t, cspecgram = scipy.signal.stft(
    x[0].numpy(), fs=fs,
    nperseg=25*16, noverlap=25*16-10*16, # 25ms window, 10ms shift
    nfft=512, return_onesided=False)
print(cspecgram.shape, cspecgram.dtype)
ceps = np.fft.ifft(np.log(np.abs(cspecgram)+1e-6), axis=0)
print(ceps.shape)
plt.imshow(np.abs(ceps)[1:256], cmap='jet', aspect='auto',origin='lower') 
plt.savefig('out/run1_frontend_torchaudio/cepstrum.png')