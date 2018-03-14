'use strict';

const 

var gl;
var boardVerticies;

window.onload = function init()
{
    const canvas = document.getElementById("simulatorCanvas");
    
    gl = WebGLUtils.setupWebGL(canvas);
    if (!gl) 
    {
        alert("failed to load webgl");
    }
    
    gl.viewport(0, 0, canvas.width, canvas.height);
    gl.clearColor(0.3921, 0.5843, 0.9294, 1.0);
    
    //NOW DRAW SOME SHIT
    const program = initShaders(gl, "vertex-shader", "fragment-shader");
    gl.useProgram(program);    
    
}

function setupSimulator(width, height)
{
    var verticies = [
        vec2(0.0, 0.0),
        vec2(1.0, 0.0),
        vec2(1.0, 1.0),
    ];
    
    var vertexBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(verticies), gl.STATIC_DRAW);
	
    var vPosition = gl.getAttribLocation(program, "vPosition");
    gl.vertexAttribPointer(vPosition, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vPosition);
	
	
	var colorBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, colorBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, flatten(verticies), gl.STATIC_DRAW);
	
    var vPosition = gl.getAttribLocation(program, "vPosition");
    gl.vertexAttribPointer(vPosition, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(vPosition);
    
    render(verticies.length);
}

function setupBoard(width, height)
{
	boardVerticies = [];
	
	for(var y = 0; y < height; y++)
	{
		for(var x = 0; x < width; x++)
		{
			
		}
	}
}

function render(numPoints) 
{
    gl.clear(gl.COLOR_BUFFER_BIT);
    gl.drawArrays(gl.TRIANGLES, 0, numPoints);
}


function updateLoop()
{
	//if new command then parse and execute command
	
	//detect errors
	
	//update drop positions
	
	//marge or split drops
}