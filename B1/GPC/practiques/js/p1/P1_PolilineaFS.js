/*
Practica WebGL: PolilineaFS.js
Dibuja una polilinea con los puntos que el usuario clickea usando
Buffer Objects. El color del vértice depende de la distancia al centro y
se calcula en el shader de fragmentos
rvivo@upv.es 2023
*/

// SHADER DE VERTICES
const VSHADER_SOURCE = `
	attribute vec3 posicion;				  
	void main(){                             
		gl_Position = vec4(posicion, 1.0);	  
		gl_PointSize = 10.0;				  
	} `;
									  	 
// SHADER DE FRAGMENTOS
const FSHADER_SOURCE = `
	// El viewport se pasa como uniform
    uniform highp vec2 resolucion;

	void main(){
		// Centro y coordenadas respecto al centro
		highp vec2 centro = resolucion.xy / 2.0;
		highp vec2 coordcentro = gl_FragCoord.xy - centro.xy;
		// Distancias al cuadrado maxima y actual
		highp float maxdistancia =  dot(centro,centro);
		highp float distancia =  dot(coordcentro,coordcentro);
		// Tono de amarillo segun distancia
		distancia = max(0.0, 1.0 - sqrt(distancia / maxdistancia) );                          
		gl_FragColor = vec4(distancia, distancia, 0.0, 1.0);
	} `;

let bufferVertices = null;

function main()
{
	// Recupera el área de dibujo
	const canvas = document.getElementById("canvas");
	if( !canvas ) {
		console.log("Fallo al recuperar el canvas");
		return;
	}
	// Recupera el lienzo del área de dibujo
	// como un contexto WebGL
	const gl = getWebGLContext( canvas );
	if( !gl ){
		console.log("Fallo al recuperar el contexto WebGL");
		return;
	}
	// Carga, compila y monta los shaders
	if( !initShaders(gl, VSHADER_SOURCE, FSHADER_SOURCE) ){
		console.log("Fallo al inicializar los shaders");
	}
	// Fija el color de fondo -azul oscuro-
	gl.clearColor( 0.0, 0.0, 0.3, 1.0 );
	// Localiza el atributo posicion en el shader
	const coordenadas = gl.getAttribLocation( gl.program, 'posicion');
	// Crea el buffer, lo activa y enlaza con coordenadas
	bufferVertices = gl.createBuffer();
	gl.bindBuffer( gl.ARRAY_BUFFER, bufferVertices );
	gl.vertexAttribPointer( coordenadas, 3, gl.FLOAT, false, 0, 0 );
	gl.enableVertexAttribArray( coordenadas );
	// Pasamos la resolucion al shader
	const resolucion = gl.getUniformLocation(gl.program, 'resolucion');
	gl.uniform2f(resolucion, canvas.width, canvas.height);
	// Registro de la call-back de raton
	canvas.onclick = function( evento ){ click( evento, gl, canvas ); };
	// Dibujar
	render( gl );
}

var clicks = [];

function click( evento, gl, canvas )
{
	let x = evento.clientX; 	// coordenada x del cursor respecto del documento
	let y = evento.clientY;		// coordenada y del cursor respecto del documento
	const rect = evento.target.getBoundingClientRect(); // rectangulo del canvas
	// Conversión de coordenadas al cuadrado de 2x2 (s.r. por defecto en webgl)
	x = ((x - rect.left) - canvas.width/2) * 2/canvas.width;
	y = (canvas.height/2 - (y - rect.top)) * 2/canvas.height;
	// Guardar las coordenadas (s.r de webgl)
	clicks.push(x); clicks.push(y); clicks.push(0.0);
	// Redibujar
	render( gl );
}

function render( gl )
{
	const puntos = new Float32Array(clicks); // array de puntos
	// Borra el canvas con el color de fondo
	gl.clear( gl.COLOR_BUFFER_BIT );
	// Rellena los BOs con las coordenadas y lo manda a proceso
	gl.bindBuffer( gl.ARRAY_BUFFER, bufferVertices );
	gl.bufferData( gl.ARRAY_BUFFER, puntos, gl.STATIC_DRAW );
	gl.drawArrays( gl.POINTS, 0, puntos.length/3 )	
	gl.drawArrays( gl.LINE_STRIP, 0, puntos.length/3 )	
}