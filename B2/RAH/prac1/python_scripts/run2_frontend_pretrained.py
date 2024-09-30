################################################################################
# Script 2:                                                                    #
# FRONTEND PRETRAINED                                                          #
################################################################################

import torchaudio
from transformers import WavLMModel
import torch
import glob
from sklearn.manifold import TSNE
import matplotlib.pyplot as plt
import matplotlib
matplotlib.use('Agg')
import numpy as np

# 1. Load wavlm model ##########################################################
print("Part 1: Load wavlm model ##############################################")

model = WavLMModel.from_pretrained(
    "patrickvonplaten/wavlm-libri-clean-100h-base-plus")

print(model.feature_extractor)
print(model.feature_projection)
print(model.encoder)

# Print number of parameters
print('- Number of parameters:', sum(p.numel() for p in 
                                   model.parameters() if p.requires_grad))

print('\n')
# 2. Test feature extractor and model output (encoder) #########################
print("Part 2: Test feature extractor and model output (encoder) #############")

x = torch.randn(1, 16000)

f = model.feature_extractor(x)
print("- Model feature extractor shape:", f.shape)

print("- Model feature extractor:", model.feature_extractor)

y = model(x).last_hidden_state
print("- Model output (encoder) shape:", y.shape)

torch.onnx.export(model.feature_extractor, x, "feature_extractor.onnx")

print('\n')
# 3. Embeddings from feature extractor #########################################
print("Part 3: Embeddings from feature extractor #############################")

def embeddings(pattern):
    files_ = glob.glob(pattern)
    e = []
    for wav in sorted(files_):
        x, fs = torchaudio.load(wav)
        f = model.feature_extractor(x)
        e.append( f.mean(2) )
    return torch.cat(e).detach().numpy()

print('\n')
##  - Embeddings from feature extractor: gender --------------------------------
print("-> Gender ---------------------------------")

e1 = embeddings('data/data1/test/*m*2.wav')
e2 = embeddings('data/data1/test/*f*2.wav')
e = np.concatenate([e1,e2])
print("- Embeddings shape:", e1.shape, e2.shape)
et = TSNE(n_components=2).fit_transform(e)
plt.clf()
plt.scatter(et[:len(e1),0], et[:len(e1),1], c='b')
plt.scatter(et[len(e1):,0], et[len(e1):,1], c='r')
plt.savefig('out/run2_frontend_pretrained/embeddings11.png')

print('\n')
##  - Embeddings from feature extractor: speaker -------------------------------
print("-> Speaker --------------------------------")

e1 = embeddings('data/data1/test/*m_51_2.wav')
e2 = embeddings('data/data1/test/*m_53_2.wav')
e3 = embeddings('data/data1/test/*m_54_2.wav')
e4 = embeddings('data/data1/test/*m_55_2.wav')
e = np.concatenate([e1,e2,e3,e4])
l = np.concatenate([np.zeros(len(e1)), np.ones(len(e2)), np.ones(len(e3))*2,
                     np.ones(len(e4))*3])
print("- Embeddings shape:", e1.shape, e2.shape, e3.shape, e4.shape)
et = TSNE(n_components=2).fit_transform(e)
plt.clf()
plt.scatter(et[l==0,0], et[l==1,1], c='b')
plt.scatter(et[l==1,0], et[l==1,1], c='r')
plt.scatter(et[l==2,0], et[l==2,1], c='g')
plt.scatter(et[l==3,0], et[l==3,1], c='y')
plt.savefig('out/run2_frontend_pretrained/embeddings12.png')

print('\n')
##  - Embeddings from feature extractor: word ----------------------------------
print("-> Word -----------------------------------")

e1 = embeddings('data/data1/test/*m_51_1.wav')
e2 = embeddings('data/data1/test/*m_51_2.wav')
e3 = embeddings('data/data1/test/*m_51_3.wav')
e4 = embeddings('data/data1/test/*m_51_4.wav')

e = np.concatenate([e1,e2,e3,e4])
l = np.concatenate([np.zeros(len(e1)), np.ones(len(e2)), np.ones(len(e3))*2, 
                    np.ones(len(e4))*3])
print("- Embeddings shape:", e1.shape, e2.shape, e3.shape, e4.shape)
print(e1.shape, e2.shape, e3.shape, e4.shape)
et = TSNE(n_components=2).fit_transform(e)
plt.clf()
plt.scatter(et[l==0,0], et[l==1,1], c='b')
plt.scatter(et[l==1,0], et[l==1,1], c='r')
plt.scatter(et[l==2,0], et[l==2,1], c='g')
plt.scatter(et[l==3,0], et[l==3,1], c='y')
plt.savefig('out/run2_frontend_pretrained/embeddings13.png')

print('\n')
# 4. Embeddings from model output (encoder) ####################################
print("Part 4: Embeddings from model output (encoder) ########################")

def embeddings2(pattern):
    files_ = glob.glob(pattern)
    e = []
    for wav in sorted(files_):
        x, fs = torchaudio.load(wav)
        o = model(x).last_hidden_state
        e.append( o.mean(1) )
    return torch.cat(e).detach().numpy()

print('\n')
# Embeddings from model output: gender ----------------------------------------
print("-> Gender ---------------------------------")

e1 = embeddings2('data/data1/test/*m*2.wav')
e2 = embeddings2('data/data1/test/*f*2.wav')
e = np.concatenate([e1,e2])
print("- Embeddings shape:", e1.shape, e2.shape)
et = TSNE(n_components=2).fit_transform(e)
plt.clf()
plt.scatter(et[:len(e1),0], et[:len(e1),1], c='b')
plt.scatter(et[len(e1):,0], et[len(e1):,1], c='r')
plt.savefig('out/run2_frontend_pretrained/embeddings21.png')

print('\n')
# Embeddings from model output: speaker ---------------------------------------
print("-> Speaker --------------------------------")

e1 = embeddings2('data/data1/test/*m_51_2.wav')
e2 = embeddings2('data/data1/test/*m_53_2.wav')
e3 = embeddings2('data/data1/test/*m_54_2.wav')
e4 = embeddings2('data/data1/test/*m_55_2.wav')

e = np.concatenate([e1,e2,e3,e4])
l = np.concatenate([np.zeros(len(e1)), np.ones(len(e2)), np.ones(len(e3))*2, 
                    np.ones(len(e4))*3])
print("- Embeddings shape:", e1.shape, e2.shape, e3.shape, e4.shape)
et = TSNE(n_components=2).fit_transform(e)
plt.clf()
plt.scatter(et[l==0,0], et[l==1,1], c='b')
plt.scatter(et[l==1,0], et[l==1,1], c='r')
plt.scatter(et[l==2,0], et[l==2,1], c='g')
plt.scatter(et[l==3,0], et[l==3,1], c='y')
plt.savefig('out/run2_frontend_pretrained/embeddings22.png')

print('\n')
# Embeddings from model output: word ------------------------------------------
print("-> Word -----------------------------------")

e1 = embeddings2('data/data1/test/*m_51_1.wav')
e2 = embeddings2('data/data1/test/*m_51_2.wav')
e3 = embeddings2('data/data1/test/*m_51_3.wav')
e4 = embeddings2('data/data1/test/*m_51_4.wav')

e = np.concatenate([e1,e2,e3,e4])
l = np.concatenate([np.zeros(len(e1)), np.ones(len(e2)), np.ones(len(e3))*2, np.ones(len(e4))*3])

print("- Embeddings shape:", e1.shape, e2.shape, e3.shape, e4.shape)
et = TSNE(n_components=2).fit_transform(e)
plt.clf()
plt.scatter(et[l==0,0], et[l==1,1], c='b')
plt.scatter(et[l==1,0], et[l==1,1], c='r')
plt.scatter(et[l==2,0], et[l==2,1], c='g')
plt.scatter(et[l==3,0], et[l==3,1], c='y')
plt.savefig('out/run2_frontend_pretrained/embeddings23.png')