###############################################################################
# Aitana Menárguez Box                                                        #
# EJERCICIO 1 PYTHON                                                          #
# Objetivo: Repaso de estructuras básicas de python                           #
# (listas, tuplas, diccionarios, …) y su sintaxis.                            #
###############################################################################

cadena="El/DT perro/N come/V carne/N de/P la/DT carnicería/N y/C de/P la/DT nevera/N y/C canta/V el/DT la/N la/N la/N ./Fp"

# Separar CATEGORIAS
# por cada elemento (palabra/categoria), quedarse con la categoria ([1])
categorias=[x[1] for x in [y.split('/') for y in cadena.split()]]

# Separar PALABRAS
# por cada elemento (palabra/categoria), quedarse con la palabra ([0])
palabras=[x[0] for x in [y.split('/') for y in cadena.lower().split()]]

# Ejercicio 1.1 ###############################################################
# Obtener un diccionario, que para cada categoria, muestre su frecuencia. 
# Ordenar el resultado alfabéticamente por categoria.            

# Contar frecuencia de cada categoria y crear un diccionario iterando 
# directamente sobre categorias (ordenadas alfabeticamente)
dic1 = {x:categorias.count(x) for x in sorted(set(categorias))}

# Mostrar el resultado
print("EJERCICIO 1 ----------------------------------------------------------")
for x in dic1:
    print(x, dic1[x])
print('')

# Ejercicio 1.2 ###############################################################
# Obtener un diccionario, que para cada palabra, muestre su frecuencia, y una 
# lista de sus categorias morfosintácticas con su respectiva frecuencia. 
# Mostrar el resultado ordenado alfabeticamente por palabra.

# Contar frecuencia de cada palabra y crear un diccionario iterando 
# directamente sobre palabras (ordenadas alfabeticamente)
dic2 = {x:[palabras.count(x),{}] for x in sorted(set(palabras))}

# Completar el diccionario
for pal in dic2:
    # obtener indices de pal en la lista de palabras
    indices = [i for i, x in enumerate(palabras) if x == pal]
    for i in indices:
        # los indices de palabras se corresponden con las categorias
        # el metodo get obtiene el valor de la clave categorias[i] si existe
        # si no, da resultado 0 para poder sumar 1 aparicion de la categoria
        dic2[pal][1][categorias[i]] = dic2[pal][1].get(categorias[i],0)+1

# Mostrar el resultado
print("EJERCICIO 2 ----------------------------------------------------------")
for x in dic2: # por cada palabra
    # mostrar su frecuencia
    print(x, dic2.get(x)[0], end = " ")
    # mostrar cada categoria y su frecuencia para la palabra
    for cat in dic2.get(x)[1].keys():
        print(cat, dic2.get(x)[1].get(cat), end = " ")
    print('')
print('')

# Ejercicio 1.3 ###############################################################
# Calcular la frecuencia de los todos los bigramas de la cadena, considerar 
# un simbolo inicial <S> y un simbolo final </S> para la cadena.

# Anyadir simbolo inicial y final a la cadena
# en nueva variable para poder usar la original mas adelante
categorias3 = categorias.copy(); 
categorias3.insert(0,"<S>"); categorias3.append("</S>")

# Lista con los bigramas de categorias
bigr = [(categorias3[i],categorias3[i+1]) for i in range(len(categorias3)-1)]

# Diccionario con la frecuencia de bigramas de categorias (ordenadas)
dic3 = {x:bigr.count(x) for x in sorted(set(bigr))}

# Mostrar el resultado
print("EJERCICIO 3 ----------------------------------------------------------")
for x in dic3:
    print(x, dic3.get(x))
print('')

# Ejercicio 1.4 ###############################################################
# Construir una funcion que devuelva las probabilidades lexicas P(C|w) y de 
# emision P(w|C) para una palabra dada (w) para todas sus categorias (C) que 
# aparecen en el diccionario construido anteriormente. Si la palabra no existe 
# en el diccionario debe decir que la palabra es desconocida.

# Crear la funcion que calcula las probabilidades para una palabra dada
def prob(pal):
    # si NO EXISTE la palabra, devolver codigo de error
    if pal not in palabras: 
        print("La palabra indicada es desconocida")
        return
    # si EXISTE, realizar calculo
    # numero de palabras
    n = len(palabras)
    # probabilidad de la palabra P(w)
    pw = dic2.get(pal)[0] / n
    # calcular probabilidades para cada categoria
    for cat in dic2.get(pal)[1]:
        # probabilidad de la categoria P(C)
        pc = dic1.get(cat) / n
        # probabilidad conjunta P(C,w)
        pcw = dic2.get(pal)[1].get(cat) / n
        # probabilidad lexica P(C|w)
        pl = pcw / pw
        # probabilidad de emision P(w|C)
        pe = pcw / pc
        # mostrar resultado
        print("P (%s | % s) = %f" %(cat, pal, pl))
        print("P (%s | % s) = %f" %(pal, cat, pe))
        
# Codigo para poder realizar pruebas
print("EJERCICIO 4 ----------------------------------------------------------")
print("Escribe una palabra para calcular su probabilidades.")
print("Para salir, escribe un salto de línea.")
palabra = input("Palabra: ").lower()
# Mientras no se escriba un salto de línea, calcular probabilidades
while palabra != '':
    prob(palabra)
    print('')
    palabra = input("Palabra:").lower()