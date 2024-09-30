// SEMINARIO 2 - Prueba de escena

// Cargar libreria
import * as THREE from '../../lib/three.module.js';
import {GLTFLoader} from "../../lib/GLTFLoader.module.js"

// Variables estandar
let renderer, scene, camera;

// Otras globales
let esferaCubo;
let angulo = 0;

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
    scene.background = new THREE.Color(0.5, 0.5, 0.5);

    // Instanciar la camara
    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    camera.position.set(0.5,2,7);
    camera.lookAt(0,1,0);
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
    update();
    renderer.render(scene, camera);
}