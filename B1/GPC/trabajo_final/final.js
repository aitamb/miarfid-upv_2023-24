// TRABAJO FINAL: auditorio
// Aitana Menárguez Box
 
// Cargar libreria
import * as THREE from '../lib/three.module.js';
import { OrbitControls } from "../lib/OrbitControls.module.js";
import {TWEEN} from "../lib/tween.module.min.js";
import {GUI} from "../lib/lil-gui.module.min.js";

// Variables estandar
let renderer, scene, camera;

// Variables globales
let silla, base;
let matcuero, matvelvet, video, matinvisible, matvelvet_o, matvelvet_sel;
let effectController;
let sound;
let focal1, focal2;
let pantalla;

// MEDIDAS de la ESCENA
const x_tarima = 900;
const y_tarima = 30;
const z_tarima = 570;
// Silla
const x_pata = 5;
const y_pata = 17;
const z_pata = 5;
const x_cojin = 22;
const y_cojin = 6;
const z_cojin = 20;
const x_respaldo = 22;
const y_respaldo = 30;
const z_respaldo = 5;

// Sala
const x_sala = x_tarima;
const y_sala = 500;
const z_sala = 700;
let arraySillas = [];

// Variables para la vista cenital
let cenital;
let minimapa = false;

// Acciones 
init();
loadScene();
setupGUI();
render();

// Creacion de las FUNCIONES
function init (){
    // Mensaje de bienvenida
    alert("Portal de reservas de asientos de cine\n\n");

    // Instanciar el motor de render
    renderer = new THREE.WebGLRenderer();
    renderer.setSize(window.innerWidth, window.innerHeight);
    renderer.setClearColor(0x111111);
    renderer.autoClear = false; // para que no borre cada vez que se llama al render
    document.getElementById('container').appendChild(renderer.domElement);
    // para anyadir el mapa de sombras...
    renderer.antialias = true;
    renderer.shadowMap.enabled = true;

    // Instanciar el nodo raiz de la escena
    scene = new THREE.Scene();

    // Instanciar la camara
    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 2000);
    camera.position.set(-10,130,200);
    // Control de camara
    const controls = new OrbitControls(camera, renderer.domElement);
    // Configuracion mapa de asientos
    setMiniMap();

    // Captura eventos
    // - redimension de la ventana
    window.addEventListener('resize', updateAspectRatio);
    // - cambio posicion camara
    controls.addEventListener('change', checkCameraPosition);
    // - teclas teclado
    document.addEventListener('keydown', onKeyDown);

    // Fuentes de luz
    // ambiental
    const ambiental = new THREE.AmbientLight(0x555555);
    scene.add(ambiental);
    // crear 2 focales, una a cada lado de la sala
    // focal 1
    focal1 = new THREE.SpotLight(0xFFFFFF, 0.5);
    focal1.position.set(-x_sala/2 + 15, y_sala - 15, 0);
    focal1.target.position.set(-x_sala/2, 0, 0);
    focal1.angle = Math.PI/4;
    focal1.penumbra = 0.2;
    focal1.castShadow = true;
    focal1.shadow.mapSize.width = 1024;
    focal1.shadow.mapSize.height = 1024;
    focal1.shadow.camera.near = 10;
    focal1.shadow.camera.far = 2000;
    scene.add(focal1);
    scene.add(focal1.target);
    // focal 2
    focal2 = new THREE.SpotLight(0xFFFFFF, 0.5);
    focal2.position.set(x_sala/2, y_sala, 0);
    focal2.target.position.set(x_sala/2, 0, 0);
    focal2.angle = Math.PI/4;
    focal2.penumbra = 0.2;
    focal2.castShadow = true;
    focal2.shadow.mapSize.width = 1024;
    focal2.shadow.mapSize.height = 1024;
    focal2.shadow.camera.near = 10;
    focal2.shadow.camera.far = 2000;
    scene.add(focal2);
    scene.add(focal2.target);
}

function loadScene(){
    // MATERIALES y TEXTURAS
    const path = "../textures_final/";

    // material basico
    const material = new THREE.MeshBasicMaterial({color: 'brown', wireframe: false});
    const material2 = new THREE.MeshBasicMaterial({color: 'blue', wireframe: false});

    // cuero negro
    const texcuero = new THREE.TextureLoader().load(path + "cuero_negro.jpg");
    matcuero = new THREE.MeshLambertMaterial({map: texcuero});

    // terciopelo
    const texvelvet = new THREE.TextureLoader().load(path + "velvet.jpg");
    matvelvet = new THREE.MeshLambertMaterial({map: texvelvet});

    // terciopelo ocupado
    const texvelvet_o = new THREE.TextureLoader().load(path + "velvet_o.jpg");
    matvelvet_o = new THREE.MeshLambertMaterial({map: texvelvet_o});

    // terciopelo seleccionado
    const texvelvet_sel = new THREE.TextureLoader().load(path + "velvet_sel.png");
    matvelvet_sel = new THREE.MeshLambertMaterial({map: texvelvet_sel});
    
    // madera
    const texmadera = new THREE.TextureLoader().load(path + "escenario.jpg");
    texmadera.repeat.set(3, 2);
    texmadera.wrapS = texmadera.wrapT = THREE.RepeatWrapping;
    const matmadera = new THREE.MeshLambertMaterial({map: texmadera});

    // moqueta
    const texmoqueta = new THREE.TextureLoader().load(path + "moqueta.jpg");
    texmoqueta.repeat.set(3,4);
    texmoqueta.wrapS = texmoqueta.wrapT = THREE.RepeatWrapping;
    const matmoqueta = new THREE.MeshLambertMaterial({map: texmoqueta});

    // negro
    const texnegro = new THREE.TextureLoader().load(path + "mas_negro.jpg");
    // const matnegro = new THREE.MeshLambertMaterial({map: texnegro});
    const matnegro = new THREE.MeshPhongMaterial({ color: 0x000000 });

    // invisible 
    matinvisible = new THREE.MeshBasicMaterial({ transparent: true, opacity: 0 });
    
    // GEOMETRIA
    // Tarima
    for(let i = 1; i < 3; i++) {
        let z = z_tarima/i;
        const tarima = new THREE.Mesh(new THREE.BoxGeometry(x_tarima,y_tarima,z), matmoqueta);
        tarima.position.set(0,y_tarima/2 + y_tarima * (i-1), z_tarima/2 - z/2);
        tarima.receiveShadow = tarima.castShadow = true;
        scene.add(tarima);
    }

    // Sillas
    colocar_sillas(); 

    // Anyadir mapa de entorno
    const paredes = [];
    paredes.push(new THREE.MeshBasicMaterial({
        side: THREE.BackSide,
        map: new THREE.TextureLoader().load(path+"px.jpg")}));
    paredes.push(new THREE.MeshBasicMaterial({
        side: THREE.BackSide,
        map: new THREE.TextureLoader().load(path+"nx.jpg")}));
    paredes.push(new THREE.MeshBasicMaterial({
        side: THREE.BackSide,
        map: new THREE.TextureLoader().load(path+"py.jpg")}));
    paredes.push(new THREE.MeshBasicMaterial({
        side: THREE.BackSide,
        map: new THREE.TextureLoader().load(path+"ny.jpg")}));
    paredes.push(new THREE.MeshBasicMaterial({
        side: THREE.BackSide,
        map: new THREE.TextureLoader().load(path+"pz.jpg")}));
    paredes.push(new THREE.MeshBasicMaterial({
        side: THREE.BackSide,
        map: new THREE.TextureLoader().load(path+"nz.jpg")}));

    const habitacion = new THREE.Mesh(new THREE.BoxGeometry(x_sala, y_sala, z_tarima+350), paredes)
    habitacion.position.y = y_sala/2;
    habitacion.position.z = -350/2;
    scene.add(habitacion);

    // Anyadir pelicula
    video = document.createElement('video');
    video.src = "../../videos/shrek.mp4";
    video.load();
    video.muted = true;
    const texvideo = new THREE.VideoTexture(video);
    pantalla  = new THREE.Mesh(
        new THREE.PlaneGeometry(x_sala - 30, y_sala - 47, 4, 4),
        new THREE.MeshBasicMaterial({map: texvideo}));
    pantalla.position.set(0, y_sala/2 + 47, -z_tarima - 60);
    scene.add(pantalla);

    // Gestion del audio
    // crear un AudioListener y anyadirlo a la camara
    const listener = new THREE.AudioListener();
    camera.add( listener );
    // crear un PositionalAudio
    sound = new THREE.PositionalAudio( listener );
    // cargar el sonido y elegirlo como el buffer del PositionalAudio
    const audioLoader = new THREE.AudioLoader();
    audioLoader.load( '../audios/shrek.mp3', function( buffer ) {
        sound.setBuffer( buffer );
        sound.setRefDistance( 40 );
    });
    pantalla.add(sound);
}

function render(){
    let side;

    renderer.clear();
    requestAnimationFrame(render);
    update();

    const ar = window.innerWidth / window.innerHeight;
    if(ar < 1) // width < height
        side = window.innerWidth/4;
    else
        side = window.innerHeight/4;

    // Vista normal
    renderer.setViewport(0,0, window.innerWidth,window.innerHeight);
    renderer.render(scene, camera);

    // Vista cenital
    if(minimapa) {
        renderer.setViewport(0, (window.innerHeight - 300), 400, 300);
        renderer.render(scene, cenital);
    }
}

// FUNCIONES
function setMiniMap() {
    // Creacion de la camara cenital
    const camaraOrto = new THREE.OrthographicCamera(-x_tarima/2, x_tarima/2, z_tarima/2, -z_tarima/2, -10, 1000)
    cenital = camaraOrto.clone();
    cenital.position.set(0,y_sala,0);
    cenital.lookAt(0,0,0);
    cenital.up = new THREE.Vector3(0,0,-1); // Cambia el vector UP de la camara porque mira hacia abajo
}

function updateAspectRatio() { // Callback redimension de ventana
    // Cambiar dimensiones del canvas
    renderer.setSize(window.innerWidth, window.innerHeight);

    // Nueva rel de aspecto de la camara
    const ar = window.innerWidth / window.innerHeight;

    // // perspectiva
    camera.aspect = ar;
    camera.updateProjectionMatrix();

    // Actualizar matriz de proyeccion
    cenital.updateProjectionMatrix();
}

function update() {
    TWEEN.update();
}

function checkCameraPosition() {
    // Comprobar que la camara no se salga de la escena
    const cameraPosition = camera.position;
    // para x
    if(cameraPosition.x > x_sala/2 - 5)
        cameraPosition.x = x_sala/2 - 5;
    else if(cameraPosition.x < -x_sala/2 + 5)
        cameraPosition.x = -x_sala/2 + 5;
    // para y
    if(cameraPosition.y > y_sala)
        cameraPosition.y = y_sala;
    else if(cameraPosition.y < 5)
        cameraPosition.y = 5;
    // para z
    if(cameraPosition.z > z_sala/2 - 5)
        cameraPosition.z = z_sala/2 - 5;
    else if(cameraPosition.z < -z_sala/2 + 5)
        cameraPosition.z = -z_sala/2 + 5;
}

function setupGUI()
{
	// Definicion de los controles
	effectController = {
        mapa: false,
		play: function(){
            sound.play();
            video.play();
        },
        pause: function(){video.pause(); sound.pause();},
        volumen: 1,
        fila: "-",
        butaca: "-",
        ir: function() { 
            irAlAsiento(effectController.fila, effectController.butaca);
        },
        luces: true,
        seleccionar: function() {
            seleccionarAsiento(effectController.fila, effectController.butaca);
        }
	};

	// Creacion interfaz
	const gui = new GUI();

	// Construccion del menu
	const h = gui.addFolder("Selección de asiento");
    h.add(effectController, "mapa").onChange(v => {
        minimapa = v;
    });
    h.add(effectController, "fila", 
        ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10"])
        .name("Fila");
    h.add(effectController, "butaca", 
        ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14",
         "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "26", "27", "28"])
        .name("Butaca");
    h.add(effectController, "ir").name("Ir al asiento");
    h.add(effectController, "seleccionar").name("Seleccionar/Eliminar asiento");

    const videofolder = gui.addFolder("Opciones de película");
    videofolder.add(effectController, "luces").onChange(v => {
        // encender o apagar las luces
        scene.traverse(function(h) {
            if (h instanceof THREE.SpotLight) {
                h.visible = v;
            }
        });
        // cambiar la luz ambiental si luces apagadas
        if (!v) {
            scene.traverse(function(h) {
                if (h instanceof THREE.AmbientLight) {
                    h.intensity = 0.5;
                }
            });
        }
        else {
            scene.traverse(function(h) {
                if (h instanceof THREE.AmbientLight) {
                    h.intensity = 1;
                }
            });
        }
    });
    videofolder.add(effectController, "volumen", 0, 1, 0.1).name("Volumen").onChange(v => {
        sound.setVolume(v);
    });
    videofolder.add(effectController, "play").name("Reproducir");
    videofolder.add(effectController, "pause").name("Pausar");
}

function onKeyDown(event) {
    switch(event.keyCode) {
        case 37:
            camera.rotation.y += Math.PI / 90;
            break;
        case 38:
            const forward = new THREE.Vector3(0, 0, -2);
            const rotationMatrix = new THREE.Matrix4();
            rotationMatrix.makeRotationFromEuler(camera.rotation);
            forward.applyMatrix4(rotationMatrix);
            const newPosition = camera.position.clone().add(forward);
            if (isInBounds(newPosition)) {
                camera.position.copy(newPosition);
            }
            break;
        case 39:
            camera.rotation.y -= Math.PI / 90;
            break;
        case 40:
            const backward = new THREE.Vector3(0, 0, 2);
            const rotationMatrixBack = new THREE.Matrix4();
            rotationMatrixBack.makeRotationFromEuler(camera.rotation);
            backward.applyMatrix4(rotationMatrixBack);
            const newPositionBack = camera.position.clone().add(backward);
            if (isInBounds(newPositionBack)) {
                camera.position.copy(newPositionBack);
            }
        break;
    }
}

// FUNCIONES AUXILIARES
function colocar_sillas() {
    let material
    for(let n = 0; n < 2; n++) {
        for(let j = 1; j < 6; j++) { // filas
            arraySillas[(j-1) + n * 5] = []
            for(let i = -15; i < 15; i++) { // butacas
                if (i != -1 && i != 0) {
                    silla = new THREE.Mesh(new THREE.BoxGeometry(x_respaldo, y_respaldo + y_pata, z_cojin), matinvisible);
                    // material
                    if ((j == 2 && i == -4) || (j == 5 && i == -13) || 
                        (j == 5 && i == 14) || (j == 2 && i == 4) ||
                        (j == 5 && i == 13) || (j == 5 && i == -14)) {
                        material = matvelvet_o;
                        silla.name = "silla_f" + (j + n * 5) + "_b" + (i + 16) + "_o";
                    }
                    else {
                        material = matvelvet;
                        silla.name = "silla_f" + (j + n * 5) + "_b" + (i + 16) + "_l";
                    }
                    //   - pata 1
                    const pata1 = new THREE.Mesh(new THREE.BoxGeometry(x_pata,y_pata,z_pata), matcuero);
                    pata1.position.set(-x_cojin/2 + x_pata/2, y_pata/2, - z_cojin/2 + z_pata/2);
                    pata1.receiveShadow = pata1.castShadow = true;
                    silla.add(pata1);
                    //   - pata 2
                    const pata2 = new THREE.Mesh(new THREE.BoxGeometry(x_pata,y_pata,z_pata), matcuero);
                    pata2.position.set(x_cojin/2 - x_pata/2, y_pata/2, -z_cojin/2 + z_pata/2);
                    pata2.receiveShadow = pata2.castShadow = true;
                    silla.add(pata2);
                    //  - cojin
                    const cojin = new THREE.Mesh(new THREE.BoxGeometry(x_cojin,y_cojin,z_cojin), material);
                    cojin.position.set(0,y_pata + y_cojin/2,z_pata);
                    cojin.receiveShadow = cojin.castShadow = true;
                    cojin.name = "cojin";
                    silla.add(cojin);

                    // - respaldo
                    const respaldo = new THREE.Mesh(new THREE.BoxGeometry(x_respaldo,y_respaldo,z_respaldo), material);
                    respaldo.position.set(0, y_respaldo/2 + y_pata, -z_cojin/2 + z_respaldo/2);
                    respaldo.receiveShadow = respaldo.castShadow = true;
                    respaldo.name = "respaldo";
                    silla.add(respaldo);

                    scene.add(silla);

                    if (n == 0) { // sillas de abajo
                        silla.position.set((x_cojin + 7) * i,y_tarima,-z_tarima/2 + z_cojin/2 * 5 * j);
                        arraySillas[(j-1) + n * 5].push(silla);
                    }
                    else { // sillas de arriba
                        silla.position.set((x_cojin + 7) * i,y_tarima*2,z_tarima/2 - z_cojin/2 * 5 * (6 - j));
                        arraySillas[(j-1) + n * 5].push(silla);
                    }

                    silla.rotation.y = Math.PI;
                    silla.castShadow = silla.receiveShadow = true;
                }
            }
        }
    }
}

function seleccionarAsiento(fila, butaca) {
    if (fila == "-" || butaca == "-") {
        alert("Selecciona un asiento");
        return;
    }
    else if (arraySillas[fila - 1][butaca - 1].name.includes("_o")) {
        alert("Asiento no disponible");
        return;
    }
    else {
        if(arraySillas[fila - 1][butaca - 1].name.includes("_sel")){
            // eliminar seleccion
            arraySillas[fila - 1][butaca - 1].name = arraySillas[fila - 1][butaca - 1].name.replace("_sel", "");
            // cambiar el material del cojin y respaldo
            arraySillas[fila - 1][butaca - 1]
                .getObjectByName("cojin").material = matvelvet;
            arraySillas[fila - 1][butaca - 1]
                .getObjectByName("respaldo").material = matvelvet;
        }
        else {
            // seleccionar
            arraySillas[fila - 1][butaca - 1].name += "_sel";
            // cambiar el material del cojin y respaldo
            arraySillas[fila - 1][butaca - 1]
                .getObjectByName("cojin").material = matvelvet_sel;
            arraySillas[fila - 1][butaca - 1]
                .getObjectByName("respaldo").material = matvelvet_sel;
        }
    }
}

function irAlAsiento(fila, butaca) {
    // encontrar la posicion deseada
    let posicion;
    if (fila == "-" || butaca == "-") {
        alert("Selecciona un asiento");
        return;
    }
    else {
        posicion = arraySillas[fila - 1][butaca - 1].position;
    
        if (arraySillas[fila - 1][butaca - 1].name.includes("_o")) {
            alert("Asiento no disponible");
            return;
        }
        new TWEEN.Tween(camera.position)
            .to({x: posicion.x, y: posicion.y + 50, z: posicion.z}, 1000)
            .easing(TWEEN.Easing.Quadratic.InOut)
            .start();
        new TWEEN.Tween(camera.rotation)
            .to({x: 0, y: Math.PI / (8 * posicion.z), z: 0}, 1000)
            .easing(TWEEN.Easing.Quadratic.InOut)
            .start();

        effectController.mensaje = "ww";
    }
}

function isInBounds(position) {
    // Comprueba si la nueva posición está dentro de los límites de la escena.
    return (
        position.x >= -x_sala / 2 + 5 &&
        position.x <= x_sala / 2 - 5 &&
        position.z >= -z_sala / 2 + 5 &&
        position.z <= z_sala / 2 - 5 &&
        position.y >= 5 &&
        position.y <= y_sala
    );
}