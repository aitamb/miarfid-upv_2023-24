// PRACTICA 5 - Luces y materiales
// Aitana Menárguez Box
 
// Cargar libreria
import * as THREE from '../../lib/three.module.js';
import { OrbitControls } from "../../lib/OrbitControls.module.js";
import {TWEEN} from "../../lib/tween.module.min.js"
import {GUI} from "../../lib/lil-gui.module.min.js"
import {GLTFLoader} from "../../lib/GLTFLoader.module.js";

// Variables estandar
let renderer, scene, camera;

// Globales, partes del robot
let base, brazo, antebrazo, disco, mano, pinzaIz, pinzaDe, rotula;

// Globales, control
let controlador, controlRobot;

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
    // para anyadir el mapa de sombras...
    renderer.antialias = true;
    renderer.shadowMap.enabled = true;
    
    // Instanciar el nodo raiz de la escena
    scene = new THREE.Scene();

    // Instanciar la camara
    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 2000);
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

    // Fuentes de luz
    // ambiental
    const ambiental = new THREE.AmbientLight(0x2e2e2e);
    scene.add(ambiental);
    // direccional
    const direccional = new THREE.DirectionalLight(0xFFFFFF, 0.3);
    direccional.position.set(-100, 200, -100);
    direccional.castShadow = true;
    direccional.shadow.camera.left = -200;
    direccional.shadow.camera.right = 200;
    direccional.shadow.camera.top = 200;
    direccional.shadow.camera.bottom = -200;
    direccional.shadow.camera.near = 0.5;
    direccional.shadow.camera.far = 500;
    scene.add(direccional);
    // focal
    const focal = new THREE.SpotLight(0x0077fe, 0.3);
    focal.position.set(30, 300, 30);
    focal.target.position.set(0, 0, 0);
    focal.angle = Math.PI/5;
    focal.penumbra = 0.3;
    focal.castShadow = true;
    focal.shadow.camera.near = 0.5;
    focal.shadow.camera.far = 500;
    scene.add(focal);
    // focal2
    const focal2 = new THREE.SpotLight(0x0fff00, 0.3);
    focal2.position.set(-30, 300, -30);
    focal2.target.position.set(0, 0, 0);
    focal2.angle = Math.PI/6;
    focal2.penumbra = 0.3;
    focal2.castShadow = true;
    focal2.shadow.camera.near = 0.5;
    focal2.shadow.camera.far = 500;
    scene.add(focal2);
}

function loadScene(){
    // Materiales y texturas
    const path = "../../images_p5/";
    // - mesa
    const texmesa = new THREE.TextureLoader().load(path + "table.jpg");
    let matmesa = new THREE.MeshStandardMaterial({color: 'white', map: texmesa});
    // - goma
    const texgoma = new THREE.TextureLoader().load(path + "goma.jpg");
    const matgoma = new THREE.MeshLambertMaterial({map: texgoma});
    // - hierro
    const texhierro = new THREE.TextureLoader().load(path + "hierro.jpg");
    const mathierro = new THREE.MeshPhongMaterial({map: texhierro});
    // - entorno
    const entorno = [ path + "px.jpg", path + "nx.jpg",
                    path+ "py.jpg", path + "ny.jpg",
                    path + "pz.jpg", path + "nz.jpg"];
    // - metal
    const texmetal = new THREE.CubeTextureLoader().load(entorno);
    const matmetal = new THREE.MeshPhongMaterial({ color: 'white', 
                                                    specular: 'gray',
                                                    shininess: 30,
                                                    envMap: texmetal});
    
    // GEOMETRIA
    // Suelo
    const suelo = new THREE.Mesh(new THREE.PlaneGeometry(500,500,10,10), matmesa);
    suelo.rotation.x = -Math.PI / 2;
    suelo.position.y = -0.2;
    suelo.receiveShadow = true;
    scene.add(suelo);

    // Base
    base = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_BASE,RADIO_BASE,ALTURA_BASE,30), matgoma);
    base.receiveShadow = base.castShadow = true;
    // Brazo
    brazo = new THREE.Object3D();
    // - eje 
    const eje = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_EJE,RADIO_EJE,ALTURA_EJE,50), matmetal);
    eje.rotation.x = -Math.PI / 2;
    eje.receiveShadow = eje.castShadow = true;
    brazo.add(eje);
    // - esparrago
    const esparrago = new THREE.Mesh(new THREE.BoxGeometry(X_ESPARRAGO,Y_ESPARRAGO,Z_ESPARRAGO), mathierro);
    esparrago.position.y = Y_ESPARRAGO/2;
    esparrago.receiveShadow = esparrago.castShadow = true;
    brazo.add(esparrago);
    // - rotula
    rotula = new THREE.Mesh(new THREE.SphereGeometry(RADIO_ROTULA,30,30), matmetal);
    rotula.position.y = Y_ESPARRAGO;
    rotula.receiveShadow = rotula.castShadow = true;
    brazo.add(rotula);
    // - antebrazo
    antebrazo = new THREE.Object3D();
    antebrazo.position.y = Y_ESPARRAGO;

    // Antebrazo
    // - disco
    disco = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_DISCO,RADIO_DISCO,ALTURA_DISCO,50), matgoma);
    disco.receiveShadow = disco.castShadow = true;
    antebrazo.add(disco);
    // - nervios
    for(let i = 0; i <= Math.PI * 2; i += Math.PI/2){
        const nervio = new THREE.Mesh(new THREE.BoxGeometry(X_NERVIO,Y_NERVIO,Z_NERVIO), mathierro);
        nervio.position.y = Y_NERVIO/2;
        nervio.position.x = Math.cos(i + Math.PI/4) * RADIO_NERVIO;
        nervio.position.z = Math.sin(i + Math.PI/4) * RADIO_NERVIO;
        nervio.receiveShadow = nervio.castShadow = true;
        antebrazo.add(nervio);
    }
    // - mano
    mano = new THREE.Object3D();
    mano.position.y = Y_NERVIO;

    // Mano
    // - base
    const base_mano = new THREE.Mesh(new THREE.CylinderGeometry(RADIO_MANO,RADIO_MANO,ALTURA_MANO,50), matgoma);
    base_mano.rotation.x = -Math.PI / 2;
    base_mano.receiveShadow = base_mano.castShadow = true;
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
    // coordenadas de textura
    const uv = new Float32Array([0,0, 0,1, 1,1, 1,0, 0,0, 0,1, 1,1, 1,0, // cara exterior
                                0,0, 0,1, 1,1, 1,0, 0,0, 0,1, 1,1, 1,0, // cara interior
                                0,0, 0,1, 1,1, 1,0, // tapa cubo ppal
                                0,0, 0,1, 1,1, 1,0, // base cubo ppal
                                0,0, 0,1, 1,1, 1,0, // lomo cubo ppal
                                0,0, 0,1, 1,1, 1,0, // tapa cubo sec
                                0,0, 0,1, 1,1, 1,0, // base cubo sec
                                0,0, 0,1, 1,1, 1,0 // frente cubo sec
                                ]);
    // Construir los VBOs en la geometria
    geometry.setAttribute('position', new THREE.BufferAttribute(position, 3));
    geometry.setAttribute('uv', new THREE.BufferAttribute(uv, 2));
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
    geometry.computeVertexNormals();
    // Construir pinza derecha
    pinzaDe = new THREE.Mesh(geometry, mathierro);
    pinzaDe.receiveShadow = pinzaDe.castShadow = true;
    mano.add(pinzaDe);
    // Construir pinza izquierda
    pinzaIz = new THREE.Mesh(geometry, mathierro);
    pinzaIz.rotation.x = Math.PI; // Rotar en espejo
    pinzaIz.position.y = y_pinza * 2 + Y_CUBO_PPAL;
    pinzaIz.receiveShadow = pinzaIz.castShadow = true;
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

    // Modelos importados
    const glloader = new GLTFLoader();
    glloader.load('../../models/coffee_cup/scene.gltf',
    function(objeto)
    {
        scene.add(objeto.scene);
        objeto.scene.scale.set(2.2,2.2,2.2);
        objeto.scene.position.set(0,0,120);
        objeto.scene.rotation.y = -Math.PI/2;
        objeto.scene.name = 'cafe';
        objeto.scene.traverse(ob => { // para todos los objetos 3d del subgrafo
            if(ob.isObject3D) ob.castShadow = true;
        });
    });

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

    const habitacion = new THREE.Mesh(new THREE.BoxGeometry(1200,1200,1200), paredes)
    scene.add(habitacion);
}

function update(){
    // Lectura de controles en GUI
    // - base
    base.rotation.y = controlador.giro_base * Math.PI / 180;
    // - brazo
    brazo.rotation.z = controlador.giro_brazo * Math.PI / 180;
    // - antebrazo
    antebrazo.rotation.y = controlador.giro_abr_y * Math.PI / 180;
    antebrazo.rotation.z = controlador.giro_abr_z * Math.PI / 180;
    // - pinza
    mano.rotation.z = controlador.giro_pinza * Math.PI / 180;
    pinzaDe.position.z = controlador.sep_pinza;
    pinzaIz.position.z = -controlador.sep_pinza;
    // - alambres
    base.material.wireframe = controlador.alambres;
    disco.material.wireframe = controlador.alambres;
    rotula.material.wireframe = controlador.alambres;
    pinzaDe.material.wireframe = controlador.alambres;
    pinzaIz.material.wireframe = controlador.alambres;

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
    new TWEEN.Tween({y:base.rotation.y}).
        to({x:[0,0], y:[-Math.PI,0], z:[0,0]}, 8000).
        interpolation(TWEEN.Interpolation.Bezier).onUpdate(function(coords){
            controlRobot.controllers[0].setValue(coords.y * 180 / Math.PI);
        }).start();
    // - brazo
    new TWEEN.Tween({z:brazo.rotation.z}).
        to({x:[0,0], y:[0,0], z:[-Math.PI/4,0]}, 8000).
        interpolation(TWEEN.Interpolation.Linear).onUpdate(function(coords){
            controlRobot.controllers[1].setValue(coords.z * 180 / Math.PI);
        }).start();
    // - antebrazo
    new TWEEN.Tween({y:antebrazo.rotation.y}).
        to({x:[0,0], y:[Math.PI/4,0],x:[0,0]}, 8000).
        interpolation(TWEEN.Interpolation.Bezier).onUpdate(function(coords){
            controlRobot.controllers[2].setValue(coords.y * 180 / Math.PI);
        }).start();
    new TWEEN.Tween({z:antebrazo.rotation.z}).
        to({x:[0,0], y:[0,0], z:[Math.PI/4,0]}, 8000).
        interpolation(TWEEN.Interpolation.Bezier).onUpdate(function(coords){
            controlRobot.controllers[3].setValue(coords.z * 180 / Math.PI);
        }).start();
    //  - mano
    new TWEEN.Tween({z:mano.rotation.z}).
        to({x:[0,0], y:[0,0], z:[-Math.PI/2,0]}, 8000).
        interpolation(TWEEN.Interpolation.Bezier).onUpdate(function(coords){
            controlRobot.controllers[4].setValue(coords.z * 180 / Math.PI);
        }).start();
    // - separacion pinza
    new TWEEN.Tween({z:pinzaDe.position.z}).
        to({x:[0,0], y:[0,0], z:[10,0,10]}, 8000).
        interpolation(TWEEN.Interpolation.Linear).onUpdate(function(coords){
            controlRobot.controllers[5].setValue(coords.z);
        }).start();
    new TWEEN.Tween({z:pinzaIz.position.z}).
        to({x:[0,0], y:[0,0], z:[10,0,10]}, 8000).
        interpolation(TWEEN.Interpolation.Linear).onUpdate(function(coords){
            controlRobot.controllers[5].setValue(coords.z);
        }).start();
}

function setupGUI() {
    // Creacion interfaz
    const gui = new GUI();

    // Definicion de controles
    controlador = {
        giro_base: 0.0,
        giro_brazo: 0.0,
        giro_abr_y: 0.0,
        giro_abr_z: 0.0,
        giro_pinza: 0.0,
        sep_pinza: 10.0,
        alambres: false,
        // button animacion
        animacion: animate
    };

    // Construccion del menu
    controlRobot = gui.addFolder("Control robot");
    controlRobot.add(controlador, "giro_base", -180.0, 180.0, 0.025).name("Giro base");
    controlRobot.add(controlador, "giro_brazo", -45.0, 45.0, 0.025).name("Giro brazo");
    controlRobot.add(controlador, "giro_abr_y", -180.0, 180.0, 0.025).name("Giro antebrazo Y");
    controlRobot.add(controlador, "giro_abr_z", -90.0, 90.0, 0.025).name("Giro antebrazo Z");
    controlRobot.add(controlador, "giro_pinza", -40.0, 220.0, 0.025).name("Giro pinza");
    controlRobot.add(controlador, "sep_pinza", 0.0, 15.0, 0.025).name("Separacion pinza");
    controlRobot.add(controlador, "alambres").name("Alambres");
    controlRobot.add(controlador, "animacion").name("Animar");
}