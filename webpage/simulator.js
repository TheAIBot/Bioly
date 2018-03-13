window.onload = function init()
{
    canvas = document.getElementById("canvas");
    
    gl = WebGLUtils.setupWebGL(canvas);
    if (!gl) 
    {
        alert("failed to load webgl");
    }
    
    gl.viewport(0, 0, canvas.width, canvas.height);
    gl.clearColor(0.3921, 0.5843, 0.9294, 1.0);
    
    //NOW DRAW SOME SHIT
    program = initShaders(gl, "vertex-shader", "fragment-shader");
    gl.useProgram(program);    
    
}

function setupSimulator()
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

function render(numPoints) 
{
    gl.clear(gl.COLOR_BUFFER_BIT);
    gl.drawArrays(gl.TRIANGLES, 0, numPoints);
}
