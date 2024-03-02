// SEMINARIO 2 - Escena cubo

// Cargar libreria
import * as THREE from '../../lib/three.module.js';

// Variables globales
var scene = new THREE.Scene();
scene.background = new THREE.Color(0x220044);

// Creamos la camara
// perspectiva 75 angulos de vision, relacion de aspecto
var camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);

// Creamos el renderer
var renderer = new THREE.WebGLRenderer();
renderer.setSize(window.innerWidth, window.innerHeight);
document.body.appendChild(renderer.domElement);

// Cubo
var geometry = new THREE.BoxGeometry();
var material = new THREE.MeshBasicMaterial( {color: "rgb(180,20,70)", wireframe: true} );
var cubo = new THREE.Mesh(geometry, material);
cubo.position.x = -2;
scene.add(cubo);

// Camara
camera.position.z = 5;
camera.position.y = 1;

// Render
// renderer.render(scene, camera);
// crear funcion animate
var animate = function () {
    requestAnimationFrame(animate);

    cubo.rotation.x += 0.01;
    // cubo.rotation.y += 0.01;
    scene.rotation.y += 0.01
    // camera.rotation.y += 0.01

    renderer.render(scene, camera);
}

animate();