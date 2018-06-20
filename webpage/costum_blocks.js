Blockly.Blocks["start"] = {
	init: function() {
		this.jsonInit({
			"message0": "start %1",
			"args0": [
				{
					"type": "input_statement",
					"name": "program"
				}
			],
			"colour": 0,
			"tooltip": "Starts a bio program"
		});
	}
};

Blockly.Blocks["inputDeclaration"] = {
	init: function() {
		this.jsonInit({
			"message0": "new input",
			"args0": [
			],
			"message1": "fluid name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "inputName",
					"variable": "input_fluid_name",
					"check": "FLUID",
					"variableTypes": ["FLUID"]
				}
			],
			"message2": "amount %1 drops",
			"args2": [
				{
					"type": "field_number",
					"name": "inputAmount",
					"check": "Number"
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 180,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["dropletDeclaration"] = {
	init: function() {
		this.jsonInit({
			"message0": "new Droplet",
			"args0": [
			],
			"message1": "fluid name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "dropletName",
					"variable": "input_fluid_name",
					"check": "FLUID",
					"variableTypes": ["FLUID"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 180,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["outputDeclaration"] = {
	init: function() {
		this.jsonInit({
			"message0": "new output",
			"args0": [
			],			
			"message1": "module name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name",
					"check": "OUTPUT",
					"variableTypes": ["OUTPUT"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 180,
			"tooltip": "",
		});
	}
};

Blockly.Blocks["wasteDeclaration"] = {
	init: function() {
		this.jsonInit({
			"message0": "new waste",
			"args0": [
			],			
			"message1": "module name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name",
					"check": "WASTE",
					"variableTypes": ["WASTE"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 180,
			"tooltip": "",
		});
	}
};

Blockly.Blocks["sensorDeclaration"] = {
	init: function() {
		this.jsonInit({
			"message0": "new sensor",
			"args0": [
			],			
			"message1": "module name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name",
					"check": "SENSOR",
					"variableTypes": ["SENSOR"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 180,
			"tooltip": "",
		});
	}
};

Blockly.Blocks["heaterDeclaration"] = {
	init: function() {
		this.jsonInit({
			"message0": "new heater",
			"args0": [
			],			
			"message1": "module name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name",
					"check": "HEATER",
					"variableTypes": ["HEATER"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 180,
			"tooltip": "",
		});
	}
};

Blockly.Blocks["outputUsage"] = {
	init: function() {
		this.jsonInit({
			"message0": "output",
			"args0": [
			],			
			"message1": "target %1 %2",
			"args1": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name",
					"check": "OUTPUT",
					"variableTypes": ["OUTPUT"]
				},
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 150,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["wasteUsage"] = {
	init: function() {
		this.jsonInit({
			"message0": "waste",
			"args0": [
			],			
			"message1": "target %1 %2",
			"args1": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name",
					"check": "WASTE",
					"variableTypes": ["WASTE"]
				},
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 150,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["sensorUsage"] = {
	init: function() 
	{
		this.jsonInit({
			"message0": "sensor",
			"args0": [
			],	
			"message1": "target %1 %2",
			"args1": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name",
					"check": "SENSOR",
					"variableTypes": ["SENSOR"]
				},
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": "FluidType"
				}
			],
			"output": "Number",
			"colour": 240,
			"tooltip": ""
		});
		const sensorBlock = this;
		setTimeout(function()
		{
			const blockSvg = sensorBlock.getSvgRoot().getElementsByClassName("blocklyPath")[0];
			blockSvg.setAttribute("fill", "url('#sensorGradient')");
		}, 50);
	}
};

Blockly.Blocks["heaterUsage"] = {
	init: function() {
		this.jsonInit({
			"message0": "heater %1",
			"args0": [
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],						
			"message1": "target heater %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name",
					"check": "HEATER",
					"variableTypes": ["HEATER"]
				}
			],
			"message2": "temperature %1",
			"args2": [
				{
					"type": "field_number",
					"name": "temperature",
					"check": "Number"
				}
			],
			"message3": "time %1",
			"args3": [
				{
					"type": "field_number",
					"name": "time",
					"check": "Number"
				}
			],
			"output": "FluidOperator",
			"colour": 150,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["fluid"] = {
	init: function() {
		this.jsonInit({
			"message0": "new fluid",
			"args0": [
			],
			"message1": "name %1 %2",
			"args1": [
				{
					"type": "field_variable",
					"name": "fluidName",
					"variable": "fluid_name",
					"check": "FLUID",
					"variableTypes": ["FLUID"]
				},
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType", "FluidOperator"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 150,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["getFluid"] = {
	init: function() {
		this.jsonInit({
			"message0": "fluid %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "fluidName",
					"variable": "fluid_name",
					"check": "FLUID",
					"variableTypes": ["FLUID"]
				}
			],
			"message1": "amount %1 drops",
			"args1": [
				{
					"type": "field_number",
					"name": "fluidAmount",
					"check": "Number"
				}
			],
			"message2": "use all fluid %1",
			"args2": [
				{
					"type": "field_checkbox",
					"name": "useAllFluid",
					"checked": false
				}
			],
			"output": "FluidType",
			"colour": 150,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["getDropletCount"] = {
	init: function() {
		this.jsonInit({
			"message0": "droplet count of %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "fluidName",
					"variable": "fluid_name",
					"check": "FLUID",
					"variableTypes": ["FLUID"]
				}
			],
			"output": "Number",
			"colour": 240,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["mixer"] = {
	init: function() {
		this.jsonInit({
			"message0": "mix",
			"args0": [
			],
			"message1": "a %1",
			"args1": [
				{
					"type": "input_value",
					"name": "inputFluidA",
					"check": ["InputType", "FluidType"]
				}
			],
			"message2": "b %1",
			"args2": [
				{
					"type": "input_value",
					"name": "inputFluidB",
					"check": ["InputType", "FluidType"]
				}
			],
			"output": "FluidOperator",
			"colour": 150,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["fluidArray"] = {
	init: function() {
		this.jsonInit({
			"message0": "new fluid array",
			"args0": [
			],
			"message1": "name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "fluid_array_name",
					"check": ["FLUID_ARRAY"],
					"variableTypes": ["FLUID_ARRAY"]
				}
			],
			"message2": "length %1",
			"args2": [
				{
					"type": "input_value",
					"name": "arrayLength",
					"check": "Number"
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 120,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["setFluidArrayIndex"] = {
	init: function() {
		this.jsonInit({
			"message0": "in array %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "fluid_array_name",
					"check": "FLUID_ARRAY",
					"variableTypes": ["FLUID_ARRAY"]
				}
			],
			"message1": "set index %1",
			"args1": [
				{
					"type": "input_value",
					"name": "index",
					"check": ["Number"]
				}
			],
			"message2": "to %1",
			"args2": [
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["FluidType"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 120,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["getFluidArrayIndex"] = {
	init: function() {
		this.jsonInit({
			"message0": "in array %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "fluid_array_name",
					"check": "FLUID_ARRAY",
					"variableTypes": ["FLUID_ARRAY"]
				}
			],
			"message1": "get index %1",
			"args1": [
				{
					"type": "input_value",
					"name": "index",
					"check": ["Number"]
				}
			],
			"message2": "amount %1 ml",
			"args2": [
				{
					"type": "field_number",
					"name": "fluidAmount",
					"check": "Number"
				}
			],
			"message3": "use all fluid %1",
			"args3": [
				{
					"type": "field_checkbox",
					"name": "useAllFluid",
					"checked": false
				}
			],
			"output": "FluidType",
			"colour": 120,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["getArrayLength"] = {
	init: function() {
		this.jsonInit({
			"message0": "array length of %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "array_name",
				"check": ["FLUID_ARRAY", "NUMBER_ARRAY"],
					"variableTypes": ["FLUID_ARRAY", "NUMBER_ARRAY"]
				}
			],
			"output": "Number",
			"colour": 240,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["getNumberVariable"] = {
	init: function() {
		this.jsonInit({
			"message0": "value of %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "variableName",
					"variable": "variable_name",
					"check": "NUMBER",
					"variableTypes": ["NUMBER"]
				}
			],
			"output": "Number",
			"colour": 240,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["setNumberVariable"] = {
	init: function() {
		this.jsonInit({
			"message0": "set %1 to %2",
			"args0": [
				{
					"type": "field_variable",
					"name": "variableName",
					"variable": "variable_name",
					"check": ["NUMBER"],
					"variableTypes": ["NUMBER"]
				},
				{
					"type": "input_value",
					"name": "inputVariable",
					"check": ["Number"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 240,
			"tooltip": ""
		});
	}
};

//{
//	name,
//	inputs = [],
//	outputs = [],
//	variables = [],
//	programXml
//}
var inlineProgramPrograms = [{name: "program name", inputs: [], outputs: [], variables: []}];

Blockly.Blocks["inlineProgram"] = 
{
	init: function() 
	{
		this.jsonInit(
		{
			"inputsInline": false,
			"previousStatement": null,
			"nextStatement": null,
			"colour": 40,
			"tooltip": ""
		});
		
		var items = [];
		for(var i = 0; i < inlineProgramPrograms.length; i++)
		{
			const item = inlineProgramPrograms[i];
			items.push([item.name, item.name]);
		}
		
		this.appendDummyInput().appendField("run program").appendField(new Blockly.FieldDropdown(items), "programsDropdown");
		this.getField("programsDropdown").setValidator(function(option) 
		{
			this.sourceBlock_.updateShape_(option);
		});
		
		this.setMutator(new Blockly.Mutator([]));
	},
	mutationToDom: function() 
	{
		if(this.program)
		{
			const container = document.createElement('mutation');
			container.setAttribute("program_name"  , this.program.name);
			container.setAttribute("input_count"   , this.program.inputs.length);
			container.setAttribute("output_count"  , this.program.outputs.length);
			container.setAttribute("variable_count", this.program.variables.length);
			return container;
		}
		
		return null;
	},
	domToMutation: function(xmlElement) 
	{
		const programName = xmlElement.getAttribute("program_name");
		this.updateShape_(programName);
	},
	updateShape_: function(programName)
	{
		//remove previous defined inputs and outputs
		var inputCounter = 0;
		while(this.getInput("input-" + inputCounter))
		{
			this.removeInput("input-" + inputCounter);
			inputCounter++;
		}
		
		var outputCounter = 0;
		while(this.getInput("outputer-" + outputCounter))
		{
			this.removeInput("outputer-" + outputCounter);
			outputCounter++;
		}
		
		var variableCounter = 0;
		while(this.getInput("variable-" + variableCounter))
		{
			this.removeInput("variable-" + variableCounter);
			variableCounter++;
		}
	
		//find the new program
		this.program = null;
		for(var i = 0; i < window.inlineProgramPrograms.length; i++)
		{
			const program = window.inlineProgramPrograms[i];
			if(program.name == programName)
			{
				this.program = program;
				break;
			}
		}
		
		if(this.program != null)
		{
			//add programs inputs, outputs, variables
			for(var i = 0; i < this.program.inputs.length; i++)
			{
				const inputName = this.program.inputs[i];
				this.appendValueInput("input-" + i).setCheck("FluidType").appendField("input " + inputName);
			}
			
			for(var i = 0; i < this.program.variables.length; i++)
			{
				const variableName = this.program.variables[i];
				this.appendValueInput("variable-" + i).setCheck("Number").appendField("variable " + variableName);
			}
			
			for(var i = 0; i < this.program.outputs.length; i++)
			{
				const outputName = this.program.outputs[i];
				this.appendDummyInput("outputer-" + i).appendField("output " + outputName).appendField(new Blockly.FieldVariable("output fluid name", null, ["FLUID"]), "output-" + i);
			}
		}
	},
	compose: function(topBlock)
	{
		
	},
	decompose: function(localWorkspace) 
	{		
		if(this.program && this.program.programXml)
		{
			const xml = Blockly.Xml.textToDom(this.program.programXml);
			Blockly.Xml.domToWorkspace(xml, localWorkspace);
			
			return localWorkspace.getTopBlocks[0];
		}
	}
};

Blockly.Blocks["union"] = {
	init: function() {
		this.jsonInit({
			"message0": "merge fluids",
			"args0": [
			],
			"message1": "a %1",
			"args1": [
				{
					"type": "input_value",
					"name": "inputFluidA",
					"check": ["FluidType"]
				}
			],
			"message2": "b %1",
			"args2": [
				{
					"type": "input_value",
					"name": "inputFluidB",
					"check": ["FluidType"]
				}
			],
			"output": "FluidOperator",
			"colour": 150,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["importNumberVariable"] = {
	init: function() {
		this.jsonInit({
			"message0": "import variable",
			"args0": [
			],
			"message1": "import as %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "variableName",
					"variable": "variable_name",
					"check": "NUMBER",
					"variableTypes": ["NUMBER"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 240,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["numberArray"] = {
	init: function() {
		this.jsonInit({
			"message0": "new number array",
			"args0": [
			],
			"message1": "name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "number_array_name",
					"check": "NUMBER_ARRAY",
					"variableTypes": ["NUMBER_ARRAY"]
				}
			],
			"message2": "length %1",
			"args2": [
				{
					"type": "input_value",
					"name": "arrayLength",
					"check": "Number"
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 280,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["setNumberArrayIndex"] = {
	init: function() {
		this.jsonInit({
			"message0": "in number array %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "number_array_name",
					"check": "NUMBER_ARRAY",
					"variableTypes": ["NUMBER_ARRAY"]
				}
			],
			"message1": "set index %1",
			"args1": [
				{
					"type": "input_value",
					"name": "index",
					"check": ["Number"]
				}
			],
			"message2": "to %1",
			"args2": [
				{
					"type": "input_value",
					"name": "inputNumber",
					"check": "Number"
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 280,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["getNumberArrayIndex"] = {
	init: function() {
		this.jsonInit({
			"message0": "in number array %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "number_array_name",
					"check": "NUMBER_ARRAY",
					"variableTypes": ["NUMBER_ARRAY"]
				}
			],
			"message1": "get index %1",
			"args1": [
				{
					"type": "input_value",
					"name": "index",
					"check": ["Number"]
				}
			],
			"output": "Number",
			"colour": 280,
			"tooltip": ""
		});
	}
};









