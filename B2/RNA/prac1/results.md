# RNA - Práctica 1
## Pruebas realizadas con DA, BN y LR Scheduler
En el directorio *results* se encuentran los *outputs* de las diferentes pruebas
de los hiperparámetros para la práctica 1. En *scripts* están los ficheros de
los experimentos con las modificaciones que se indican a continuación.

### Test 1
75 epochs. *mnist_mlp_4_da.py* sin modificaciones.

* Best Accuracy **98.94**

### Test 2
75 epochs. Se ha añadido la transformación *ElasticTransform(alpha=110.0)* 
con una probabilidad del 50%, a partir de la siguiente función:
```
import random

def rand_func():
    if random.random() < 0.5:
        return transforms.ElasticTransform(alpha=110.0)
    else:
        return transforms.RandomRotation(0)

train_transform = transforms.Compose(
                    [transforms.RandomRotation((-15, 10)),
                    transforms.RandomAffine(degrees=2, 
                    translate=(0.02,0.01), 
                    scale=(0.9, 1.1)),
                    rand_func(),
                    transforms.ToTensor()])
```

* Best Accuracy **99.39** in epoch 61 (99.36 en epoch 36)

### Test 3.1
35 epochs. Se han añadido al anterior las transformaciones *gaussianBlur* y 
*randomInvert*.
Va MUY lento, no he llegado a acabar la ejecución porque no estaba dando buenos
resultados.

### Test 3.2
40 epochs. De las anteriores, solamente con la transformación *randomInvert*. 
Tampoco lo he llegado a acabar de ejecutar.

### Test 4
A partir del *test 3.1* se ha añadido un factor de "aleatoriedad" del 50% a 
ambas transformaciones. Además, se ha utilizado el *scheduler* ReduceLROnPlateau
con los valores por defecto.

* Best Accuracy **98.53** in epoch 42

### Test 5
Igual que el *test 2* pero con *GaussianBlur(kernel_size=3)*

* Best Accuracy **99.21** in epoch 27

### Test 6 
Se ha modificado la función del *test 2* de la siguiente manera:
```
ef rand_func2():
    if random.random() < 0.5:
        return transforms.GaussianBlur(kernel_size=3)
    else:
        return transforms.RandomRotation(0)

train_transform = transforms.Compose(
                    [
                    transforms.RandomRotation(3),
                    transforms.RandomAffine(degrees=2, 
                    translate=(0.002,0.001), scale=(0.001, 1.64)),
                    transforms.ElasticTransform(alpha=150.0),
                    rand_func2(),
                    transforms.ToTensor(),
                    ])
```

* Best Accuracy **98.32** in epoch 64

### Test 7
Como el *test 5* pero el *scheduler* ReduceLROnPlateau por defecto.

* Best Accuracy **99.03** in epoch 66

### Test 8
Como el *test 2* pero el *scheduler* ReduceLROnPlateau por defecto.

* Best Accuracy **99.22** in epoch 56

### Test 9
Como el *test 2* pero sin el factor de aleatoriedad.

* Best Accuracy **99.17** in epoch 63

### Test 10
Se han eliminado la transformación *RandomAffine* y la aleatoriedad del. Se ha 
añadido el *scheduler* ReduceLROnPlateau con la configuración modificada 
(patience=7, threshold=0.001 y cooldown=1).
He quitado un tipo de transformacion, creo que la rotacion o la affine
He puesto onPlateau y he quitado el random al *ElasticTransform*. 

* Best Accuracy **99.21** in epoch 55

### Test 11
Igual que el *test 10* pero la paciencia del ReduceLROnPlateau igual a 0.

* Best Accuracy **98.5** in epoch 5

### Test 12
Como el *test 10* pero el *learning rate* inicial es igual a 0.1 y los valores 
del *scheduler* ReduceLROnPlateau son patience = o y factor = 0.9

* Best Accuracy **99.37** in epoch 63

### Test 13
Como el *test 12* pero con 100 epochs.

* Best Accuracy **99.43**  in epoch 99

## Final
Con el *test 13* se ha obtenido el mejor resultado hasta el momento. Se ha
guardado el modelo en *models*.