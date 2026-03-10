$(document).ready(function () {
    $("#form").validate({
        rules: {
            txtPasswordNew: {
                required: true,
                minlength: 8
            },
            txtPasswordConfirm: {
                required: true,
                equalTo: "#txtPasswordNew"
            }
        },
        messages: {
            txtPasswordNew: {
                required: "(*) Ingresa una contraseña",
                minlength: "(*) La contraseña debe tener al menos 8 caracteres"
            },
            txtPasswordConfirm: {
                required: "(*) Confirma tu contraseña",
                equalTo: "(*) Las contraseñas no coinciden"
            }
        },
        errorElement: 'span',
        errorClass: 'text-danger',
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        }
    });
    $(document).ready(function () {
        // Obtenemos la cédula de la URL
        var urlParams = new URLSearchParams(window.location.search);
        var cedula = urlParams.get('Cedula_Solicitante');

        if (cedula) {
            $.ajax({
                url: "/Solicitantes/EditarUnSolicitante",
                type: 'GET',
                data: { Cedula_Solicitante: cedula },
                success: function (response) {
                    var solicitante = response.data;

                    if (solicitante) {
                        // Llenamos los campos del formulario con los datos del usuario
                        $("#txtid").val(solicitante.ID_Usuario);
                        $("#cbotipodocumento").val(solicitante.TipoDocumento);
                        $("#inputCedula").val(solicitante.Cedula_Solicitante);
                        $("#txtNombre").val(solicitante.Nombre);
                        $("#txtDireccion").val(solicitante.Direccion);
                        $("#inputTelefono").val(solicitante.Telefono);
                        $("#inputTelefono2").val(solicitante.Telefono2);
                        $("#inputFax").val(solicitante.Fax);
                        $("#txtCorreo").val(solicitante.Correo);
                        $("#cboEstado").val(solicitante.Activo ? 1 : 0);
                        $("#txtFechaRegistro").val(formatearFecha(solicitante.FechaRegistro));

                    } else {
                        alert("No se encontró un usuario con esa cédula.");
                    }
                },
                error: function (xhr, status, error) {
                    alert('Ocurrió un error al obtener los datos del usuario.');
                }
            });
        }
    });

});
function formatearFecha(fecha) {
    if (!fecha) return '';
    const timestamp = parseInt(fecha.replace(/\/Date\((\d+)\)\//, '$1'));
    const dateObj = new Date(timestamp);
    return dateObj.toLocaleString('es-ES', {
        dateStyle: 'short',
        timeStyle: 'short'
    });
}
function Guardar() {
    var request = {
        objeto: {
            ID_Usuario: $("#inputCedula").val(), // input hidden con el ID del usuario
            TipoDocumento: $("#cbotipodocumento").val(),
            Cedula_Solicitante: $("#inputCedula").val(),
            Nombre: $("#txtNombre").val(),
            Direccion: $("#txtDireccion").val(),
            Telefono: $("#inputTelefono").val(),
            Telefono2: $("#inputTelefono2").val(),
            Fax: $("#inputFax").val(),
            Correo: $("#txtCorreo").val(),
            Activo: parseInt($("#cboEstado").val()),
            FechaRegistro: $("#txtFechaRegistro").val() // Solo si lo necesitas enviar
        }
    };


    jQuery.ajax({
        url: $.MisUrls.url._GuardarSolicitantes,
        type: "POST",
        data: JSON.stringify(request),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.resultado) {
                $('#FormModal').modal('hide');
                swal("Éxito", "Información manipulada exitosamente", "success");
            } else {
                swal("Mensaje", "No se pudo guardar los cambios. Verifica los datos.", "warning");
            }
        },
        error: function (error) {
            console.log("Error en la solicitud:", error);
            swal("Error", "Hubo un problema al intentar guardar los datos.", "error");
        }
    });
}
function Cancelar() {
    location.reload(); // Recarga la página actual
}
