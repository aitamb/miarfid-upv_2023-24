// - Crear contexto webGL basico
// obtener elemento canvas creado en webglapp.html
var canvas = document.getElementById("canvas");
// crear contecto webgl
var gl = canvas.getContext("webgl2");

// - Comprobar si webgl soportado en navegador
if (!gl) {
  throw new Error("WebGL no soportado");
}

// - Borrar el canvas
gl.clearColor(0.2, 0.0, 0.6, 1.0);
gl.clear(gl.COLOR_BUFFER_BIT);

// - Crear shaders
// se deben cambiar los shaders para anyadir color

// const vertexShader = `#version 300  es
// precision mediump float; 
// in vec2 position; ---------------------------> parametro de entrada
// void main() { SIEMPRE ESTARA gl_Position (y gl_PointSize puede)
//     gl_Position = vec4(position, 0.0, 1.0); -> posicion del vertice
//     gl_PointSize = 10.0; --------------------> tamanyo del punto
// }`;

// const fragmentShader = `#version 300  es
// precision mediump float;
// out vec4 fragColor; -------------------------> parametro de salida 
// void main() {
//     fragColor = vec4(1.0, 1.0, 1.0, 1.0); ---> color del fragmento
// }`;

const vertexShader = `#version 300  es
precision mediump float;
in vec2 position;
in vec3 color;
out vec3 vColor; 
void main() {
    gl_Position = vec4(position, 0.0, 1.0);
    vColor = color;
}`;

const fragmentShader = `#version 300  es
precision mediump float;
out vec4 fragColor;
in vec3 vColor;
void main() {
    fragColor = vec4(vColor, 1.0);
}`;

// - Compilar shaders
const vs = gl.createShader(gl.VERTEX_SHADER); // crear vertex shader
gl.shaderSource(vs, vertexShader); // indicar codigo fuente (que esta en la cadena que hemos creado)
gl.compileShader(vs); // compilar shader
// comprobar si se ha compilado correctamente
if(!gl.getShaderParameter(vs, gl.COMPILE_STATUS)) {
  throw new Error(gl.getShaderInfoLog(vs));
}

const fs = gl.createShader(gl.FRAGMENT_SHADER); // crear fragment shader
gl.shaderSource(fs, fragmentShader); // indicar codigo fuente (que esta en la cadena que hemos creado)
gl.compileShader(fs); // compilar shader
// comprobar si se ha compilado correctamente
if(!gl.getShaderParameter(fs, gl.COMPILE_STATUS)) {
    throw new Error(gl.getShaderInfoLog(fs));
}

// - Crear programa
const program = gl.createProgram();
gl.attachShader(program, vs); // asignar vertex shader
gl.attachShader(program, fs); // asignar fragment shader
gl.linkProgram(program); // enlazar programa
// comprobar si se ha enlazado correctamente
if(!gl.getProgramParameter(program, gl.LINK_STATUS)) {
    throw new Error(gl.getProgramInfoLog(program));
    }
// indicar que vamos a usar el programa creado
gl.useProgram(program);

// CREAR TRIANGULO
// - Crear vertices del triangulo
// const vertices = [ // vertices del triangulo
//     0.0, 0.5,
//     -0.5, -0.5,
//     0.5, -0.5
// ];

// CREAR RECTANGULO
// - Crear vertices del rectangulo
const vertices = [ // vertices del rectangulo
    -0.5, -0.5,
    -0.5, 0.5,
    0.5, -0.5,
    0.5, 0.5
];

// - Crear buffer (para pasar los vertices al shader)
const buffer = gl.createBuffer();
gl.bindBuffer(gl.ARRAY_BUFFER, buffer); // indicar el buffer
gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW); // indicar los datos
const positionAttribLocation = gl.getAttribLocation(program, 'position'); // obtener posicion del atributo position
gl.vertexAttribPointer( // gracias a la localizacion, le pasamos los datos al atributo position
    positionAttribLocation, // localizacion obtenida
    2, // cantidad de elementos
    gl.FLOAT, // tipo de los elementos
    false, 0, 0 
);
gl.enableVertexAttribArray(positionAttribLocation); // habilitar el atributo position

// - Crear colores del rectangulo
const colors = [ // colores del rectangulo (cada vertice uno)
    1.0, 0.0, 0.0, // rojo
    0.0, 1.0, 0.0, // verde
    0.0, 0.0, 1.0, // azul
    1.0, 1.0, 0.0 // amarillo
];

// - Crear buffer (para pasar los colores al shader)
const colorBuffer = gl.createBuffer();
gl.bindBuffer(gl.ARRAY_BUFFER, colorBuffer); // indicar el buffer
gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(colors), gl.STATIC_DRAW); // indicar los datos
const color = gl.getAttribLocation(program, 'color'); // obtener posicion del atributo color
gl.vertexAttribPointer( // gracias a la localizacion, le pasamos los datos al atributo color
    color, // localizacion obtenida
    3, // cantidad de elementos
    gl.FLOAT, // tipo de los elementos
    false, 0, 0
);
gl.enableVertexAttribArray(color); // habilitar el atributo color

// - Dibujar triangulo
// gl.drawArrays(gl.TRIANGLES, 0, 3); // para el rectangulo, no vale con poner un 4

// - Dibujar rectangulo
gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4); // hace falta poner TRIANGE_STRIP