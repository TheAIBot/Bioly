function afterLoading() {
    document.getElementById("simulatorCanvas").addEventListener('mousemove', function (e) {
        if (e.buttons == 1) {
            const canvas = document.getElementById("simulatorCanvas");
            offsetCurrentViewPosition(e.movementX / canvas.width, -e.movementY / canvas.height);
            didGraphicsChange = true;
        }
    });

    document.getElementById("simulatorCanvas").addEventListener('wheel', function (e) {
        changeZoom(e.deltaY > 0 ? -0.1 : 0.1);
        e.preventDefault();
        didGraphicsChange = true;
    });

    initSimulator();
    initSimulatorRender();

    openTab(event, 'simulatorCanvas');
}

function sleep(time) {
    return new Promise((resolve) => setTimeout(resolve, time));
}

async function getFileContent(filePath) {
    let fileContent = null;
    await fetch("programs/diluter.txt").then(x => {
        if (x.status == 200) {
            fileContent = x.text();
        } else {
            fileContent = null;
        }
    });

    return fileContent;
}