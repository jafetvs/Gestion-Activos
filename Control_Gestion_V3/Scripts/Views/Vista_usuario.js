var tabladata;

$(document).ready(function () {
    activarMenu("Seguridad");

    // Configuración de validación del formulario
   $("#form").validate({
        rules: { // Reglas de validación
            Cedula_Usuario: {
                required: true
            },
            Nom_Completo: {
                required: true
            },
            Nom_User: {
                required: true
            },
            Correo: {
                required: true,
                email: true
            }
        },
        messages: { // Mensajes de error personalizados
            Cedula: {
                required: "(*) Ingresa tu número de identificación",
                digits: "(*) Solo se permiten números"
            },
            Nom_Completo: "(*) El campo es requerido",
            Nom_User: "(*) El campo es requerido",
            Correo: {
                required: "(*) Ingresa correo electrónico",
                email: "(*) Ingresa un correo válido"
            }
        },
        errorElement: 'span', // Define el elemento para mostrar los errores
        errorPlacement: function (error, element) { // Controla dónde mostrar los errores
            error.addClass('text-danger');
            error.insertAfter(element);
        },
        highlight: function (element, errorClass, validClass) { // Resalta el campo con error
            $(element).addClass('is-invalid').removeClass('is-valid');
        },
        unhighlight: function (element, errorClass, validClass) { // Elimina el resaltado al corregir
            $(element).removeClass('is-invalid').addClass('is-valid');
        }
    });
    // Limpiar el formulario y los campos cuando el modal se cierra
    $('#FormModal').on('hidden.bs.modal', function () {
        // Resetear el formulario
        $('#form')[0].reset();
        $('#form .is-valid, #form .is-invalid').removeClass('is-valid is-invalid');
        // Limpiar las clases de validación (si alguna fue aplicada)
        $('#form .is-valid, #form .is-invalid').removeClass('is-valid is-invalid');
        // Limpiar los mensajes de error (eliminar texto en los <span>)
        $('#form span.error').remove();
    });
   

    // Cargar roles en el combobox

    jQuery.ajax({
        url: $.MisUrls.url._ObtenerRoles,
        type: "GET",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $("#cboRol").html("");
            if (data.data != null) {
                $.each(data.data, function (i, item) {
                    if (item.Activo == true) {
                        $("<option>").attr({ "value": item.IdRol }).text(item.Descripcion).appendTo("#cboRol");
                    }
                });
                $("#cboRol").val($("#cboRol option:first").val());
                $("#txtNom_Rol").val($("#cboRol option:selected").text());
            }
        },
        error: function (error) {
            console.log(error);
        }

    });

    // Actualizar el nombre del rol al cambiar la selección
    $("#cboRol").change(function () {
        $("#txtNom_Rol").val($("#cboRol option:selected").text());
    });

    tabladata = $('#tbdata').DataTable({
        "ajax": {
            "url": $.MisUrls.url._ObtenerUsuarios,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "IdUsuario" },
            {
                "data": "oRol", render: function (data) {
                    return data.Descripcion;
                }
            },
            { "data": "Cedula_Usuario" },
            { "data": "Nom_User" },
            { "data": "Nom_Completo" },
            { "data": "Correo" },
            {
                "data": "Direccion",
                "render": function (data) {
                    return data ?? "<i class='text-muted'>No asignado</i>";
                }
            },
            {
                "data": "Telefono1",
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
            {
                "data": "FechaRegistro",
                "render": function (data) {
                    return formatearFecha(data);
                }
            },
            {
                "data": "Activo", "render": function (data) {
                    return data
                        ? '<span class="badge bg-success">Activo</span>'
                        : '<span class="badge bg-danger">No Activo</span>';
                }
            },
            {
                "data": "Cedula",
                "render": function (data, type, row, meta) {
                    return "<button class='btn btn-primary btn-sm' type='button' onclick='EditarUsuario(" + row.Cedula_Usuario + ")'><i class='fas fa-pen'></i></button>" +
                        "<button class='btn btn-danger btn-sm ml-2' type='button' onclick='eliminar(" + row.Cedula_Usuario + ")'><i class='fa fa-trash'></i></button>";

                },
                "orderable": false,
                "searchable": false,
                "width": "40px"
            }
        ],
        "language": {
            "url": $.MisUrls.url.Url_datatable_spanish
        },
        scrollX: true,
        responsive: false,
        paging: true,
        autoWidth: false,
        dom: 'Bfrtip',
        /* scrollX: true,
         responsive: false,
         paging: true,
         autoWidth: false,
         dom: 'Bfrtip',*/
        buttons: [
            {
                extend: 'excelHtml5',
                text: 'Exportar a Excel',
                title: 'Datos de Usuarios'
            },
            'pageLength',
        ]
    });
    // Solución: ajustar columnas tras renderizado
    setTimeout(function () {
        tabladata.columns.adjust().draw();
    }, 300);

});
function EditarUsuario(Cedula_Usuario) {
    window.location.href = '/Usuario/EditarUsuario?Cedula_Usuario=' + Cedula_Usuario;
}


// Formatear fechas
function formatearFecha(fecha) {
    if (!fecha) return '';
    const timestamp = parseInt(fecha.replace(/\/Date\((\d+)\)\//, '$1'));
    const dateObj = new Date(timestamp);
    return dateObj.toLocaleString('es-ES', {
        dateStyle: 'short',
        timeStyle: 'short'
    });
}

// Abrir el modal con datos de usuario
function abrirPopUpForm(json) {
    $("#txtid").val(0);
    if (json != null) {
        $("#txtid").val(json.IdUsuario);
        $("#txtNom_User").val(json.Nom_User);
        $("#inputCedula").val(json.Cedula_Usuario);
        $("#inputCedula").val(json.Cedula_Usuario).prop('readonly', true);
        $("#txtCorreo").val(json.Correo);
        $("#txtNom_Rol").val(json.Nom_Rol);
        $("#txtClave").val(json.Clave);
        $("#cboRol").val(json.IdRol);
        $("#cboEstado").val(json.Activo == true ? 1 : 0);
    } 
    $('#FormModal').modal('show');
}

// Guardar cambios
function Guardar() {
    if ($("#form").valid()) {
        var request = {
            objeto: {
                IdUsuario: $("#txtid").val(),
                Cedula_Usuario: $("#inputCedula").val(),
                Nom_Rol: $("#txtNom_Rol").val(),
                Nom_Completo: $("#txtNombre").val(),
                Nom_User: $("#txtNom_User").val(),
                Correo: $("#txtCorreo").val(),
                Clave: $("#txtClave").val(),
                IdRol: $("#cboRol").val(),
                Activo: parseInt($("#cboEstado").val()) == 1
            }
        };

        jQuery.ajax({
            url: $.MisUrls.url._GuardarUsuario,
            type: "POST",
            data: JSON.stringify(request),
            dataType: "json",
            contentType: "application/json; charset=utf-8",

            success: function (data) {
                if (data.resultado) {
                    tabladata.ajax.reload();
                    $('#FormModal').modal('hide');
                    swal("Éxito", "Información manipulada exitosamente", "success");
                } else {
                    swal("Mensaje", "No se pudo guardar los cambios, Verifica que el ID del usuario no exista en el sistema.", "warning");
                }
            },
            error: function (error) {
                console.log("Error en la solicitud:", error); // Mostrar cualquier error que ocurra durante la solicitud AJAX
                swal("Error", "Hubo un problema al intentar crear la cuenta.", "error");
            },
            error: function (error) {
                console.log(error);
            }
        });
    }
}

function eliminar(Cedula_Usuario) {
    swal({
        title: "Mensaje",
        text: "El usuario con ID: " + Cedula_Usuario + " será eliminado permanentemente. ¿Seguro que deseas continuar?",
        type: "warning",
        showCancelButton: true,
        confirmButtonText: "Si",
        confirmButtonColor: "#DD6B55",
        cancelButtonText: "No",
        closeOnConfirm: true
    },

        function () {
            jQuery.ajax({
                url: $.MisUrls.url._EliminarUsuario + "?Cedula_Usuario=" + Cedula_Usuario,
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
