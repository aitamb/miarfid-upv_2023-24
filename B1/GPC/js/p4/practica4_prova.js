// PRACTICA 4 - Interaccion-Animacion
 
// Cargar libreria
import * as THREE from '../../lib/three.module.js';
import { OrbitControls } from "../../lib/OrbitControls.module.js";
import {TWEEN} from "../../lib/tween.module.min.js"
import {GUI} from "../../lib/lil-gui.module.min.js"

// Variables estandar
let renderer, scene, camera;

// Globales, partes del robot
let base, brazo, antebrazo, disco, mano, pinzaIz, pinzaDe;

// Globales, control
let controlador;

// MEDIDAS del ROBOT
// - Base
const RADIO_BASE = 50;
const ALTURA_BASE = 15;
// - Brazo
const RADIO_EJE = 20;
const ALTURA_EJE = 18;
const X_ESPARRAGO = 18;
const Y_ESPARRAGO = 120;
const Z_ESPARRAGO = 12;
const RADIO_ROTULA = 20;
// - Antebrazo
const RADIO_DISCO = 22;
const ALTURA_DISCO = 6;
const X_NERVIO = 4;
const Y_NERVIO = 80;
const Z_NERVIO = 4;
const RADIO_NERVIO = RADIO_ROTULA/2;
const RADIO_MANO = 15;
const ALTURA_MANO = 40;
// - Pinza
const Y_CUBO_PPAL = 20;
const X_CUBO_PPAL = 19;
const X_TOTAL = 38;
const Z_CUBO_PPAL = 4;
const Z_CUBO_SEC = 2;

// Variables para la vista cenital
let cenital, L;

// Acciones 
init();
loadScene();
setupGUI();
render();

// Creacion de las FUNCIONES
function init (){
    // Instanciar el motor de render
    renderer = new THREE.WebGLRenderer();
    renderer.setSize(window.innerWidth, window.innerHeight);
    renderer.setClearColor(0xFFFFFF);
    renderer.autoClear = false; // para que no borre cada vez que se llama al render
    document.getElementById('container').appendChild(renderer.domElement);
    
    // Instanciar el nodo raiz de la escena
    scene = new THREE.Scene();

    // Instanciar la camara
    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    camera.position.set(0,300,200);
    camera.lookAt(0,1,0);

    // Control de camara
    const controls = new OrbitControls(camera, renderer.domElement);

    // Configuracion camaras
    L = Math.min(window.innerWidth, window.innerHeight) / 4;
    setMiniCamera();

    // Captura eventos
    // - redimension de la ventana
    window.addEventListener('resize', updateAspectRatio);

    // - teclas teclado
    document.addEventListener('keydown', onKeyDown);
}

function loadScene(){
    // Material sencillo
    const material = new THREE.MeshNormalMaterial({wireframe: false, flatShading: true});

    // Suelo
    const suelo = new THREE.Mesh(new THREE.PlaneGeometry(1000,1000,10,10), material);
    suelo.rotation.x = -Math.PI / 2;
    suelo.position.y = -0.2;
    scene.add(suelo);

    // Base
    base = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_BASE,RADIO_BASE,ALTURA_BASE,30), material);

    // Brazo
    brazo = new THREE.Object3D();
    // - eje 
    const eje = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_EJE,RADIO_EJE,ALTURA_EJE,50), material);
    eje.rotation.x = -Math.PI / 2;
    brazo.add(eje);
    // - esparrago
    const esparrago = new THREE.Mesh(new THREE.BoxGeometry(X_ESPARRAGO,Y_ESPARRAGO,Z_ESPARRAGO), material);
    esparrago.position.y = Y_ESPARRAGO/2;
    brazo.add(esparrago);
    // - rotula
    const rotula = new THREE.Mesh(new THREE.SphereGeometry(RADIO_ROTULA,30,30), material);
    rotula.position.y = Y_ESPARRAGO;
    brazo.add(rotula);
    // - antebrazo
    antebrazo = new THREE.Object3D();
    antebrazo.position.y = Y_ESPARRAGO;

    // Antebrazo
    // - disco
    disco = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_DISCO,RADIO_DISCO,ALTURA_DISCO,50), material);
    antebrazo.add(disco);
    // - nervios
    for(let i = 0; i <= Math.PI * 2; i += Math.PI/2){
        const nervio = new THREE.Mesh(new THREE.BoxGeometry(X_NERVIO,Y_NERVIO,Z_NERVIO), material);
        nervio.position.y = Y_NERVIO/2;
        nervio.position.x = Math.cos(i + Math.PI/4) * RADIO_NERVIO;
        nervio.position.z = Math.sin(i + Math.PI/4) * RADIO_NERVIO;
        antebrazo.add(nervio);
    }
    // - mano
    mano = new THREE.Object3D();
    mano.position.y = Y_NERVIO;

    // Mano
    // - base
    const base_mano = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_MANO,RADIO_MANO,ALTURA_MANO,50), material);
    base_mano.rotation.x = -Math.PI / 2;
    mano.add(base_mano);
    // - pinzas
    // Obtener datos de las pinzas
    var x_pinza = RADIO_MANO * 2/10; 
    var y_pinza = - ALTURA_MANO/4; 
    var z_pinza = Z_CUBO_PPAL; 
    // Instanciar geometria propia
    const geometry = new THREE.BufferGeometry();
    // Construir los arrays
    // posicion de los vertices
    const position = getPinzaPosition(x_pinza, y_pinza, z_pinza);
    // Construir los VBOs en la geometria
    geometry.setAttribute('position', new THREE.BufferAttribute(position, 3));
    // Indices de tringulos
    const indices = [0,2,1, 0,5,2, 3,2,5, 3,5,4, // cara exterior
                    6,7,8, 6,8,11, 11,8,9, 11,9,10, //cara interior
                    1,8,7, 1,2,8, // tapa cubo ppal
                    0,6,11, 0,11,5, // base cubo ppal
                    7,0,1, 7,6,0, // lomo cubo ppal
                    2,3,9, 2,9,8, // tapa cubo sec
                    5,11,4, 11,10,4, // base cubo sec
                    3,4,9, 4,10,9 // frente cubo sec
                    ];
    geometry.setIndex(indices);
    // Construir pinza derecha
    pinzaDe = new THREE.Mesh(geometry, material);
    mano.add(pinzaDe);
    // Construir pinza izquierda
    pinzaIz = new THREE.Mesh(geometry, material);
    pinzaIz.rotation.x = Math.PI; // Rotar en espejo
    pinzaIz.position.y = y_pinza * 2 + Y_CUBO_PPAL;
    mano.add(pinzaIz);
    // Anyadir las partes a la escena
    const robot = new THREE.Object3D();
    antebrazo.add(mano);
    brazo.add(antebrazo);
    base.add(brazo);
    robot.add(base);

    scene.add(robot);

    // Ejes
    scene.add(new THREE.AxesHelper(3));
}

function update(){
    TWEEN.update();
}

function render(){
    renderer.clear();
    requestAnimationFrame(render);
    update();
    let side;
    // Dibujar vista cenital
    const ar = window.innerWidth / window.innerHeight;
    if(ar < 1) // width < height
        side = window.innerWidth/4;
    else
        side = window.innerHeight/4;

    renderer.setViewport(0,0, window.innerWidth,window.innerHeight);
    renderer.render(scene, camera);

    renderer.setViewport(0, (window.innerHeight - side), side,side);
    renderer.render(scene, cenital);
}

// FUNCIONES AUXILIARES
function getPinzaPosition(x,y,z) {
    const position = new Float32Array([
        x, y, z, // v0
        x, y + Y_CUBO_PPAL, z, // v1
        x + X_CUBO_PPAL, y + Y_CUBO_PPAL, z, // v2
        x + X_TOTAL, y + (3/4) * Y_CUBO_PPAL, z - (Z_CUBO_PPAL - Z_CUBO_SEC), // v3
        x + X_TOTAL, y + (1/4) * Y_CUBO_PPAL, z - (Z_CUBO_PPAL - Z_CUBO_SEC), // v4
        x + X_CUBO_PPAL, y, z, // v5

        x, y, z - Z_CUBO_PPAL, // v6
        x, y + Y_CUBO_PPAL, z - Z_CUBO_PPAL, // v7
        x + X_CUBO_PPAL, y + Y_CUBO_PPAL, z - Z_CUBO_PPAL, // v8 
        x + X_TOTAL, y + (3/4) * Y_CUBO_PPAL, z - Z_CUBO_PPAL, // v9
        x + X_TOTAL, y + (1/4) * Y_CUBO_PPAL, z - Z_CUBO_PPAL, // v10
        x + X_CUBO_PPAL, y, z - Z_CUBO_PPAL // v11
    ]);

    return position
}

function setMiniCamera() {
    // Creacion de la camara
    // las medidas de la vista se relacionan con las medidas de la ventana
    const camaraOrto = new THREE.OrthographicCamera(-L/4, L/4, L/4, -L/4, -10, 300)

    cenital = camaraOrto.clone();
    cenital.position.set(0,250,0);
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

    // Nuevo L para la mini camara
    L = Math.min(window.innerWidth, window.innerHeight) / 4;

    // Ortografica
    // Se cambia la L en funcion de la L
    cenital.left = -L/4;
    cenital.right = L/4;
    cenital.bottom = -L/4;
    cenital.top = L/4;

    // Actualizar matriz de proyeccion
    cenital.updateProjectionMatrix();
}

function onKeyDown(event) {
    switch(event.keyCode) {
        case 37: // left: mover en -z
            base.position.z -= 1;
            break;
        case 38: // up: mover en +x
            base.position.x += 1;
            break;
        case 39: // right: mover en +z
            base.position.z += 1;
            break;
        case 40: // down: mover en -x
            base.position.x -= 1;
            break;
    }
}

function animate(){
    // Animaciones
    // - base
    new TWEEN.Tween(base.rotation).
        to({x:[0,0], y:[-Math.PI,0], z:[0,0]}, 8000).
        interpolation(TWEEN.Interpolation.Bezier).start();
    // - brazo
    new TWEEN.Tween(brazo.rotation).
        to({x:[0,0], y:[0,0], z:[-Math.PI/4,0]}, 8000).
        interpolation(TWEEN.Interpolation.Linear).start();
    // - antebrazo
    new TWEEN.Tween(antebrazo.rotation).
        to({x:[0,0], y:[Math.PI/2,0],x:[0,0]}, 8000).
        interpolation(TWEEN.Interpolation.Bezier).start();
    new TWEEN.Tween(antebrazo.rotation).
        to({x:[0,0], y:[0,0], z:[Math.PI/2,0]}, 8000).
        interpolation(TWEEN.Interpolation.Bezier).start();
    //  - mano
    new TWEEN.Tween(mano.rotation).
        to({x:[0,0], y:[0,0], z:[-Math.PI/2,0]}, 8000).
        interpolation(TWEEN.Interpolation.Bezier).start();
    // - separacion pinza
    new TWEEN.Tween(pinzaDe.position).
        to({x:[0,0], y:[0,0], z:[10,0,10]}, 8000).
        interpolation(TWEEN.Interpolation.Linear).start();
    new TWEEN.Tween(pinzaIz.position).
        to({x:[0,0], y:[0,0], z:[-10,0,-10]}, 8000).
        interpolation(TWEEN.Interpolation.Linear).start();
}

function setupGUI() {
    // Creacion interfaz
    const gui = new GUI();
    
    // Construccion del menu con los callbacks
    // - base
    const controlRobot = gui.addFolder("Control robot");
    controlRobot.add({giro_base: 0.0}, 'giro_base', -180.0, 180.0, 0.025).name("Giro base").
        onChange(function(giro_base){
            base.rotation.y = giro_base * Math.PI / 180;
        });
    // - brazo
    controlRobot.add({giro_brazo: 0.0}, 'giro_brazo', -45.0, 45.0, 0.025).name("Giro brazo").
        onChange(function(giro_brazo){
            brazo.rotation.z = giro_brazo * Math.PI / 180;
        });
    // - antebrazo
    controlRobot.add({giro_antebrazo_y: 0.0}, 'giro_antebrazo_y', -180.0, 180.0, 0.025).name("Giro antebrazo Y").
        onChange(function(giro_antebrazo_y){
            antebrazo.rotation.y = giro_antebrazo_y * Math.PI / 180;
        });
    controlRobot.add({giro_antebrazo_z: 0.0}, 'giro_antebrazo_z', -90.0, 90.0, 0.025).name("Giro antebrazo Z").
        onChange(function(giro_antebrazo_z){
            antebrazo.rotation.z = giro_antebrazo_z * Math.PI / 180;
        });
    // - mano
    controlRobot.add({giro_pinza: 0.0}, 'giro_pinza', -40.0, 220.0, 0.025).name("Giro pinza").
        onChange(function(giro_pinza){
            mano.rotation.z = giro_pinza * Math.PI / 180;
        });
    // - pinza
    controlRobot.add({apertura_pinza: 10.0}, 'apertura_pinza', 0.0, 15.0, 0.025).name("Apertura pinza").
        onChange(function(apertura_pinza){
            pinzaDe.position.z = apertura_pinza;
            pinzaIz.position.z = -apertura_pinza;
        });
    // - alambres
    controlRobot.add({alambres: false}, 'alambres').name("Alambres").
        onChange(function(alambres){
            base.material.wireframe = alambres;
        });
    // - animacion
    controlRobot.add({animacion: false}, 'animacion').name("Animacion").
        onChange(function(animacion){
            if(animacion)
                animate();
        });
}