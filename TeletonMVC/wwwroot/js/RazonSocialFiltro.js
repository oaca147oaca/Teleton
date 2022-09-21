

document.getElementById("Seleccionaso").addEventListener("click", intermedia)

function intermedia() {
    let valSeleccionaso = document.getElementById("Seleccionaso").value
    TraerValor(valSeleccionaso)
}

function TraerValor(valor) {
    if (valor != null) {
        RecorrerLista(valor);
    }

}

function RecorrerLista(valor) {
    const constante = $.getJSON("../Colaborador/TraerTodosTipoColJSON", function (tipoCols) {

        let objetos = JSON.parse(tipoCols)
        var div = document.getElementById("razonDiv");
        for (var i = 0; i < objetos.length; i++) {
            if (objetos[i].Id == valor && objetos[i].Nombre == "Empresa") {
               
                div.style.display = "block";
                break;
            } else {
                div.style.display = "none";
            }
        }
    });
}