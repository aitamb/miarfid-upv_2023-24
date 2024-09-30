# Calculate the entropy of the paths given a sequence

import numpy as np
import math

# Variables
Q = 5

# Graph
G = np.zeros((Q, Q), dtype=dict)
G[0][1] = {'a': 0.2, 'b': 0.0}
G[0][2] = {'a': 0.8, 'b': 0.0}
G[1][1] = {'a': 0.3, 'b': 0.0}
G[1][2] = {'a': 0.0, 'b': 0.4}
G[1][3] = {'a': 0.3, 'b': 0.0}
G[2][2] = {'a': 0.3, 'b': 0.0}
G[2][3] = {'a': 0.0, 'b': 0.5}
G[2][4] = {'a': 0.2, 'b': 0.0}
G[3][2] = {'a': 0.6, 'b': 0.0}
G[3][3] = {'a': 0.0, 'b': 0.2}
G[3][4] = {'a': 0.2, 'b': 0.0}

# Sequence
seq = '0ababaa'

# Inicialization
# - C matrix
C = np.zeros((len(seq), Q)) # C[t][j]: probability of being in state j at time t
C[0][0] = 1.0 # C[0][0]: probability of being the first state (only state 0)
# - H matrix
H = np.zeros((len(seq), Q)) # H[t][j]: entropy of being in state j at time t

# Recursion
for t in range(1, len(seq)):
    for j in range(Q):
        # c computing
        c = 0
        num = 0
        den = 0
        for i in range(Q):
            if G[i][j] != 0:
                num += C[t-1][i] * G[i][j][seq[t]]
        for k in range(Q):
            for i in range(Q):
                if G[i][k] != 0:
                    den += C[t-1][i] * G[i][k][seq[t]]
        if den == 0:
            c = 0
        else:
            c = num / den
        C[t][j] = c

        # p computing (for H computing)
        P = []
        for i in range(Q):
            num = 0
            den = 0
            if G[i][j] != 0:
                num = C[t-1][i] * G[i][j][seq[t]]
            for k in range(Q):
                if G[k][j] != 0:
                    den += C[t-1][k] * G[k][j][seq[t]]
            if den == 0:
                p = 0
            else:
                p = num / den
            P.append(p)

        # H computing
        h = 0
        sum1 = 0; sum2 = 0
        for i in range(Q):
            sum1 += H[t-1][i] * P[i]
        for i in range(Q):
            if P[i] != 0:
                sum2 += P[i] * math.log2(P[i]) 
        h = sum1 - sum2
        H[t][j] = h


print('C matrix:')
print(C)
print('-'*60)
print('H matrix:')
print(H)