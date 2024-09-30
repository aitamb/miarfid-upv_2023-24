# Calculate derivational entropy for a given graph

import numpy as np

# We have a graph G with n nodes and m edges
# We will calculate the derivational entropy of the graph
# like this H = (I -M)^-1 * ksi
# where I is the identity matrix, M is the adjacency matrix of the graph and the vector ksi

# Characteristic matrix of the graph
M = np.array([[0.0, 0.2, 0.3, 0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0],  # 0
              c]) # 9

# Identity matrix
I = np.identity(10)

# Ksi vector
ksi = np.ones(10)
for i in range(9):
    value = 0
    for m in M[i]:
        if m != 0:
            value += m * np.log2(m)
    ksi[i] = -value
ksi[9] = 0

# Compute the entropy
S = np.linalg.inv((I - M))
H = S.dot(ksi)

# print with 3 decimals
print("The derivational entropy of the graph is: ", H[0])