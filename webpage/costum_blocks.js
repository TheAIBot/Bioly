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
					"variable": "input_fluid_name"
				}
			],
			"message2": "amount %1 %2",
			"args2": [
				{
					"type": "field_number",
					"name": "inputAmount",
					"check": "Number"
				},
				{
					"type": "field_dropdown",
					"name": "inputUnit",
					"options": [
						["drops", "0"],
						["ml", "1"]
					]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 40,
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
					"variable": "module_name"
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 280,
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
					"variable": "module_name"
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 280,
			"tooltip": "",
		});
	}
};

Blockly.Blocks["outputUseage"] = {
	init: function() {
		this.jsonInit({						
			"message0": "target %1 %2",
			"args0": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name"
				},
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 280,
			"tooltip": "",
			"inputsInline": false
		});
	}
};

Blockly.Blocks["heaterUseage"] = {
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
					"variable": "module_name"
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
			"colour": 80,
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
					"variable": "fluid_name"
				},
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType", "FluidOperator"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 80,
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
					"variable": "fluid_name"
				}
			],
			"message1": "amount %1 ml",
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
			"colour": 80,
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
			"colour": 120,
			"tooltip": ""
		});
	}
};

Blockly.Blocks["waste"] = {
	init: function() {
		this.jsonInit({
			"message0": "waste %1",
			"args0": [
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 240,
			"tooltip": "",
			"inputsInline": false
		});
	}
};



Blockly.Blocks["sensor"] = {
	init: function() {
		this.jsonInit({
			"message0": "sensor %1",
			"args0": [
				{
					"type": "input_value",
					"name": "inputFluid",
					"check": ["InputType", "FluidType"]
				}
			],
			"output": "Number",
			"colour": 320,
			"tooltip": "",
			"inputsInline": false
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
					"variable": "fluid_array_name"
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
			"colour": 40,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["setFLuidArrayIndex"] = {
	init: function() {
		this.jsonInit({
			"message0": "in array %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "fluid_array_name"
				}
			],
			"message1": "set index %1 to %2",
			"args1": [
				{
					"type": "field_number",
					"name": "arrayName"
				},
				{
					"type": "input_value",
					"name": "value",
					"check": ["InputType", "FluidType", "FluidOperator"]
				}
			],
			"previousStatement": null,
			"nextStatement": null,
			"colour": 40,
			"tooltip": ""
		});
	}
};
Blockly.Blocks["getFLuidArrayIndex"] = {
	init: function() {
		this.jsonInit({
			"message0": "in array %1",
			"args0": [
				{
					"type": "field_variable",
					"name": "arrayName",
					"variable": "fluid_array_name"
				}
			],
			"message1": "get index %1",
			"args1": [
				{
					"type": "field_number",
					"name": "arrayName"
				}
			],
			"output": "FluidType",
			"previousStatement": null,
			"nextStatement": null,
			"colour": 40,
			"tooltip": ""
		});
	}
};

//{
//	name,
//	inputs = [],
//	outputs = [],
//	programXml
//}
var inlineProgramPrograms = [{name: "program name", inputs: [], outputs: []}];

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
		
		this.appendDummyInput().appendField("program").appendField(new Blockly.FieldDropdown(items), "programsDropdown");
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
			container.setAttribute("programName", this.getFieldValue("programsDropdown"));
			return container;
		}
		
		return null;
	},
	domToMutation: function(xmlElement) 
	{
		const programName = xmlElement.getAttribute("programName");
		this.updateShape_(programName);
	},
	updateShape_: function(programName)
	{
		for(var i = 0; i < 100; i++)
		{
			this.removeInput("input-" + i, true);
			this.removeInput("output-" + i, true);
		}
	
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
			for(var i = 0; i < this.program.inputs.length; i++)
			{
				const inputName = this.program.inputs[i];
				this.appendValueInput("input-" + i).setCheck(["InputType", "FluidType"]).appendField("input " + inputName);
			}
			
			for(var i = 0; i < this.program.outputs.length; i++)
			{
				const outputName = this.program.outputs[i];
				this.appendDummyInput("output-" + i).appendField("output " + outputName).appendField(new Blockly.FieldVariable("output fluid name"));
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
			localWorkspace.options.readOnly = true;
			
			return localWorkspace.getTopBlocks[0];
		}
	}
};









