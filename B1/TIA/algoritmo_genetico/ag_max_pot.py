###############################################################################
# ALGORITMO GENETICO - MAXIMIZAR  POTENCIA DE INSTALACION SOLAR               #
# AITANA MENARGUEZ BOX                                                        #
###############################################################################

import random
import math
import operator
import numpy as np
import time

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
# *Disenyo, codificacion y de decodificacion de los individuos (soluciones):
# sera una lista de N elementos, cada elemento sera 0 o 1 dependiendo de si
# hay placa o no en esa posicion

# - generacion aleatoria de individuos
def generarIndividuo(i):
    individuo = []
    costo = 0
    for j in range(N): # cada individuo tiene N genes (posibles posiciones)
        random.seed(i + j/(i+1))
        individuo.append(random.randint(0,1))
        if g_type == 1 and individuo[j] == 1: # si es aleatoria con costo
            costo += calculo_coste(j, precio_p)
            if costo > presupuesto:
                individuo[j] = 0
                costo -= calculo_coste(j, precio_p)
    return individuo

# *Poblacion inicial
def poblacionInicial():
    poblacion = []
    # - generacion aleatoria de individuos
    for i in range(n_ind): # generar n_ind individuos
        individuo = generarIndividuo(i)
        poblacion.append(individuo) # anyadir individuo a la poblacion

    return poblacion

# *Aptitud:
# evaluar fitness de cada individuo
def fitness(ind):
    potencia = 0
    costo = 0
    for i in range(len(ind)): # calcular costo y potencia para el individuo
        if ind[i] == 1:
            potencia += calculo_potencia(i, potencia_p)
            costo += calculo_coste(i, precio_p)

    # devolver la potencia generada (se quiere maximizar)
    # con mucha penalizacion si se supera el presupuesto
    # tambien se devuelven la potencia y el costo de dicho individuo
    if costo > presupuesto:
        penalizacion = costo *100000
    else: penalizacion = 0
    return potencia, costo, potencia - penalizacion

# *Nueva poblacion
# - seleccion de padres (elitista)
def seleccion(poblacion):
    # ordenar poblacion por fitness
    poblacion = ordenar(poblacion)
    if s_type == 0: # elitista (0)
        # seleccionar la mejor mitad de la poblacion
        return poblacion[:math.floor(n_ind*0.5)]
    else: # ruleta (1)
        # calcular la suma de todos los fitness
        suma = 0
        for ind in poblacion:
            suma += fitness(ind)[0]
        # calcular la probabilidad de cada individuo
        prob = []
        for ind in poblacion:
            prob.append(fitness(ind)[0]/suma)
        # seleccionar los padres
        padres = []
        for i in range(math.floor(n_ind*0.5)):
            padres.append(poblacion[np.random.choice(len(poblacion), p=prob)])
        return padres
    
# - cruce
def cruce(padres): # dos puntos, por parejas
    # de cada dos 
    hijos = []
    grupo = N//3
    for i in range(len(padres)-1):
        padre1 = padres[i]
        padre2 = padres[i+1]
        hijo1 = padre1[:grupo] + padre2[grupo:2*grupo] + padre1[2*grupo:]
        hijo2 = padre2[:grupo] + padre1[grupo:2*grupo] + padre2[2*grupo:]
        hijos.append(hijo1)
        hijos.append(hijo2)

    return hijos

# - mutacion
def mutacion(hijos):
    for i in range(len(hijos)):
        for j in range(N):
            random.seed(i+j)
            if random.uniform(0,1) < 0.1:
                hijos[i][j] = 1 - hijos[i][j]

    return hijos

# - reemplazo generacional elitista
def reemplazo(poblacion, hijos):
    # elegir mejores individuos tanto de la poblacion como de los hijos
    poblacion = poblacion + hijos
    poblacion = ordenar(poblacion)
    return poblacion[:n_ind]

# AUXILIARES ##################################################################
def ordenar(poblacion): # ordenar poblacion por fitness
    pob = {fitness(ind)[2]: ind for ind in poblacion}
    pob = sorted(pob.items(), key=operator.itemgetter(0), reverse=True)

    return [ind for _, ind in pob]

# PARAMETROS ##################################################################
# Lectura de fichero de datos
parametros = np.loadtxt("parametros.txt")
print("------------------------------------------------------------------")
for line in parametros: # para cada linea del fichero, ejecutar algoritmo
    # Mejor individuo
    best_ind = []
    best_cost = 0
    best_pot = 0
    best_gen = 0
    best_fit = 0

    # Numero de individuos
    n_ind = int(line[0])

    # Numero de generaciones
    n_gen = int(line[1])

    # Tipo de generacion
    g_type = int(line[2]) # 0: aleatoria, 1: aleatoria con costo

    # Gestion de restricciones en la evaluacion
    ev_type = int(line[3]) # 0: reparacion, 1: descarte
    it = int(line[4]) # iteraciones para la reparacion

    # Tipo de seleccion
    s_type = int(line[5]) # 0: elitista, 1: ruleta

    # ALGORITMO ###############################################################
    # Ejecucion de todos los pasos del algoritmo genetico
    gen = 1; evolucionar = True
    # se inicia el contador
    inicio = time.time()
    # se define una poblacion inicial
    poblacion = poblacionInicial()

    # se inicializa el mejor individuo
    poblacion = ordenar(poblacion)
    best_ind = poblacion[0]; best_gen = gen
    best_pot, best_cost, best_fit = fitness(best_ind)

    while evolucionar:
        # se evalua la aptitud (fitness) de cada individuo en la poblacion
        for i, ind in enumerate(poblacion):
            potencia, costo, fit = fitness(ind)
            # gestion de restricciones
            if costo > presupuesto:
                if ev_type == 0: # reparacion
                    for i in range(it):
                        pos = N-1-i # quitar los de mas coste, que estan al final
                        if ind[pos] == 1:
                            ind[pos] = 0
                            costo -= calculo_coste(pos, precio_p)
                            potencia -= calculo_potencia(pos, potencia_p)
                            if costo <= presupuesto:
                                break
                else: # descarte, se crea nuevo individuo aleatorio
                    poblacion[poblacion.index(ind)] = generarIndividuo(random.randint(0,n_ind))

        # se eleccionan los padres segun probabilida de cruce
        padres = seleccion(poblacion)

        # se cruzan los padres para obtener los hijos
        hijos = cruce(padres)

        # se mutan los hijos con una probabilidad
        hijos = mutacion(hijos)

        # se reemplaza la poblacion a partir de los hijos
        poblacion = reemplazo(poblacion, hijos)

        # se actualiza el mejor individuo
        poblacion = ordenar(poblacion)
        if fitness(poblacion[0])[2] > fitness(best_ind)[2]:
            best_ind = poblacion[0]; best_gen = gen
            best_pot, best_cost, best_fit = fitness(best_ind)

        # se cambia de generacion 
        gen += 1

        # se comprueba la condicion de parada (el numero de generaciones)
        if gen > n_gen:
            evolucionar = False

    # se para el contador
    fin = time.time()

    # RESULTADOS ##################################################################
    # Se muestra informacion general
    print("n_gen:", gen - 1)
    print("n_ind:", n_ind)
    print("N:", N)
    print("g_type:", g_type)
    print("s_type:", s_type)
    print("ev_type:", ev_type)
    if ev_type == 0:
        print("it:", it)
    print("tiempo_total:", fin - inicio)

    # Se muestra el mejor individuo
    print("best_pot:", best_pot)
    print("best_cost:", best_cost)
    print("best_gen:", best_gen)
    print("best_fit:", best_fit)
    # print("best_ind:", best_ind)

    print("------------------------------------------------------------------")