
$.datepicker.regional['es'] = {
    closeText: 'Cerrar',
    prevText: '< Ant',
    nextText: 'Sig >',
    currentText: 'Hoy',
    monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
        'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
    monthNamesShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun',
        'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
    dayNames: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
    dayNamesShort: ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'],
    dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sá'],
    weekHeader: 'Sm',
    dateFormat: 'dd/mm/yy',
    firstDay: 1,
    isRTL: false,
    showMonthAfterYear: false,
    yearSuffix: ''
};

// Establecer la configuración regional por defecto
$.datepicker.setDefaults($.datepicker.regional['es']);
$("#txtFechaInicio").datepicker();
$("#txtFechaFin").datepicker();
$("#txtFechaInicio").val(ObtenerFecha());
$("#txtFechaFin").val(ObtenerFecha());

function ObtenerFecha() {
    var d = new Date();
    var month = d.getMonth() + 1;
    var day = d.getDate();
    var output = (('' + day).length < 2 ? '0' : '') + day + '/' + (('' + month).length < 2 ? '0' : '') + month + '/' + d.getFullYear();

    return output;
}

function formatearFecha(fecha) {
    if (!fecha || typeof fecha !== 'string') return '';

    const match = fecha.match(/\/Date\((\d+)\)\//);
    if (!match) return '';

    const timestamp = parseInt(match[1], 10);
    const dateObj = new Date(timestamp);

    if (isNaN(dateObj)) return '';

    return dateObj.toLocaleString('es-ES', {
        dateStyle: 'short',
        timeStyle: 'short'
    });
}
function formatearHora(data) {
    if (data) {
        // Verificar si el objeto contiene las claves necesarias
        const { Hours, Minutes, Milliseconds } = data;

        // Validar que existan los valores requeridos
        if (typeof Hours === "number" && typeof Minutes === "number") {
            // Formatear la hora a un formato legible HH:mm:ss
            const horas = String(Hours).padStart(2, "0");
            const minutos = String(Minutes).padStart(2, "0");
            const segundos = String(Math.floor(Milliseconds / 1000) || 0).padStart(2, "0");

            return `${horas}:${minutos}:${segundos}`; // Formato HH:mm:ss
        }
    }
    return ""; // Retorna vacío si no hay datos válidos
}

// Controlar la visibilidad de campos según el valor de cboProcedencia
$("#cboProcedencia").on("change", function () {
    const selectedValue = $(this).val();

    if (selectedValue === "1") {
        // Ocultar ambos campos
        $("#txtNom_Procedencia").closest(".mb-3").hide();
        $("#cboDepartamento").closest(".mb-3").hide();
    } else if (selectedValue === "5") {
        // Mostrar solo el campo cboDepartamento
        $("#txtNom_Procedencia").closest(".mb-3").hide();
        $("#cboDepartamento").closest(".mb-3").show();
    } else if (["2", "3", "4"].includes(selectedValue)) {
        // Mostrar solo el campo txtNom_Procedencia
        $("#txtNom_Procedencia").closest(".mb-3").show();
        $("#cboDepartamento").closest(".mb-3").hide();
    } else {
        // Ocultar ambos campos como fallback
        $("#txtNom_Procedencia").closest(".mb-3").hide();
        $("#cboDepartamento").closest(".mb-3").hide();
    }
});




/************************************************CAMPOS SELECT**************************************** */
$(document).ready(function () {
    cargarComboGestiones('#cboGestion');
    cargarComboGestiones('#cboGestion2'); // segundo select
});

function cargarComboGestiones(selectId) {
    $.ajax({
        url: $.MisUrls.url._ObtenerTipoGestion,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.data) {
                const $select = $(selectId);
                $select.empty(); // Limpia el select

                // Agrega opción por defecto (solo si NO es cboGestion2)
                if (selectId !== '#cboGestion2') {
                    $select.append('<option value="">Seleccione</option>');
                }

                let primerValorActivo = null;

                response.data.forEach(function (gestion) {
                    if (gestion.Activo) {
                        const optionHtml = `<option value="${gestion.Cod_Gestion}">${gestion.Descripcion}</option>`;
                        $select.append(optionHtml);

                        // Guarda el primer valor activo
                        if (!primerValorActivo) {
                            primerValorActivo = gestion.Cod_Gestion;
                        }
                    }
                });

                // Si es cboGestion2, selecciona el primer valor activo automáticamente
                if (selectId === '#cboGestion2' && primerValorActivo) {
                    $select.val(primerValorActivo).trigger('change');
                }
            }
        },
        error: function (error) {
            console.error("Error al obtener los tipos de gestión:", error);
        }
    });
}


$(document).ready(function () {
    $("#cboGestion").trigger("change");
    $("#cboGestion2").trigger("change");
});
// Obtener procedencias
$.ajax({
    url: $.MisUrls.url._ObtenerProcedencia,
    type: 'GET',
    dataType: 'json',
    success: function (response) {
        if (response.data) {
            response.data.forEach(function (procedencia) {
                if (procedencia.Activo) {
                    $('#cboProcedencia').append(
                        `<option value="${procedencia.Cod_Procedencia}">${procedencia.Descripcion}</option>`
                    );
                }
            });
        }
    },
    error: function (error) {
        console.error("Error al obtener las procedencias:", error);
    }
});
// Configuración inicial para ocultar o mostrar los campos según la opción seleccionada
$(document).ready(function () {
    $("#cboProcedencia").trigger("change");
});
// Obtener departamentos
$.ajax({
    url: $.MisUrls.url._ObtenerDepartamentos,
    type: 'GET',
    dataType: 'json',
    success: function (response) {
        if (response.data) {
            response.data.forEach(function (departamento) {
                if (departamento.Activo) {
                    $('#cboDepartamento').append(
                        `<option value="${departamento.Descripcion}">${departamento.Descripcion}</option>`
                    );
                }
            });
        }
    },
    error: function (error) {
        console.error("Error al obtener los departamentos:", error);
    }
});

/****************************************************MOSTRA FORMULARIO DE CASOS ***********************************/
function abrirPopUpForm() {
    $("#txtIdCaso").val(0);
    $("#cboProcedencia").val($("#cboProcedencia option:first").val());
    $("#txtTema").val("");
    $("#txtNom_Procedencia").val("");
    $("#cboUrgencia").val($("#cboUrgencia option:first").val());
    $("#cboGestion").val($("#cboGestion option:first").val());
    $("#txtCedulaDemandante").val("");
    $("#txtCedulaDemandado").val("");
    $("#txtFechaLimite").val("");
    $("#cboEstado").val($("#cboEstado option:first").val());
    $("#txtDniResponsable").val("");
    $("#cboParte").val($("#cboParte option:first").val());
    $('#FormModal').modal('show');
}
//Limpiar Formulario de casos 
function limpiarFiltros() {
    $("#txtFechaInicio").val('');
    $("#txtFechaFin").val('');
    $("#filterNumExpediente").val('');
    $("#txtCedulaDemandante").val('');
    $("#cboUrgencia").val('');
    $("#cboEstado").val('');
    $("#cboGestion").val('');
    $("#cboProcedencia").val('');
}
function CargarTodos() {
    window.location.reload();
}
function buscar() {
    // Crear un objeto para los parámetros
    // Obtener el valor de CondicionalCasos
    const condicional = $("#CondicionalCasos").val();

    // Crear un objeto para los parámetros
    const parametros = {
        fechainicio: $("#txtFechaInicio").val().trim(),
        fechafin: $("#txtFechaFin").val().trim(),
        estado: $("#cboEstado").val(),
        tipoGestion: $("#cboGestion").val()
    };
    if (condicional == "1") {
        Cedula_Solicitante: $("#cedulaUsuario").val().trim();
    }
    // Si CondicionalCasos es distinto de "1", agregar los parámetros adicionales
    if (condicional == "2") {
        parametros.Num_Expediente = $("#filterNumExpediente").val().trim();
        parametros.Grado_de_Urgencias = $("#cboUrgencia").val();
        parametros.procedencia = $("#cboProcedencia").val();
    }
    // Validar que al menos un filtro esté lleno
    const hayFiltros = Object.values(parametros).some(valor => valor !== "");
    if (!hayFiltros) {
        swal("Mensaje", "Debe ingresar al menos un filtro para realizar la búsqueda", "warning");
        return;
    }
    // Construir la URL solo con los filtros que tengan valor
    const queryString = Object.keys(parametros)
        .filter(key => parametros[key] !== "") // Filtrar solo parámetros con valor
        .map(key => `${key}=${encodeURIComponent(parametros[key])}`) // Crear pares clave=valor
        .join("&");
    const url = `${$.MisUrls.url._ObtenerCasos}?${queryString}`;
    tabladata.ajax.url(url).load(() => {
        // Callback después de cargar los datos: limpiar los campos
        $("#txtFechaInicio").val('');
        $("#txtFechaFin").val('');
        $("#filterNumExpediente").val('');
        $("#txtCedulaDemandante").val('');
        $("#cboUrgencia").val('');
        $("#cboEstado").val('');
        $("#cboGestion").val('');
        $("#cboProcedencia").val('');
    });
}