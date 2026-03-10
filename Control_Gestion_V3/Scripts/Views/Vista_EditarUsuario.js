
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







$(document).ready(function () {
    // Obtenemos la cédula de la URL
    var urlParams = new URLSearchParams(window.location.search);
    var Cedula_Usuario = urlParams.get('Cedula_Usuario');
    // Si no viene en la URL, obtenerlo desde el input oculto
    if (!Cedula_Usuario) {
        Cedula_Usuario = document.getElementById("cedulaUsuario").value;
    }

    
    if (Cedula_Usuario) {
        $.ajax({
            url: "/Usuario/ObtenerUnUsuario",
            type: 'GET',
            data: { Cedula_Usuario: Cedula_Usuario },
            success: function (response) {
                var usuario = response.data;

                if (usuario) {
                    // Llenamos los campos del formulario con los datos del usuario
                    $("#inputIdUsuario").val(usuario.IdUsuario);
                    $("#inputCedula").val(usuario.Cedula_Usuario);
                    $("#txtNombre").val(usuario.Nom_Completo);
                    $("#txtNombre_User").val(usuario.Nom_User);
                    $("#txtDireccion").val(usuario.Direccion);
                    $("#inputTelefono").val(usuario.Telefono1);
                    $("#inputTelefono2").val(usuario.Telefono2);
                    $("#inputFax").val(usuario.Fax);
                    $("#txtNom_Rol").val(usuario.Nom_Rol);
                    $("#txtCorreo").val(usuario.Correo);
                    $("#cboRol").val(usuario.IdRol);
                    //  $("#txtPasswordNuevo").val(usuario.Clave); 
                    $("#cboEstado").val(usuario.Activo == true ? 1 : 0);
                    $("#txtFechaRegistro").val(formatearFecha(usuario.FechaRegistro));
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
            IdUsuario: $("#inputIdUsuario").val(), 
            Cedula_Usuario: $("#inputCedula").val(),
            Nom_Rol: $("#txtNom_Rol").val(),
            Nom_Completo: $("#txtNombre").val(),
            Nom_User: $("#txtNombre_User").val(),
            Direccion: $("#txtDireccion").val(),
            Telefono1: $("#inputTelefono").val(),
            Telefono2: $("#inputTelefono2").val(),
            Fax: $("#inputFax").val(),
            Correo: $("#txtCorreo").val(),
            Clave: $("#inputPassword").val(), 
            IdRol: $("#cboRol").val(),
            Activo: parseInt($("#cboEstado").val()),
            // Activo: $("#cboEstado").val(),
            FechaRegistro: $("#txtFechaRegistro").val() // si lo usas
        }
    };

    jQuery.ajax({
        url: "/Usuario/Guardar",
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
