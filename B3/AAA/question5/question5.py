import numpy as np
import matplotlib.pyplot as plt
from scipy.stats import norm

GAMMA = 1.0
EPSILON = 1e-10

# Leer datos de un fichero
data = np.loadtxt('../question4/data_question4.txt')

# Parámetros iniciales
initial_means = np.array([-5, 7, 2])
initial_std_devs = np.array([4, 4, 4])
known_weights = np.array([0.6, 0.4, 0.0])
known_std_devs = np.array([4, 4, 4])
known_means = np.array([-5, 7, 2])
n_components = len(initial_means)

# Inicialización de pesos
weights = np.ones(n_components) / n_components

def e_step(data, weights, means, std_devs):
    responsibilities = np.zeros((data.shape[0], n_components))
    for k in range(n_components):
        responsibilities[:, k] = weights[k] * norm.pdf(data, means[k], std_devs[k])
    responsibilities_sum = responsibilities.sum(axis=1, keepdims=True) + EPSILON
    responsibilities /= responsibilities_sum
    return responsibilities

def m_step(data, responsibilities, weights, means):  # con regularización
    # Actualizar los pesos con regularización
    responsibilities = np.clip(responsibilities, EPSILON, None)  # Evitar logaritmos de cero
    weighted_responsibilities = responsibilities * (1 + GAMMA * np.log(responsibilities))
    
    # Actualizar pesos
    weights = weighted_responsibilities.sum(axis=0)
    weights /= weights.sum()  # Normalizar los pesos

    # Actualizar medias
    weighted_sum = weighted_responsibilities.T @ data
    means = weighted_sum / weighted_responsibilities.sum(axis=0)

    return weights, means

def log_likelihood(data, weights, means, std_devs):
    likelihood = np.zeros_like(data)
    for k in range(n_components):
        likelihood += weights[k] * norm.pdf(data, means[k], std_devs[k])
    likelihood = np.clip(likelihood, EPSILON, None)  # Evitar logaritmos de cero
    return np.sum(np.log(likelihood))

def em_algorithm(data, weights, means, std_devs, tol=1e-6, max_iter=100):
    log_likelihoods = []
    for iter in range(max_iter):
        # Imprimir los pesos y medias en cada iteración en un fichero
        with open('parameters.txt', 'a') as f:
            f.write(f"Iteración {iter+1}:\n")
            f.write(f"Pesos: {weights}\n")
            f.write(f"Medias: {means}\n")
            f.write("\n")

        responsibilities = e_step(data, weights, means, std_devs)
        weights, means = m_step(data, responsibilities, weights, means)
        log_likelihoods.append(log_likelihood(data, weights, means, std_devs))
        if len(log_likelihoods) > 1 and np.abs(log_likelihoods[-1] - log_likelihoods[-2]) < tol:
            break
    print("Número de iteraciones:", iter + 1)
    return weights, means, log_likelihoods

# Ejecutar el algoritmo EM
weights, means, log_likelihoods = em_algorithm(data, weights, initial_means, initial_std_devs)

# Mostrar los pesos y medias obtenidos
print("Pesos obtenidos:", weights)
print("Medias obtenidas:", means)

# Graficar el log-likelihood
plt.plot(log_likelihoods)
plt.title('Log-Likelihood durante el EM regularizado')
plt.xlabel('Iteración')
plt.ylabel('Log-Likelihood')
plt.show()

# Graficar datos ---------------------------------------------------------------
plt.figure(figsize=(8, 4))
plt.hist(data, bins=50, density=True, alpha=0.6, color='palegreen', edgecolor='black', linewidth=0.5, width=0.5)
# Graficar histograma ORIGINAL -------------------------------------------------
# Mixtura de las 3 gaussianas
x = np.linspace(min(data), max(data), 1000)
mixture_pdf = (known_weights[0] * (1/(known_std_devs[0] * np.sqrt(2 * np.pi))) * np.exp(-0.5 * ((x - known_means[0])/known_std_devs[0])**2) +
               known_weights[1] * (1/(known_std_devs[1] * np.sqrt(2 * np.pi))) * np.exp(-0.5 * ((x - known_means[1])/known_std_devs[1])**2))
plt.plot(x, mixture_pdf, color='red', label='Mixtura original', linewidth=1)

# Gaussianas individuales
y1 = known_weights[0] * (1/(known_std_devs[0] * np.sqrt(2 * np.pi))) * np.exp(-0.5 * ((x - known_means[0])/known_std_devs[0])**2)
y2 = known_weights[1] * (1/(known_std_devs[1] * np.sqrt(2 * np.pi))) * np.exp(-0.5 * ((x - known_means[1])/known_std_devs[1])**2)
plt.plot(x, y1, color='black', linestyle='--', linewidth=0.6)
plt.text(-4, 0.06, 'θ₁', fontsize=12)
plt.plot(x, y2, color='black', linestyle='--', linewidth=0.6)
plt.text(12, 0.02, 'θ₂', fontsize=12)

# Graficar histograma ESTIMADO -------------------------------------------------
x = np.linspace(min(data), max(data), 1000)
# Mixtura de Gaussianas ajustada
mixture_pdf_est = np.zeros_like(x)
for k in range(n_components):
    mixture_pdf_est += weights[k] * norm.pdf(x, means[k], initial_std_devs[k])
plt.plot(x, mixture_pdf_est, color='blue', label='Mixtura estimada', linewidth=1)

# Gaussianas individuales
y0 = weights[0] * norm.pdf(x, means[0], initial_std_devs[0])
y1 = weights[1] * norm.pdf(x, means[1], initial_std_devs[1])
y2 = weights[2] * norm.pdf(x, means[2], initial_std_devs[2])
plt.plot(x, y0, color='blue', linestyle='-.', linewidth=0.8)
plt.plot(x, y1, color='blue', linestyle='-.', linewidth=0.8)
plt.plot(x, y2, color='blue', linestyle='-.', linewidth=0.8)
plt.text(means[0], 0.04, 'θ\u0305₁', fontsize=12, color='blue')
plt.text(means[1], 0.02, 'θ\u0305₂', fontsize=12, color='blue')
plt.text(means[2], 0.01, 'θ\u0305₃', fontsize=12, color='blue')

# Tamaño de los valores de los ejes
plt.xticks(fontsize=10)
plt.yticks(fontsize=10)
plt.legend()

plt.savefig('question5_gamma1.png', dpi=300)
plt.show()
