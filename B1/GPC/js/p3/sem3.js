// SEMINARIO 3 - Camara

// Cargar libreria
import * as THREE from "../../lib/three.module.js";
import {GLTFLoader} from "../../lib/GLTFLoader.module.js";
import { OrbitControls } from "../../lib/OrbitControls.module.js";

// Variables estandar
let renderer, scene, camera;

// Otras globales
let esferaCubo;
let angulo = 0;

let alzado, planta, perfil;
const L = 5;

// Acciones 
init();
loadScene();
render();

// Creacion de las funciones
function init (){
    // Instanciar el motor de render
    renderer = new THREE.WebGLRenderer();
    renderer.setSize(window.innerWidth, window.innerHeight);
    renderer.setClearColor(0xB1BED0);
    renderer.autoClear = false; // para que no borre cada vez que se llama al render
    document.getElementById('container').appendChild(renderer.domElement);

    // Instancial el nodo raiz de la escena
    scene = new THREE.Scene();
    // scene.background = new THREE.Color(0.5, 0.5, 0.5);

    // Instanciar la camara
    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    camera.position.set(0.5,2,7);
    camera.lookAt(0,1,0);

    // Control de camara
    const controls = new OrbitControls(camera, renderer.domElement);

    // Configuracion de las camaras
    const ar = window.innerWidth / window.innerHeight;
    setCameras(ar);

    // Captura de eventos para redimension de la ventana
    window.addEventListener('resize', updateAspectRatio);

    // Captura de eventos del raton
    renderer.domElement.addEventListener('dblclick', rotateShape);
}

function loadScene(){
    // Material sencillo
    const material = new THREE.MeshBasicMaterial({color: 'yellow', wireframe: true});

    // Suelo
    const suelo = new THREE.Mesh(new THREE.PlaneGeometry(10,10,10,10), material);
    suelo.rotation.x = -Math.PI / 2;
    suelo.position.y = -0.2;
    scene.add(suelo);

    // Esfera y cubo
    const esfera = new THREE.Mesh(new THREE.SphereGeometry(1,20,20), material);
    esfera.position.x = -1;
    const cubo = new THREE.Mesh(new THREE.BoxGeometry(2,2,2), material);
    cubo.position.x = 1;

    // Objeto 3D formado por las dos figuras
    esferaCubo = new THREE.Object3D();
    esferaCubo.add(esfera);
    esferaCubo.add(cubo);
    esferaCubo.position.y = 1.5;

    esferaCubo.name = "grupoEC";

    scene.add(esferaCubo);

    // Ejes
    scene.add(new THREE.AxesHelper(3));
    cubo.add(new THREE.AxesHelper(1));

    // Modelos importados
    const loader = new THREE.ObjectLoader();
    loader.load('../../models/soldado/soldado.json', 
    function(objeto){ // anyade el objeto cargado al cubo y cambia posicion
        cubo.add(objeto);
        objeto.position.y = 1;
    });

    const glloader = new GLTFLoader();
    glloader.load('../../models/robota/scene.gltf',
    function(objeto){
        esfera.add(objeto.scene); // porque es otro formato
        objeto.scene.scale.set(0.5,0.5,0.5);
        objeto.scene.position.y = 1;
        objeto.scene.rotation.y = -Math.PI/2;
        console.log("ROBOT");
        console.log(objeto);
    });
}

function update(){
    angulo += 0.01;
    scene.rotation.y = angulo;
}

function render(){
    requestAnimationFrame(render);
    // update();
    renderer.clear();
    let w = window.innerWidth/2;
    let h = window.innerHeight/2;
    // El S.R. del viewport es left-bottom con X right y Y up
    renderer.setViewport(0,h, w,h);
    renderer.render(scene, alzado);
    renderer.setViewport(0,0, w,h);
    renderer.render(scene, planta);
    renderer.setViewport(w,h, w,h);
    renderer.render(scene, perfil);
    renderer.setViewport(w,0, w,h);
    renderer.render(scene, camera);
}

function setCameras(ar){
    // creacion de las camaras
    // ar -> aspect ratio (rel. de aspecto)
    let camaraOrto;
    if(ar>1) 
        camaraOrto = new THREE.OrthographicCamera(-L*ar, L*ar, L, -L, -10, 100);
    else 
        camaraOrto = new THREE.OrthographicCamera(-L, L, L/ar, -L/ar, -10, 100);

    alzado = camaraOrto.clone();
    alzado.position.set(0,0,10); 
    alzado.lookAt(0,0,0);

    perfil = camaraOrto
    perfil.position.set(10,0,0);
    perfil.lookAt(0,0,0);

    planta = camaraOrto.clone();
    planta.position.set(0,10,0);
    planta.lookAt(0,0,0);
    planta.up = new THREE.Vector3(0,0,-1); // Cambia el vector UP de la camara porque mira hacia abajo
}

function updateAspectRatio() {
    // Cada vez que se cambie el tamayo de la ventana, se llama a esta funcion
    // Cambiar las dimensiones del canvas
    renderer.setSize(window.innerWidth, window.innerHeight);

    // Nuevo ar de la camara
    const ar = window.innerWidth / window.innerHeight;

    // perspectiva
    camera.aspect = ar;
    camera.updateProjectionMatrix();

    // ortografica
    // en funcion del ar, se cambia la camara
    if(ar > 1){
        alzado.left = planta.left = perfil.left = -L*ar;
        alzado.right = planta.right = perfil.right = L*ar;
        alzado.bottom = planta.bottom = perfil.bottom = -L;
        alzado.top = planta.top = perfil.top = L;
    }
    else {
        alzado.left = planta.left = perfil.left = -L;
        alzado.right = planta.right = perfil.right = L;
        alzado.bottom = planta.bottom = perfil.bottom = -L/ar;
        alzado.top = planta.top = perfil.top = L/ar;

    }

    // actualizar matrices de proyeccion
    alzado.updateProjectionMatrix();
    planta.updateProjectionMatrix();
    perfil.updateProjectionMatrix();
}

function rotateShape(evento){
    // Capturar poisicon de doble clicj (S.R. top-left) con Y down
    let x = evento.clientX;
    let y = evento.clientY;

    // Zona click
    let derecha = false, abajo = false;
    let cam = null;
    if (x > window.innerWidth/2) {
        derecha = true;
        x -= window.innerWidth/2;
    }
    if (y > window.innerHeight/2) {
        abajo = true;
        y -= window.innerHeight/2;
    }

    // cam es la camara que recibe el doble click
    if (derecha)
        if(abajo) cam = camera;
        else cam = perfil;
    else 
        if(abajo) cam = planta;
        else cam = alzado;
        
    // Normalizar las coordenadas de click al cuadrado de 2x2
    // x*2 / w/2 -> x*4 / w
    x = (x*4 / window.innerWidth) - 1;
    y = -(y*4 / window.innerHeight) + 1;

    // Rayo e intersecciones
    const rayo = new THREE.Raycaster();
    rayo.setFromCamera(new THREE.Vector2(x,y), cam);

    const intersecciones = rayo.intersectObjects(
        scene.getObjectByName("grupoEC").children, false); // false para que no sea recursivo
    if(intersecciones.length > 0)
        intersecciones[0].object.rotation.y += Math.PI / 8;
}
