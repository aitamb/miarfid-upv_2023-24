# Técnicas de Inteligencia Artificial
## Trabajo final
### Descripción inicial del problema
Se pretende cubrir la superficie de un terreno con placas solares. Estas placas 
tienen todas las misma características: misma superficie y obtienen energía a 
partir de la luz solar (kWh).

El terreno está distribuido en zonas donde se pueden colocar o no las placas 
solares. Cada zona tiene asociado un porcentaje de luz que recibe a lo largo del 
día además de un coste de instalación. Es decir, debido a las características 
del solar, no vale lo mismo colocar una placa solar en una zona que en otra.
La división del terreno se hará en zonas del mismo tamaño que las placas.

El objetivo es encontrar la mejor distribución de las placas en el terreno para 
maximizar la cantidad de energía obtenida, dado un presupuesto fijo 
(posiblemente ampliable a minimizar este presupuesto).

### Diseño de las soluciones
Cada individuo es una solución del problema.
La solución serán las ubicaciones de las placas solares.

La descripción del problema vendrá dado por un diccionario en el cual, para 
cada clave (que identificará a una zona en el mapa) el valor será una tupla
(porcentaje de luz que recibe la zona, coste de instalación de la placa en la
zona). 

La solución será un vector de tamañon N, siendo N el número de zonas en las
cuales se divide el terreno. Cada posición representará cada una de las zonas
y el valor de dicha posición será 1 (si se coloca una placa en esa zona) o 0
(si no se coloca placa en esa zona).

Por ejemplo [0,1,1,0,0,1,0,1] --> se colocarán placas en las zonas 1,2,5 y 7.
El coste de la función objetivo será 

Hay alguna forma de evitar que se generen soluciones no factibles (que superen 
el presupueso). A lo mejor numerarlas zonas del solar en orden descendente de 
coste para saber qué combinaciones de zonas superan el presupuesto.

Similar al problema de la mochila.