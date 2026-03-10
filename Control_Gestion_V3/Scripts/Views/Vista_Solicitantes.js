var tabladata;
$(document).ready(function () {
    activarMenu("Solicitantes");
    // Validación del formulario
   $("#form").validate({
        rules: {
            Cedula: {
                required: true
            },
            Nombre: {
                required: true
            },
            Correo: {
                required: true,
                email: true
            }
        },
        messages: {
            Cedula: {
                required: "(*) Ingresa tu número de identificación",
                digits: "(*) Solo se permiten números"
            },
            Nombre: "(*) El campo es requerido",
            Correo: {
                required: "(*) Ingresa correo electrónico",
                email: "(*) Ingresa un correo válido"
            }
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            error.addClass('text-danger error'); // Agrega ambas clases si deseas usar `.error` en el reset
            error.insertAfter(element);
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
        }
    });
    // Limpiar al cerrar el modal
    $('#FormModal').on('hidden.bs.modal', function () {
        $('#form')[0].reset();
        $('#form .is-valid, #form .is-invalid').removeClass('is-valid is-invalid');
        $('#form span.error, #form span.text-danger').remove(); // Limpia ambos por si acaso
    });
    // Inicialización de la tabla
     tabladata = $('#tbdata').DataTable({
        "ajax": {
            "url": $.MisUrls.url._ObtenerSolicitates,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "ID_Usuario" },
            { "data": "TipoDocumento" },
            { "data": "Cedula_Solicitante" },
            { "data": "Nombre" },
            {
                "data": "Direccion",
                "render": function (data) {
                    return data ?? "<i class='text-muted'>No asignado</i>";
                }
            },
            {
                "data": "Telefono",
                "render": function (data) {
                    return data != null ? data : "<i class='text-muted'>No asignado</i>";
                }
            },
            {
                "data": "Telefono2",
                "render": function (data) {
                    return data != null ? data : "<i class='text-muted'>No asignado</i>";
                }
            },
            {
                "data": "Fax",
                "render": function (data) {
                    return data != null ? data : "<i class='text-muted'>No asignado</i>";
                }
            },
            { "data": "Correo" },
            {
                "data": "FechaRegistro",
                "render": function (data) {
                    return formatearFecha(data);
                }
            },
            {
                "data": "Activo",
                "render": function (data) {
                    return data
                        ? '<span class="badge bg-success">Activo</span>'
                        : '<span class="badge bg-danger">No Activo</span>';
                }
            },
            {
                "data": "Cedula_Solicitante",
                "render": function (data, type, row, meta) {
                    return `
                    <button class='btn btn-primary btn-sm btn-editar' data-id='${row.Cedula_Solicitante}'>
                        <i class='fas fa-pen'></i>
                    </button>
                    <button class='btn btn-danger btn-sm ms-2 btn-eliminar' data-id='${row.Cedula_Solicitante}'>
                        <i class='fa fa-trash'></i>
                    </button>`;
                },
                "orderable": false,
                "searchable": false,
                "width": "40px"
            }
        ],
        "language": {
            "url": $.MisUrls.url.Url_datatable_spanish
        },
        "scrollX": true,
        "responsive": false,
        "paging": true,
        "autoWidth": false,
        "dom": 'Bfrtip',
        "buttons": [
            {
                extend: 'excelHtml5',
                text: 'Exportar a Excel',
                title: 'Datos de Usuarios',
            },
            'pageLength',
        ]
    });

});

// Delegación de eventos para botón Editar
$('#tbdata tbody').on('click', '.btn-editar', function () {
    const cedula = $(this).data('id');
    EditarSolicitante(cedula);
});

// Delegación de eventos para botón Eliminar
$('#tbdata tbody').on('click', '.btn-eliminar', function () {
    const cedula = $(this).data('id');
    eliminar(cedula);
});



function formatearFecha(fecha) {
    if (!fecha) return '';
    const timestamp = parseInt(fecha.replace(/\/Date\((\d+)\)\//, '$1'));
    const dateObj = new Date(timestamp);

    // Formatear la fecha y la hora en formato español
    return dateObj.toLocaleString('es-ES', {
        dateStyle: 'short',
        timeStyle: 'short'
    });
}
function EditarSolicitante(Cedula_Solicitante) {
    window.location.href = '/Solicitantes/EditarSolicitante?Cedula_Solicitante=' + Cedula_Solicitante;
}
function abrirPopUpForm() {
    $("#txtid").val(0);
    $("#cbotipodocumento").val($("#cbotipodocumento option:first").val());
    $("#inputCedula").val("");
    $("#txtNombre").val("");
    $("#txtDireccion").val("");
    $("#inputTelefono").val("");
    $("#inputFax").val("");
    $("#txtCorreo").val("");
    $("#cboEstado").val(1);
    $('#FormModal').modal('show');
}
function Guardar() {
    if ($("#form").valid()) {
        var request = {
            objeto: {
                //  ID_Proceso: $("#txtId").val(),
                ID_Usuario: $("#txtId").val(),
                TipoDocumento: $("#cbotipodocumento").val(),
                Cedula_Solicitante: $("#inputCedula").val(),
                Nombre: $("#txtNombre").val(),
                Direccion: $("#txtDireccion").val(),
                Telefono: $("#inputTelefono").val(),
                Fax: $("#inputFax").val(),
                Correo: $("#txtCorreo").val(),
                Activo: parseInt($("#cboEstado").val()) == 1 ? true : false
            }
        }

        jQuery.ajax({
            url: $.MisUrls.url._GuardarSolicitantes,
            type: "POST",
            data: JSON.stringify(request),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {

                if (data.resultado) {
                    tabladata.ajax.reload();
                    $('#FormModal').modal('hide');
                    swal("Mensaje", "Usuario guardado exitosamente", "warning")
                } else {
                    tabladata.ajax.reload();
                    $('#FormModal').modal('hide');
                    swal("Mensaje", "No se pudieron guardar los cambios.Verifica si el usuario ya existe", "warning")
                 
                }
            },
            error: function (error) {
                console.log(error)
            },
            beforeSend: function () {

            },
        });
    }
}

function eliminar(Cedula_Solicitante) {
    swal({
        title: "Mensaje",
        text: "El usuario con ID: " + Cedula_Solicitante + " será eliminado permanentemente. ¿Seguro que deseas continuar?",
        type: "warning",
        showCancelButton: true,
        confirmButtonText: "Si",
        confirmButtonColor: "#DD6B55",
        cancelButtonText: "No",
        closeOnConfirm: true
    },

        function () {
            jQuery.ajax({
                url: $.MisUrls.url._EliminarSolicitantes + "?Cedula_Solicitante=" + Cedula_Solicitante,
                type: "GET",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.resultado) {
                        tabladata.ajax.reload();
                        setTimeout(function () {
                            swal("Éxito", "Usuario eliminado exitosamente", "success");
                        }, 200);
                    } else {
                        setTimeout(function () {
                            swal("Mensaje", "No se pudo eliminar el usuario porque está asignado a un caso", "warning");
                        }, 200);
                    }
                },
                error: function (error) {
                    console.log(error)
                },
                beforeSend: function () {

                },
            });
        });
}