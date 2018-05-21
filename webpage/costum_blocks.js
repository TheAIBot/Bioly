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
			"message1": "module name %1",
			"args1": [
				{
					"type": "field_variable",
					"name": "moduleName",
					"variable": "module_name"
				}
			],
			"message2": "fluid name %1",
			"args2": [
				{
					"type": "field_variable",
					"name": "inputName",
					"variable": "input_fluid_name"
				}
			],
			"message3": "amount %1 %2",
			"args3": [
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

var inlineProgramPrograms = []
//{
//	name,
//	inputs = [],
//	outputs = []
//
//
Blockly.Blocks["inlineProgram"] = {
	init: function() {
		init: function() 
		{
			
			
			this.appendField("program")
				.appendField(new Blockly.FieldDropdown([
				['first item', 'ITEM1'],
				['second item', 'ITEM2']
				]),
				'FIELDNAME');
		}
	}
};

const inlineProgramMutatorMixFunctions = 
{
	mutationToDom: function() 
	{
		var container = document.createElement('mutation');
		var divisorInput = (this.getFieldValue('PROPERTY') == 'DIVISIBLE_BY');
		container.setAttribute('divisor_input', divisorInput);
		return container;
	},
	domToMutation: function(xmlElement) 
	{
		var divisorInput = (xmlElement.getAttribute('divisor_input') == 'true');
		this.updateShape_(divisorInput);
	},
	updateShape_: function(inputs, outputs)
	{
		// Add or remove a Value Input.
		var inputExists = this.getInput('DIVISOR');
		if (divisorInput) 
		{
			if (!inputExists) 
			{
				this.appendValueInput('DIVISOR').setCheck('Number');
			}
		} 
		else if (inputExists) 
		{
			this.removeInput('DIVISOR');
		}
	}
};
inlineProgramMutatorExtensionFunction = function() 
{
	this.getField("programName").setValidator(function() 
	{
		this.sourceBlock_.updateShape_();
	});
};

Blockly.Extensions.registerMutator("inlineProgramMutator", inlineProgramMutatorMixFunctions, inlineProgramMutatorExtensionFunction);









