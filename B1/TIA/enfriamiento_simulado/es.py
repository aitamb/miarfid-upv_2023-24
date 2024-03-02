###############################################################################
# ALGORITMO ENFRIAMIENTO SIMULADO - MAXIMIZAR POTENCIA DE INSTALACION SOLAR   #
# AITANA MENARGUEZ BOX                                                        #
###############################################################################

import random
import math
import time
import numpy as np

# DESCRIPCION ##################################################################
# Caracteristicas de las placas solares
ancho_p = 1.7; alto_p = 1; precio_p = 500; potencia_p = 300; mano_obra = 15

# Caracteristicas del terreno
ancho_t = 17; alto_t = 10
N = math.floor((ancho_t/ancho_p) * (alto_t/alto_p)) # Posibles pos de placas

def calculo_potencia(n, potencia_p): # La potencia depende de la posicion
    random.seed(n+1)
    return (random.randint(1,5) * 0.25 * potencia_p)

def calculo_coste(n, precio_p): # El precio depende de la posicion
    random.seed(n-1)
    return precio_p + mano_obra * (0.2 * random.randint(1,5))

# Restricciones:
presupuesto = 20000

# Objetivo:
# Se quiere MAXIMIZAR la POTENCIA SIN SUPERAR el PRESUPUESTO

# FUNCIONES ###################################################################

def generar_sol_inicial(): # Genera una solucion inicial aleatoria (poner la del AG)
    sol = [0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 
           0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 
           1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 
           1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1]

    return sol

def generar_vec_aleatorio(sol_act): # Genera un vecino aleatorio de la solucion
    sol = sol_act.copy()
    # cantidad de cambios aleatorios
    random.seed()
    n_cambios = random.randint(1, N)
    # reaizar cambios aleatorios
    for i in range(n_cambios):
        random.seed()
        j = random.randint(0, N-1) # posicion a cambiar aleatoria
        random.seed()
        sol[j] = random.randint(0,1) # cambio aleatorio

    return sol

def f(ind):
    potencia = 0
    costo = 0
    for i in range(len(ind)): # calcular costo y potencia para el individuo
        if ind[i] == 1:
            potencia += calculo_potencia(i, potencia_p)
            costo += calculo_coste(i, precio_p)

    # devolver la potencia generada (se quiere maximizar) penalizada
    # con mucha penalizacion si se supera el presupuesto
    # tambien se devuelven la potencia y el costo de dicho individuo
    if costo > presupuesto:
        penalizacion = costo * 100000
    else: penalizacion = 0
    return potencia, costo, potencia - penalizacion

def omega(T,k): # Decremento de la temperatura
    return T*k

# Lectura de fichero de datos
parametros = np.loadtxt("parametros.txt")
print("#no. | T0 | k | L | tiempo | best_f | best_cost | best_pot | best_it | best_t | best_ind#")
for no,line in enumerate(parametros): # para cada linea del fichero, ejecutar algoritmo
    # PARAMETROS ##################################################################
    # Mejor individuo
    best_ind = []
    best_cost = 0
    best_pot = 0
    best_f = 0
    best_it = 0
    best_t = 0

    # Temperatura inicial
    T0 = line[0]
    # Decremento de la temperatura
    k = line[1]
    # Iteraciones en cada temperatura
    L = int(line[2])

    # ALGORITMO ###############################################################
    # Ejecucion del enfriamiento simulado
    inicio = time.time()
    # Inicializacion de variables
    T = T0
    # solucion inicial
    sol_act = generar_sol_inicial()
    # guardar el mejor individuo
    best_ind = sol_act; best_t = T 
    best_pot, best_cost, best_f = f(sol_act)

    continuar = True
    # iterar hasta cumplir condicion de terminacion
    it = 0
    filename= "resultados/exp_" + str(no) + ".txt"
    with open(filename, 'w') as file:
        file.write("#T | best_f#\n")
        while continuar:
            it += 1
            for i in range(L): # L iteraciones para cada T
                # generar vecino aleatorio
                sol_cand = generar_vec_aleatorio(sol_act)
                # calcular f de cada solucion
                pot_act, cost_act, f_act = f(sol_act)
                pot_cand, cost_cand, f_cand = f(sol_cand)
                # calcular la diferencia de f
                delta = f_cand - f_act
                delta = delta/10000000000
                # si diferencia positiva (maximizar), aceptar solucion
                if delta > 0:
                    sol_act = sol_cand
                elif random.random() > math.exp(-delta/T):
                    sol_act = sol_cand
                # actualizar mejor solucion
                if f(sol_act)[2] > best_f:
                    best_ind = sol_act; best_t = T; best_it = it
                    best_pot, best_cost, best_f = f(sol_act)
            # actualizar temperatura
            T = omega(T,k)
            # comprobar condicion de terminacion
            if T < 0.01:
                continuar = False
            if continuar:
                write_line = str(T) + " " + str(best_f) + '\n'
                file.write(write_line)
    fin = time.time()
    tiempo = fin - inicio
    print(str(no), str(T0), str(k), str(L), str(tiempo), str(best_f), str(best_cost), str(best_pot), str(best_it), str(best_t), str(best_ind))