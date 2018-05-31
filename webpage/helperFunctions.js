function setGraph(nodes, edges)
{
	window.graphDiv = cytoscape(
	{
		container: document.getElementById('graphDiv'),

		boxSelectionEnabled: false,
		autounselectify: true,

		layout: 
		{
			name: 'dagre'
		},

		style: 
		[
			{
				selector: 'node',
				style: 
				{
					'min-zoomed-font-size': 5,
					'content': 'data(label)',
					'text-opacity': 0.5,
					'text-valign': 'center',
					'text-halign': 'right',
					'background-color': '#11479e',
					'text-wrap': 'wrap'
				}
			},
			{
				selector: ':parent',
				style: 
				{
					'background-opacity': 0.333	
				}
			},
			{
				selector: 'edge',
				style: 
				{
					'min-zoomed-font-size': 5,
					'content': 'data(label)',
					'curve-style': 'bezier',
					'width': 4,
					'target-arrow-shape': 'triangle',
					'line-color': '#ffffff',
					'target-arrow-color': '#ffffff'
				}
			},
			{
				selector: 'edge.haystack',
				style: 
				{
					'curve-style': 'haystack',
					'display': 'none'
				}
			}
		],
		wheelSensitivity: 0.2,

		elements: 
		{
		nodes: nodes,
		edges: edges
		},
	});
}

function loadWorkspace(xmlText) {
	const xml = Blockly.Xml.textToDom(xmlText);
	Blockly.Xml.domToWorkspace(xml, workspace);
}

function getWorkspaceAsXml()
{
	const xml = Blockly.Xml.workspaceToDom(workspace);
	return Blockly.Xml.domToText(xml);
}

function openTab(e, tabName) 
{
    const tabs = document.getElementsByClassName("tabItemContent");
    for (var i = 0; i < tabs.length; i++) 
	{
        tabs[i].style.display = "none";
    }

    const tablinks = document.getElementsByClassName("tabLink");
    for (var i = 0; i < tablinks.length; i++) 
	{
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }

    // Show the current tab, and add an "active" class to the button that opened the tab
    document.getElementById(tabName).style.display = "block";
    e.currentTarget.className += " active";
}

function ShowBlocklyErrors(errorInfo)
{
	//{
	//	id,
	//	message
	//}
	const allBlocks = workspace.getAllBlocks();
	for(var i = 0; i < errorInfo.length; i++)
	{
		for(var k = 0; k < allBlocks.length; k++)
		{
			if(errorInfo[i].id == allBlocks[k].id)
			{
				allBlocks.splice(k, 1);
				break;
			}
		}
	}
	for(var i = 0; i < allBlocks.length; i++)
	{
		allBlocks[i].setWarningText(null);
	}
	
	workspace.highlightBlock(null);
	for(var i = 0; i < errorInfo.length; i++)
	{
		const block = workspace.getBlockById(errorInfo[i].id);
		if(block)
		{
			block.setWarningText(errorInfo[i].message);
			workspace.highlightBlock(errorInfo[i].id, true);	
		}
	}
}

function ClearErrors()
{
	const allBlocks = workspace.getAllBlocks();
	for(var i = 0; i < allBlocks.length; i++)
	{
		allBlocks[i].setWarningText(null);
	}
	workspace.highlightBlock(null);
}

if(typeof(CefSharp) == "undefined") 
{
	startBlockly([{name: "crashes", inputs: ["fish","cake"], outputs: ["output"], programXml: "<xml xmlns='http://www.w3.org/1999/xhtml'><variables><variable type='' id='NeF,?^-?QI62dY,7zxDa'>module_name</variable><variable type='' id='_8:*Pdd^_65;0KevbL,b'>fish</variable><variable type='' id='D@0NgR83{nJO%uGlbp)}'>cake</variable><variable type='' id='D;GD)KAJARzUI{MQrsE9'>fluid_name</variable><variable type='' id='I;yYnVpyGU.!2qJ|I_i#'>output</variable></variables><block type='start' id='hQZ$BLEQ#L_n(TO32!N9' x='157' y='102'><statement name='program'><block type='inputDeclaration' id='n9c@H/gTj?#N6/[w,AcO'><field name='moduleName' id='NeF,?^-?QI62dY,7zxDa' variabletype=''>module_name</field><field name='inputName' id='_8:*Pdd^_65;0KevbL,b' variabletype=''>fish</field><field name='inputAmount'>10</field><field name='inputUnit'>0</field><next><block type='inputDeclaration' id='NeaGj7UP,sW1%xORfA?c'><field name='moduleName' id='NeF,?^-?QI62dY,7zxDa' variabletype=''>module_name</field><field name='inputName' id='D@0NgR83{nJO%uGlbp)}' variabletype=''>cake</field><field name='inputAmount'>10</field><field name='inputUnit'>0</field><next><block type='outputDeclaration' id='4WnQP)EkdP/[^(S`udvt'><field name='moduleName' id='I;yYnVpyGU.!2qJ|I_i#' variabletype=''>output</field><next><block type='fluid' id='n5nHI(M]m9#^cxR`zE~N'><field name='fluidName' id='D;GD)KAJARzUI{MQrsE9' variabletype=''>fluid_name</field><value name='inputFluid'><block type='mixer' id='7|@/Pf_uRWh0E5`f5vE.'><value name='inputFluidA'><block type='getFluid' id='Xg!l+G`[-CP~C~_JQTRC'><field name='fluidName' id='D@0NgR83{nJO%uGlbp)}' variabletype=''>cake</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value><value name='inputFluidB'><block type='getFluid' id='YJF67j_^U_ECKC)x}jml'><field name='fluidName' id='_8:*Pdd^_65;0KevbL,b' variabletype=''>fish</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value></block></value><next><block type='outputUseage' id=':.THDu}^]k9iW==NB{!O'><field name='moduleName' id='I;yYnVpyGU.!2qJ|I_i#' variabletype=''>output</field><value name='inputFluid'><block type='getFluid' id='2[UJWp5#^yS!s}$L(X)_'><field name='fluidName' id='D;GD)KAJARzUI{MQrsE9' variabletype=''>fluid_name</field><field name='fluidAmount'>2</field><field name='useAllFluid'>FALSE</field></block></value></block></next></block></next></block></next></block></next></block></statement></block></xml>"},{name: "ReassignFluid", inputs: ["input1","input2"], outputs: ["output"], programXml: "<xml xmlns='http://www.w3.org/1999/xhtml'><variables><variable type='' id='d18):Ym8d801JUmRP_Ez'>Fisk</variable><variable type='' id='#N6^?rxXqdzB84l3~TlB'>input1</variable><variable type='' id=':dc*?i]h}tb_#YN_*fY2'>input2</variable><variable type='' id='d!)Y+j*lK7X167Y6w5pj'>mix1</variable><variable type='' id='H?$uV_#2{w_1bQCD#fQ5'>fluidVariableToReassign</variable><variable type='' id='gzTQ:0+1u44Bl6j)af}*'>mix2</variable><variable type='' id='XtZ/5I:ExlN+U5it{D7M'>output</variable><variable type='' id='t+S_%SkicX]kbQ7F3:{y'>Kage</variable><variable type='' id='?lX2qKOOk/}ajrgUS}o3'>fluid_name</variable><variable type='' id='90[+/MHp^kM{NUJI:+6v'>input_fluid_name</variable><variable type='' id='UQ~L0F#Oqehsl^,XD,${'>module_name</variable></variables><block type='start' id='k$AXCT@o!OFRn?!AKy7s' x='209' y='117'><statement name='program'><block type='inputDeclaration' id='yS[w(DOccgvj~IyHW~kr'><field name='moduleName' id='d18):Ym8d801JUmRP_Ez' variabletype=''>Fisk</field><field name='inputName' id='#N6^?rxXqdzB84l3~TlB' variabletype=''>input1</field><field name='inputAmount'>10</field><field name='inputUnit'>0</field><next><block type='inputDeclaration' id='-;rm$c_,lV(!!-[q0JJ)'><field name='moduleName' id='t+S_%SkicX]kbQ7F3:{y' variabletype=''>Kage</field><field name='inputName' id=':dc*?i]h}tb_#YN_*fY2' variabletype=''>input2</field><field name='inputAmount'>10</field><field name='inputUnit'>0</field><next><block type='outputDeclaration' id='rMBV]zGBgk$!(,LD(@XR'><field name='moduleName' id='XtZ/5I:ExlN+U5it{D7M' variabletype=''>output</field><next><block type='fluid' id='LP/)%)`v;$2n}~k[wS{g'><field name='fluidName' id='H?$uV_#2{w_1bQCD#fQ5' variabletype=''>fluidVariableToReassign</field><value name='inputFluid'><block type='getFluid' id='5.mQ~iheqq0NHV;NE]fT'><field name='fluidName' id='#N6^?rxXqdzB84l3~TlB' variabletype=''>input1</field><field name='fluidAmount'>5</field><field name='useAllFluid'>FALSE</field></block></value><next><block type='fluid' id='@9M3d}+i?o@$+l{AGye.'><field name='fluidName' id='H?$uV_#2{w_1bQCD#fQ5' variabletype=''>fluidVariableToReassign</field><value name='inputFluid'><block type='getFluid' id='0upX@jOdkh)Ch-E-k.~;'><field name='fluidName' id=':dc*?i]h}tb_#YN_*fY2' variabletype=''>input2</field><field name='fluidAmount'>5</field><field name='useAllFluid'>FALSE</field></block></value><next><block type='outputUseage' id='00kdbV!=Y4#y[%E={?G`'><field name='moduleName' id='XtZ/5I:ExlN+U5it{D7M' variabletype=''>output</field><value name='inputFluid'><block type='getFluid' id='t-1H6G]*s3{z!_S5Hb,1'><field name='fluidName' id='H?$uV_#2{w_1bQCD#fQ5' variabletype=''>fluidVariableToReassign</field><field name='fluidAmount'>5</field><field name='useAllFluid'>FALSE</field></block></value></block></next></block></next></block></next></block></next></block></next></block></statement></block></xml>"},{name: "SemiParallelMixing", inputs: ["H2O","CH3OH"], outputs: ["output"], programXml: "<xml xmlns='http://www.w3.org/1999/xhtml'><variables><variable type='' id='1WrdW_4?6A2K,dyOkVd]'>input</variable><variable type='' id='eN`j$n=r|;a-nJP4i}*`'>H2O</variable><variable type='' id='=q9em!V@p:mQ_n+*v6qt'>transfer1</variable><variable type='' id=')7C`nm$w.O]wh5e(w`}Y'>mix1</variable><variable type='' id='gi)to,#oa`tqJG5.cjr*'>transfer2</variable><variable type='' id='eEJRMP:;f%80y*=Nh[(u'>output</variable><variable type='' id='HGDkd~e{/Tr31ACi8iSB'>CH3OH</variable><variable type='' id='o6t^WC%c4gu+#O9?$#=8'>input2</variable><variable type='' id='[#eYap7V62]15j%YCNB%'>fluid_name</variable><variable type='' id='K/YN%+9T+D8lR!vzz:f2'>mix2</variable><variable type='' id='~Ot@~IApz24dt9=T;c@('>mix3</variable><variable type='' id='rc0~Yn~:{fyAiMn$7x!Q'>module_name</variable></variables><block type='start' id='!wy`Yw(pCm^G)enH$tf1' x='237' y='176'><statement name='program'><block type='inputDeclaration' id='Is91_rabIM_Fv$;dm4v|'><field name='moduleName' id='1WrdW_4?6A2K,dyOkVd]' variabletype=''>input</field><field name='inputName' id='eN`j$n=r|;a-nJP4i}*`' variabletype=''>H2O</field><field name='inputAmount'>10</field><field name='inputUnit'>0</field><next><block type='inputDeclaration' id='7E=00n@5X%ee:4ovu(09'><field name='moduleName' id='o6t^WC%c4gu+#O9?$#=8' variabletype=''>input2</field><field name='inputName' id='HGDkd~e{/Tr31ACi8iSB' variabletype=''>CH3OH</field><field name='inputAmount'>10</field><field name='inputUnit'>0</field><next><block type='outputDeclaration' id='|g5U1?Ft]n@qL$W~}IR;'><field name='moduleName' id='eEJRMP:;f%80y*=Nh[(u' variabletype=''>output</field><next><block type='fluid' id='%TmW712j#U4Fk1x8hb-!'><field name='fluidName' id=')7C`nm$w.O]wh5e(w`}Y' variabletype=''>mix1</field><value name='inputFluid'><block type='mixer' id='piYnG^+ku,z$(DgoKj_}'><value name='inputFluidA'><block type='getFluid' id='a[G*Q3i}Cy/#d{Y(F{oO'><field name='fluidName' id='HGDkd~e{/Tr31ACi8iSB' variabletype=''>CH3OH</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value><value name='inputFluidB'><block type='getFluid' id='frhEF~mfT[]tb-QQTwm6'><field name='fluidName' id='eN`j$n=r|;a-nJP4i}*`' variabletype=''>H2O</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value></block></value><next><block type='fluid' id='4J4%:5*Slp;0b%kzrSdA'><field name='fluidName' id='K/YN%+9T+D8lR!vzz:f2' variabletype=''>mix2</field><value name='inputFluid'><block type='mixer' id='bglNouNi1,yuKM$j}G07'><value name='inputFluidA'><block type='getFluid' id='1vj5~lk){/4nynh5%}2%'><field name='fluidName' id=')7C`nm$w.O]wh5e(w`}Y' variabletype=''>mix1</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value><value name='inputFluidB'><block type='getFluid' id='~bll1jW:`Y8NqCCLclW^'><field name='fluidName' id='eN`j$n=r|;a-nJP4i}*`' variabletype=''>H2O</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value></block></value><next><block type='fluid' id='Oees}bwET!}RFzh]^ovK'><field name='fluidName' id='~Ot@~IApz24dt9=T;c@(' variabletype=''>mix3</field><value name='inputFluid'><block type='mixer' id='yZRWZbOxjj8?w3~u@,Ls'><value name='inputFluidA'><block type='getFluid' id='4C+sTz6FK~OjNnb:RR7h'><field name='fluidName' id='HGDkd~e{/Tr31ACi8iSB' variabletype=''>CH3OH</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value><value name='inputFluidB'><block type='getFluid' id='6tFx-A-V1=t([epT0z[.'><field name='fluidName' id='eN`j$n=r|;a-nJP4i}*`' variabletype=''>H2O</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value></block></value><next><block type='outputUseage' id='JK,-33G~#.S%4z{=anw]'><field name='moduleName' id='eEJRMP:;f%80y*=Nh[(u' variabletype=''>output</field><value name='inputFluid'><block type='getFluid' id='=WniOV1pdQIU%zh:0Est'><field name='fluidName' id=')7C`nm$w.O]wh5e(w`}Y' variabletype=''>mix1</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value><next><block type='outputUseage' id='lA,Y@c||QNV%~St~x7w`'><field name='moduleName' id='eEJRMP:;f%80y*=Nh[(u' variabletype=''>output</field><value name='inputFluid'><block type='getFluid' id='HTOb*+,RZ6P1i88p1d/z'><field name='fluidName' id='K/YN%+9T+D8lR!vzz:f2' variabletype=''>mix2</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value><next><block type='outputUseage' id='OO])C$b))d1~v?5hHeC%'><field name='moduleName' id='eEJRMP:;f%80y*=Nh[(u' variabletype=''>output</field><value name='inputFluid'><block type='getFluid' id='!h4m/?ZOH*@|@HJ{nhNK'><field name='fluidName' id='~Ot@~IApz24dt9=T;c@(' variabletype=''>mix3</field><field name='fluidAmount'>2</field><field name='useAllFluid'>FALSE</field></block></value></block></next></block></next></block></next></block></next></block></next></block></next></block></next></block></next></block></statement></block></xml>"},{name: "SequentialMixing", inputs: ["input1","input2"], outputs: ["output"], programXml: "<xml xmlns='http://www.w3.org/1999/xhtml'><variables><variable type='' id='d18):Ym8d801JUmRP_Ez'>Kage</variable><variable type='' id='#N6^?rxXqdzB84l3~TlB'>input1</variable><variable type='' id=':dc*?i]h}tb_#YN_*fY2'>input2</variable><variable type='' id='d!)Y+j*lK7X167Y6w5pj'>mix1</variable><variable type='' id='H?$uV_#2{w_1bQCD#fQ5'>fluid_name</variable><variable type='' id='gzTQ:0+1u44Bl6j)af}*'>mix2</variable><variable type='' id='XtZ/5I:ExlN+U5it{D7M'>output</variable><variable type='' id='t+S_%SkicX]kbQ7F3:{y'>module_name</variable></variables><block type='start' id='k$AXCT@o!OFRn?!AKy7s' x='209' y='117'><statement name='program'><block type='inputDeclaration' id='yS[w(DOccgvj~IyHW~kr'><field name='moduleName' id='d18):Ym8d801JUmRP_Ez' variabletype=''>Kage</field><field name='inputName' id='#N6^?rxXqdzB84l3~TlB' variabletype=''>input1</field><field name='inputAmount'>10</field><field name='inputUnit'>0</field><next><block type='inputDeclaration' id='Sgl|ifMe0ZCgAq[DU(Yq'><field name='moduleName' id='d18):Ym8d801JUmRP_Ez' variabletype=''>Kage</field><field name='inputName' id=':dc*?i]h}tb_#YN_*fY2' variabletype=''>input2</field><field name='inputAmount'>10</field><field name='inputUnit'>0</field><next><block type='outputDeclaration' id='rMBV]zGBgk$!(,LD(@XR'><field name='moduleName' id='XtZ/5I:ExlN+U5it{D7M' variabletype=''>output</field><next><block type='fluid' id='R`[x{_~Cj_hYDNFeZpM5'><field name='fluidName' id='d!)Y+j*lK7X167Y6w5pj' variabletype=''>mix1</field><value name='inputFluid'><block type='mixer' id='[:Qw|2z2j!V#l3-@}+UM'><value name='inputFluidA'><block type='getFluid' id='oo4zgRQ:z$zMABJ8m=.R'><field name='fluidName' id=':dc*?i]h}tb_#YN_*fY2' variabletype=''>input2</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value><value name='inputFluidB'><block type='getFluid' id='[dwSRAtt7UOZ1Y5nd-jM'><field name='fluidName' id='#N6^?rxXqdzB84l3~TlB' variabletype=''>input1</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value></block></value><next><block type='fluid' id='sUb6P7@q{~B^Y9[RnaQO'><field name='fluidName' id='gzTQ:0+1u44Bl6j)af}*' variabletype=''>mix2</field><value name='inputFluid'><block type='mixer' id='ka{#I*S_kAXj0_!|Gz}^'><value name='inputFluidA'><block type='getFluid' id='s?$Bz{9JY]lGwATPn2Q6'><field name='fluidName' id='d!)Y+j*lK7X167Y6w5pj' variabletype=''>mix1</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value><value name='inputFluidB'><block type='getFluid' id='DXMm@97!JQma?mDYag`P'><field name='fluidName' id='#N6^?rxXqdzB84l3~TlB' variabletype=''>input1</field><field name='fluidAmount'>1</field><field name='useAllFluid'>FALSE</field></block></value></block></value><next><block type='outputUseage' id='iyjdS7-L=!D#l%J4q=6)'><field name='moduleName' id='XtZ/5I:ExlN+U5it{D7M' variabletype=''>output</field><value name='inputFluid'><block type='getFluid' id='eNPtl4]`}jb9^/M[7{~C'><field name='fluidName' id='gzTQ:0+1u44Bl6j)af}*' variabletype=''>mix2</field><field name='fluidAmount'>2</field><field name='useAllFluid'>FALSE</field></block></value></block></next></block></next></block></next></block></next></block></next></block></statement></block></xml>"},{name: "SimpleFluidTransfer", inputs: ["H2O"], outputs: ["output"], programXml: "<xml xmlns='http://www.w3.org/1999/xhtml'><variables><variable type='' id='1WrdW_4?6A2K,dyOkVd]'>input</variable><variable type='' id='eN`j$n=r|;a-nJP4i}*`'>H2O</variable><variable type='' id='=q9em!V@p:mQ_n+*v6qt'>transfer1</variable><variable type='' id=')7C`nm$w.O]wh5e(w`}Y'>fluid_name</variable><variable type='' id='gi)to,#oa`tqJG5.cjr*'>transfer2</variable><variable type='' id='eEJRMP:;f%80y*=Nh[(u'>output</variable></variables><block type='start' id='!wy`Yw(pCm^G)enH$tf1' x='237' y='176'><statement name='program'><block type='inputDeclaration' id='Is91_rabIM_Fv$;dm4v|'><field name='moduleName' id='1WrdW_4?6A2K,dyOkVd]' variabletype=''>input</field><field name='inputName' id='eN`j$n=r|;a-nJP4i}*`' variabletype=''>H2O</field><field name='inputAmount'>10</field><field name='inputUnit'>0</field><next><block type='outputDeclaration' id='m^,y!E4oABaS0sQ?0bOl'><field name='moduleName' id='eEJRMP:;f%80y*=Nh[(u' variabletype=''>output</field><next><block type='fluid' id='20[?^RM%S%ZXymp67z5e'><field name='fluidName' id='=q9em!V@p:mQ_n+*v6qt' variabletype=''>transfer1</field><value name='inputFluid'><block type='getFluid' id='K`#/o{eOM`4+POXZ.`S4'><field name='fluidName' id='eN`j$n=r|;a-nJP4i}*`' variabletype=''>H2O</field><field name='fluidAmount'>5</field><field name='useAllFluid'>FALSE</field></block></value><next><block type='fluid' id='OaezQUpaR=PScg4mGtqL'><field name='fluidName' id='gi)to,#oa`tqJG5.cjr*' variabletype=''>transfer2</field><value name='inputFluid'><block type='getFluid' id='3iXw1Eklz;om5`uUnh/)'><field name='fluidName' id='=q9em!V@p:mQ_n+*v6qt' variabletype=''>transfer1</field><field name='fluidAmount'>3</field><field name='useAllFluid'>FALSE</field></block></value><next><block type='outputUseage' id='0xJnH3qs9kRa++0k{9#9'><field name='moduleName' id='eEJRMP:;f%80y*=Nh[(u' variabletype=''>output</field><value name='inputFluid'><block type='getFluid' id='Ihwaml/i{v9b3rE}4r%b'><field name='fluidName' id='gi)to,#oa`tqJG5.cjr*' variabletype=''>transfer2</field><field name='fluidAmount'>3</field><field name='useAllFluid'>FALSE</field></block></value></block></next></block></next></block></next></block></next></block></statement></block></xml>"},{name: "SimpleInputOutput", inputs: ["H2O"], outputs: ["Bar"], programXml: "<xml xmlns='http://www.w3.org/1999/xhtml'><variables><variable type='' id='e08F0v|9?K|QM*DFU;d.'>Bar</variable><variable type='' id='E_]NGx^Y6B3S8ml+{`:m'>Foo</variable><variable type='' id='St_B8UY3s#6t^(^_aYv|'>fluid_name</variable><variable type='' id='Q#R$h1IxxD0+r;08|r`L'>H2O</variable></variables><block type='start' id='{?CHz?dE}psPooSDsvwt' x='193' y='71'><statement name='program'><block type='inputDeclaration' id='*;sK8$HA+EpRKr2@jd3N'><field name='moduleName' id='E_]NGx^Y6B3S8ml+{`:m' variabletype=''>Foo</field><field name='inputName' id='Q#R$h1IxxD0+r;08|r`L' variabletype=''>H2O</field><field name='inputAmount'>5</field><field name='inputUnit'>0</field><next><block type='outputDeclaration' id='XYvBOE)}80wUPg7[Nqk*'><field name='moduleName' id='e08F0v|9?K|QM*DFU;d.' variabletype=''>Bar</field><next><block type='outputUseage' id='Eaz/7fI%B)O|[o$N(nal'><field name='moduleName' id='e08F0v|9?K|QM*DFU;d.' variabletype=''>Bar</field><value name='inputFluid'><block type='getFluid' id=';+*A;~Tn-xREhG*5uf!{'><field name='fluidName' id='Q#R$h1IxxD0+r;08|r`L' variabletype=''>H2O</field><field name='fluidAmount'>5</field><field name='useAllFluid'>FALSE</field></block></value></block></next></block></next></block></statement></block></xml>"}]);
}











