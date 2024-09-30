// PRACTICA 2 - Brazo robot

// Cargar libreria
import * as THREE from '../../lib/three.module.js';

// Variables estandar
let renderer, scene, camera;

// Variables globales
const Y_CUBO_PPAL = 20;
const X_CUBO_PPAL = 19;
const X_TOTAL = 38;
const Z_CUBO_PPAL = 4;
const Z_CUBO_SEC = 2;
let angulo = 0;

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

// Acciones 
init();
loadScene();
render();

// Creacion de las funciones
function init (){
    // Instanciar el motor de render
    renderer = new THREE.WebGLRenderer();
    renderer.setSize(window.innerWidth, window.innerHeight);
    document.getElementById('container').appendChild(renderer.domElement);

    // Instancial el nodo raiz de la escena
    scene = new THREE.Scene();
    scene.background = new THREE.Color(1, 1, 1);

    // Instanciar la camara
    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    // camera.position.set(240,260,250);
    // camera.position.set(30,260,0);
    camera.position.set(0,300,200);
    camera.lookAt(0,1,0);
}

function loadScene(){
    // Material sencillo
    const material = new THREE.MeshBasicMaterial({color: 'red', wireframe: true});

    // Suelo
    const suelo = new THREE.Mesh(new THREE.PlaneGeometry(1000,1000,10,10), material);
    suelo.rotation.x = -Math.PI / 2;
    suelo.position.y = -0.2;
    scene.add(suelo);

    // Base
    const base = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_BASE,RADIO_BASE,ALTURA_BASE,30), material);

    // Brazo
    const brazo = new THREE.Object3D();
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
    const antebrazo = new THREE.Object3D();

    // Antebrazo
    // - disco
    const disco = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_DISCO,RADIO_DISCO,ALTURA_DISCO,50), material);
    disco.position.y = Y_ESPARRAGO;
    antebrazo.add(disco);
    // - nervios
    for(let i = 0; i <= Math.PI * 2; i += Math.PI/2){
        const nervio = new THREE.Mesh(new THREE.BoxGeometry(X_NERVIO,Y_NERVIO,Z_NERVIO), material);
        nervio.position.y = Y_NERVIO/2 + Y_ESPARRAGO;
        nervio.position.x = Math.cos(i + Math.PI/4) * RADIO_NERVIO;
        nervio.position.z = Math.sin(i + Math.PI/4) * RADIO_NERVIO;
        antebrazo.add(nervio);
    }
    // - mano
    const mano = new THREE.Object3D();

    // Mano
    // - base
    const base_mano = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_MANO,RADIO_MANO,ALTURA_MANO,50), material);
    base_mano.rotation.x = -Math.PI / 2;
    base_mano.position.y = Y_ESPARRAGO + Y_NERVIO;
    mano.add(base_mano);
    // - pinzas
    // Obtener datos de las pinzas
    var x_pinza = RADIO_MANO * 8/10; 
    var y_pinza = Y_ESPARRAGO + Y_NERVIO - ALTURA_MANO/4; 
    var z_pinza = 4/10 * ALTURA_MANO; 
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
    const pinzaDe = new THREE.Mesh(geometry, material);
    mano.add(pinzaDe);
    // Construir pinza izquierda
    const pinzaIz = new THREE.Mesh(geometry, material);
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
    angulo += 0.01;
    scene.rotation.y = angulo;
}

function render(){
    requestAnimationFrame(render);
    update();
    renderer.render(scene, camera);
}

// Funciones auxiliares
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