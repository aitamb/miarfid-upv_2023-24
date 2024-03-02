###############################################################################
# Aitana Menárguez Box                                                        #
# EJERCICIO 1 Expresiones regulares                                           #
# Objetivo: Manipulación de cadenas y uso de expresiones regulares para       #
# construir un tokenizador para el español con una serie de restricciones.    #
###############################################################################

import re

entrada = "Sr. y Sra. López, Dª. Dolores Peris y Dr. Pérez, vengan el 12  de  abril de   2023 a las 17:30h;  si no pueden venir, nos los comunican en la  web http://www_xxx.ss.com o vienen el 14 de abril de 2023 a las 9:00h.\
    \nOMG 😱 No puedo creer que ya es viernes 🎉. A salir a romperla 🍻 con mis panas 💃🕺... ¡¡¡¡Que empiece el fin de semana!!!! 🤘#PorFinEsViernes #Aleluya @pepito.\
    \nLa caja pesa 12.5 Kg y mide 55,5 cm de largo, 35.5 cm de ancho y 40.5 cm de alto, por lo tanto, el importe del transporte es de 15,67 euros.\
    \nTengo ganas de cenar, pide: 4 tercios y una pizza... ah!, no te olvides del postre.\
    \nTodo lo que sigue son ejemplos de acrónimos que no se deberían separar: EE.UU., S.L., CC.OO., S.A., U.R.S.S., aunque también se pueden ser EEUU, SL., SA., URSS, ...\
    \nLa ONU fue fundada el 24 de octubre de 1945 y se encarga de mantener la paz y seguridad mundial.\
    \nLa OMS está trabajando arduamente para combatir la pandemia de COVID-19 en todo el mundo.\
    \n1 de enero, 2 de febrero, 3 de marzo, 4 de abril, 5 de mayo, 6 de junio, 7 de julio San Fermin.\
    \nEsta comarca tiene 1/4 de su extensión en aguam 1/2 de monatañas y el 25%  restante es de tierras cultivas, en total 12000 km2.\
    \nEl \"bote\" está lleno, 'vacio' no 'semi-vacio'.\
    \nD. Antonio Pérez Delgado, Dª. Maria Olivares Sempere, D. Juan Alonso Rodriguez, presentense al despacho del Sr. Director.\
    \nPerdón, se me olvidaba, mi correo es fpla@dsic.upv.es y la web http://users.dsic.upv.es/~fpla/  ha cambiado,  ahora es http://personales.upv.es/~fpla/ "

# reg_ex = """
#     ([A-Z][a-z]+\\s[A-Z][a-z]+\\s[A-Z][a-z]+)|
#     (Sr\\.|Sra\\.|Srta\\.|Dr\\.|Dra\\.|D\\.|Dª\\.*)|
#     (\\w+@(\\w+\\.)+(com|es))|((http:\\/\\/(\\w+\\.)+)(es|com)(~|\\w|\\/)*)|(@\\w+)|(#(\\w|\\d)+)|((\\d{1,2}-\\d{1,2}-\\d{4})|(\\d{1,2}\\/\\d{1,2}\\/\\d{4}))|(\\d{1,2} de (enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|octubre|noviembre|diciembre)+( de \\d+)*)|(\\d{1,2}:\\d{2}h*)|(([A-Z]{1,2}\\.){2,})|([A-Z]+([A-Z])+\\.*)|(\\d+(,|\\.)\\d+)|(([\"\'])*([a-z]|[A-Z]|[ñáéíóú-])+\\28)|(\\d+\\/\\d+|\\d+%)|(\\d+)|([\\(\\)\\.,‘“\\?¿!¡…;:])|(\\S.)
#     """

pattern=re.compile (r"((([A-Z]|[ÁÉÍÓÚ]){1}([a-z]|[áéíóú])+)\s"       +    # Nombre Apellido Apellido
                    r"(([A-Z]|[ÁÉÍÓÚ]){1}([a-z]|[áéíóú])+)\s"        +    # -
                    r"(([A-Z]|[ÁÉÍÓÚ]){1}([a-z]|[áéíóú])+))|"        +    # -
                    r"((Sr|Sra|Srta|Dr|Dra|D|Dª)\.)|"                +    # Tratamientos
                    r"(\w+@(\w+\.)+(com|es))|"                       +    # Correo electronico
                    r"((http:\/\/(\w+\.)+)(es|com)(~|\w|\/)*)|"      +    # URL
                    r"(@\w+)|"                                       +    # Menciones
                    r"(#(\w|\d)+)|"                                  +    # Hashtags
                    r"((\d{1,2}-\d{1,2}-\d{4})|"                     +    # Fecha dd-mm-aaaa
                    r"(\d{1,2}\/\d{1,2}\/\d{4}))|"                   +    # Fecha dd/mm/aaaa    
                    r"(\d{1,2}\s+de\s+(enero|febrero|marzo|abril|"   +    # Fecha dd de mes de aaaa
                    r"mayo|junio|julio|agosto|septiembre|"           +    # -
                    r"octubre|noviembre|diciembre)+(\s+de\s+\d+)*)|" +    # -
                    r"(\d{1,2}:\d{2})|"                              +    # Hora hh:mm
                    r"(([A-Z]{1,2}\.){2,})|([A-Z]+([A-Z])+\.*)|"     +    # Acronimos
                    r"(\d+(,|\.)\d+)|"                               +    # Numeros con decimales
                    r"(([a-z]|[A-Z]|[ñáéíóú-])+)|"                   +    # Palabras (y entre comillas)
                    r"(\d+\/\d+|\d+[%]*)|"                           +    # Numeros, fracciones y porcentajes
                    r"(\.\.\.)|"                                     +    # Puntos suspensivos
                    r"([\(\)\.,'\"\?¿!;:])|"                         +    # Otros signos de puntuacion
                    r"(\S)"                                               # Cualquier otro caracter (emojis)
                    )

for e in entrada.split("\n"): # Cada linea es una frase a tokenizar
    print("## " + e) # Imprimir linea entera
    for token in pattern.finditer(e):
        print(token.group())