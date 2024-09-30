/*
Practica WebGL: PolineaAPP.js
Dibuja una polilinea con los puntos que el usuario clickea usando
Buffer Objects. El color del vértice depende de la distancia al centro,
se calcula en la app y se pasa como atributo al shader de vértices.
rvivo@upv.es, 2023
*/

// SHADER DE VERTICES
const VSHADER_SOURCE = 
	`
	attribute vec3 posicion;				  
	attribute vec4 color;		// color del vertice			  	  
    varying highp vec4 vColor;  // color del fragmento				  
	void main(){                             
		gl_Position = vec4(posicion, 1.0);	  
		gl_PointSize = 10.0;				  
		vColor = color;				          
	}
	`
									  	 
// SHADER DE FRAGMENTOS
const FSHADER_SOURCE =
	`
    varying highp vec4 vColor;		// color del fragmento		     
	void main(){                                
		gl_FragColor = vColor;					 
	}
	`

let bufferVertices = null;
let bufferColores = null;

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
	// Localiza el atributo color en el shader
	const colores = gl.getAttribLocation( gl.program, 'color');
	// Crea el buffer, lo activa y enlaza con coordenadas
	bufferColores = gl.createBuffer();
	gl.bindBuffer( gl.ARRAY_BUFFER, bufferColores );
	gl.vertexAttribPointer( colores, 4, gl.FLOAT, false, 0, 0 );
	gl.enableVertexAttribArray( colores );
	// Registro de la call-back de raton
	canvas.onclick = function( evento ){ click( evento, gl, canvas ); };
	// Dibujar
	render( gl );
}

const clicks = [];
const coloresClicks = [];
function click( evento, gl, canvas )
{
	let x = evento.clientX; 	// coordenada x del cursor respecto del documento
	let y = evento.clientY;		// coordenada y del cursor respecto del documento
	const rect = evento.target.getBoundingClientRect(); // rectangulo del canvas
	// Conversión de coordenadas al cuadrado de 2x2
	x = ((x - rect.left) - canvas.width/2) * 2/canvas.width;
	y = (canvas.height/2 - (y - rect.top)) * 2/canvas.height;
	// Guardar las coordenadas y copia el array
	clicks.push(x); clicks.push(y); clicks.push(0.0);
	// Procesar color por distancia al centro (circulo de radio raiz de 2)
	var d = 1.0 - Math.sqrt( (x*x + y*y) / 2.0 );
	for( var i = 0; i < 3; i++ ) coloresClicks.push(d);
	// Alfa. Color de 4 componentes
	coloresClicks.push(1.0);
	// Redibujar
	render( gl );
}

function render( gl )
{
	const puntos = new Float32Array(clicks); // array de puntos
	const colores = new Float32Array(coloresClicks); // array de colores
	// Borra el canvas con el color de fondo
	gl.clear( gl.COLOR_BUFFER_BIT );
	// Rellena los BOs con las coordenadas y colores y lo manda a proceso
	gl.bindBuffer( gl.ARRAY_BUFFER, bufferVertices );
	gl.bufferData( gl.ARRAY_BUFFER, puntos, gl.STATIC_DRAW );
	gl.bindBuffer( gl.ARRAY_BUFFER, bufferColores );
	gl.bufferData( gl.ARRAY_BUFFER, colores, gl.STATIC_DRAW );
	gl.drawArrays( gl.POINTS, 0, puntos.length/3 )	
	gl.drawArrays( gl.LINE_STRIP, 0, puntos.length/3 )	
}